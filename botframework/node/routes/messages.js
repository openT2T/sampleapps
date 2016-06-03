'use strict';

const express = require('express');
const router = express.Router();
const replies = require('../replies');
const users = require('../users');
const settings = require('../config/config.json');
const builder = require('botbuilder');
const conversations = require('../conversations');

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ appId: settings.botframework.applicationId, appSecret: settings.botframework.applicationSecret });
bot.add('/', conversations);

/* POST: main entry point that handles incoming messages */
router.post('/', bot.verifyBotFramework(), bot.listen());

module.exports = router;
