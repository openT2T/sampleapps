'use strict';

const express = require('express');
const router = express.Router();
const replies = require('../replies');
const users = require('../users');
const conversations = require('../conversations');
var settings = require('../config/config.json');

/* GET */
router.get('/', function (req, res) {
  if (req.query['hub.mode'] === 'subscribe' &&
    req.query['hub.verify_token'] === settings.facebook.verifyToken) {
    res.send(req.query['hub.challenge']);
  } else {
    res.sendStatus(400);
  }
});

/* POST: main entry point that handles incoming messages */
router.post('/', function (req, res) {

  var messaging_events = req.body.entry[0].messaging;
  for (var i = 0; i < messaging_events.length; i++) {
    var event = req.body.entry[0].messaging[i];
    var senderId = event.sender.id;
    
    console.log('senderId: ' + senderId);
    console.log('token: ' + users.getToken(senderId));
    console.log('notified: ' + users.getSignInNotified(senderId));
    
    if (!!users.getToken(senderId)) {

      users.setSignInNotified(senderId, false);
      console.log('Event: ' + JSON.stringify(event));

      if (event.message && event.message.text) {

        // Yay! We got a new message!
        console.log('Got a message...');
        conversations.processMessageEntry(senderId, event.message);
      }

      if (event.postback) {
        text = JSON.stringify(event.postback);
        replies.sendTextMessage(senderId, 'Postback received: ' + text.substring(0, 200));

        continue;
      }
    } else if (!users.getSignInNotified(senderId)) {
      replies.sendSignInMessage(senderId);
      users.setSignInNotified(senderId, true);
      
      break;
    }
  }

  res.sendStatus(200);
});

module.exports = router;
