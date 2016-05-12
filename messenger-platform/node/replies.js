var request = require('request');
var creds = require('./creds');

module.exports = {
    sendTextMessage: function (sender, text) {

        console.log('Sending text message...');

        var messageData = {
            text: text
        };

        request({
            url: 'https://graph.facebook.com/v2.6/me/messages',
            qs: { access_token: creds.PAGE_ACCESS_TOKEN },
            method: 'POST',
            json: {
                recipient: { id: sender },
                message: messageData,
            }
        }, function (error, response, body) {
            if (error) {
                console.log('Error sending message: ', error);
            } else if (response.body.error) {
                console.log('Error: ', response.body.error);
            }
        });
    },

    sendStructuredMessage: function (sender) {

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

        request({
            url: 'https://graph.facebook.com/v2.6/me/messages',
            qs: { access_token: creds.PAGE_ACCESS_TOKEN },
            method: 'POST',
            json: {
                recipient: { id: sender },
                message: messageData,
            }
        }, function (error, response, body) {
            if (error) {
                console.log('Error sending message: ', error);
            } else if (response.body.error) {
                console.log('Error: ', response.body.error);
            }
        });
    }
}