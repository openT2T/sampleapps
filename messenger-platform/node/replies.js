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
    }
}