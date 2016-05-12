var request = require('request');
var settings = require('./settings');

var sendMessageData = function (recipient, messageData) {
    request({
        url: 'https://graph.facebook.com/v2.6/me/messages',
        qs: { access_token: settings.PAGE_ACCESS_TOKEN },
        method: 'POST',
        json: {
            recipient: { id: recipient },
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
    sendTextMessage: function (recipient, text) {

        console.log('Sending text message...');

        var messageData = {
            text: text
        };

        sendMessageData(recipient, messageData);
    },

    sendSignInMessage: function (recipient) {

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
                                    "url": settings.SERVER_URL + '/wink?uid=' + recipient,
                                    "title": "Sign In"
                                }
                            ],
                        }
                    ]
                }
            }
        };

        sendMessageData(recipient, messageData);
    },

    sendStructuredMessage: function (recipient) {

        console.log('Sending structured message...');

        var messageData = {
            "attachment": {
                "type": "template",
                "payload": {
                    "template_type": "generic",
                    "elements": [
                        {
                            "title": "First card",
                            "subtitle": "Element #1 of an hscroll",
                            "image_url": "http://messengerdemo.parseapp.com/img/rift.png",
                            "buttons": [
                                {
                                    "type": "web_url",
                                    "url": "https://www.messenger.com/",
                                    "title": "Web url"
                                },
                                {
                                    "type": "postback",
                                    "title": "Postback",
                                    "payload": "Payload for first element in a generic bubble",
                                }
                            ],
                        },
                        {
                            "title": "Second card",
                            "subtitle": "Element #2 of an hscroll",
                            "image_url": "http://messengerdemo.parseapp.com/img/gearvr.png",
                            "buttons": [
                                {
                                    "type": "postback",
                                    "title": "Postback",
                                    "payload": "Payload for second element in a generic bubble",
                                }
                            ],
                        }
                    ]
                }
            }
        };

        sendMessageData(recipient, messageData);
    }
}