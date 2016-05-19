
'use strict'

const Wit = require('node-wit').Wit;
const settings = require('./config/config.json');
const replies = require('./replies');
const users = require('./users');

// This will contain all user sessions.
// Each session has an entry:
// sessionId -> {fbid: facebookUserId, context: sessionState}
const sessions = {};

const findOrCreateSession = (fbid) => {
  let sessionId;
  // Let's see if we already have a session for the user fbid
  Object.keys(sessions).forEach(k => {
    if (sessions[k].fbid === fbid) {
      // Yep, got it!
      sessionId = k;
    }
  });
  if (!sessionId) {
    // No session found for user fbid, let's create a new one
    sessionId = new Date().toISOString();
    sessions[sessionId] = { fbid: fbid, context: {} };
  }
  return sessionId;
};

const firstEntityValue = (entities, entity) => {
  const val = entities && entities[entity] &&
    Array.isArray(entities[entity]) &&
    entities[entity].length > 0 &&
    entities[entity][0].value
    ;
  if (!val) {
    return null;
  }
  return typeof val === 'object' ? val.value : val;
};

// Our bot actions
const actions = {

  say(sessionId, context, message, cb) {
    // Our bot has something to say!
    // Let's retrieve the Facebook user whose session belongs to
    const recipientId = sessions[sessionId].fbid;
    if (recipientId) {
      // Yay, we found our recipient!
      // Let's forward our bot response to her.
      replies.sendTextMessage(recipientId, message);
      cb();
    } else {
      console.log('Oops! Couldn\'t find user for session:', sessionId);
      // Giving the wheel back to our bot
      cb();
    }
  },

  merge(sessionId, context, entities, message, cb) {

    // Retrieve the thing entity and store it into a context field
    const thing = firstEntityValue(entities, 'thing');
    if (!!thing) {
      context.thing = thing;
    }

    // Retrieve the on_off entity and store it into a context field
    const on_off = firstEntityValue(entities, 'on_off');
    if (!!on_off) {
      context.on_off = on_off;
    }

    cb(context);
  },

  error(sessionId, context, error) {
    console.log(error.message);
  },

  // Bot custom actions
  // See https://wit.ai/docs/quickstart
  processStateCommand(sessionId, context, cb) {

    console.log('*** processStateCommand: ' + JSON.stringify(context));

    // Let's retrieve the Facebook user whose session belongs to
    const uid = sessions[sessionId].fbid;
    var lampIds = users.getLampIds(uid);
    var token = users.getToken(uid);

    console.log('uid: ' + uid);
    console.log('lampIds: ' + JSON.stringify(lampIds));
    console.log('token: ' + token);

    for (var i = 0; i < lampIds.length; i++) {
      var translator = require('./' + settings.repoDir + '/org.OpenT2T.Sample.SuperPopular.Lamp/Wink\ Light\ Bulb/js/thingTranslator');

      // device object passed in as context to the translator
      function Device(deviceId, accessToken) {
        this.props = ' { "id": "' + deviceId + '", "access_token": "' + accessToken + '" }';
        this.name = "Wink Light Bulb";
      }

      var device = new Device(lampIds[i], token);

      // initialize the translator for this device
      translator.initDevice(device);

      switch (context.on_off) {
        case "on":
          translator.turnOn();
          break;
        case "off":
          translator.turnOff();
          break;
      }
    }

    cb(context);
  }
};

// Setting up our bot
const wit = new Wit(settings.facebook.witToken, actions);

module.exports = {

  // process incoming message
  processMessageEntry: function (senderId, message) {

    console.log('Processing message entry...');

    // We retrieve the user's current session, or create one if it doesn't exist
    // This is needed for our bot to figure out the conversation history
    const sessionId = findOrCreateSession(senderId);

    // We retrieve the message content
    const msg = message.text;
    const atts = message.attachments;


    if (atts) {
      // We received an attachment

      // Let's reply with an automatic message
      replies.sendTextMessage(senderId, 'Sorry I can only process text messages for now.');

    } else if (msg) {
      // We received a text message

      // Let's forward the message to the Wit.ai Bot Engine
      // This will run all actions until our bot has nothing left to do
      wit.runActions(
        sessionId, // the user's current session
        msg, // the user's message 
        sessions[sessionId].context, // the user's current session state
        (error, context) => {
          if (error) {
            console.log('Oops! Got an error from Wit:', error);
          } else {
            // Our bot did everything it has to do.
            // Now it's waiting for further messages to proceed.
            console.log('Waiting for futher messages.');

            // Based on the session state, you might want to reset the session.
            // This depends heavily on the business logic of your bot.
            // Example:
            // if (context['done']) {
            //   delete sessions[sessionId];
            // }

            // Updating the user's current session state
            sessions[sessionId].context = context;
          }
        }
      );
    }
  }
}