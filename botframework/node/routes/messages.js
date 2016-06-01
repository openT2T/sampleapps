'use strict';

const express = require('express');
const router = express.Router();
const replies = require('../replies');
const users = require('../users');
const settings = require('../config/config.json');
const builder = require('botbuilder');

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ appId: 'YourAppId', appSecret: 'YourAppSecret' });
bot.add('/', function (session) {
    session.send('Hello World');
});

/* POST: main entry point that handles incoming messages */
router.post('/', bot.verifyBotFramework(), bot.listen());

module.exports = router;
