'use strict';

const request = require('request');
const settings = require('./config/config.json');

const sendMessageData = function (recipientId, messageData) {
    request({
        url: 'https://graph.facebook.com/v2.6/me/messages',
        qs: { access_token: settings.facebook.pageAccessToken },
        method: 'POST',
        json: {
            recipient: { id: recipientId },
            message: messageData,
        }
    }, function (error, response, body) {
        if (error) {
            console.log('Error sending message: ', error);
        } else if (response.body.error) {
            console.log('Error: ', response.body.error);
        }
    });
};

module.exports = {
    sendTextMessage: function (recipientId, text) {

        console.log('Sending text message...');

        var messageData = {
            text: text
        };

        sendMessageData(recipientId, messageData);
    },

    sendSignInMessage: function (recipientId) {

        console.log('Sending sign in message...');

        var messageData = {
            "attachment": {
                "type": "template",
                "payload": {
                    "template_type": "generic",
                    "elements": [
                        {
                            "title": "Looks like you're not signed in to Wink",
                            "subtitle": "I can control stuff via Wink. Give me access, please!",
                            "image_url": "http://www.wink.com/img/product/wink-hub/variants/840410102358/hero_01.png",
                            "buttons": [
                                {
                                    "type": "web_url",
                                    "url": settings.serverUrl + '/winkOnboarding?uid=' + recipientId,
                                    "title": "Sign In"
                                }
                            ],
                        }
                    ]
                }
            }
        };

        sendMessageData(recipientId, messageData);
    }
}