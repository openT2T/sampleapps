var express = require('express');
var router = express.Router();
var replies = require('../replies');
var users = require('../users');

/* GET */
router.get('/', function (req, res) {

  if (req.query['hub.verify_token'] === 'Sup3rS3cr3t') {
    res.send(req.query['hub.challenge']);
  }
  res.send('Error, wrong validation token');
});

/* POST: main entry point that handles incoming messages */
router.post('/', function (req, res) {

  messaging_events = req.body.entry[0].messaging;
  for (i = 0; i < messaging_events.length; i++) {
    event = req.body.entry[0].messaging[i];
    sender = event.sender.id;
    if (!!users.getToken(sender)) {

      console.log('Event: ' + JSON.stringify(event));

      if (event.message && event.message.text) {
        text = event.message.text;

        if (text === 'Structured') {
          // Handled a structured message request
          replies.sendStructuredMessage(sender);
          continue;
        } else {
          // Handle a text message from this sender
          replies.sendTextMessage(sender, 'Text received, echo: ' + text.substring(0, 200));
        }
      }

      if (event.postback) {
        text = JSON.stringify(event.postback);
        replies.sendTextMessage(sender, 'Postback received: ' + text.substring(0, 200));
        
        continue;
      }
    } else {
      replies.sendSignInMessage(sender);
      break;
    }
  }

  res.sendStatus(200);
});

module.exports = router;
