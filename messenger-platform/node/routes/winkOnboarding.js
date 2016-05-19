'use strict';

var https = require('https');
var express = require('express');
var router = express.Router();
var request = require('request');
var settings = require('../config/config.json');
var users = require('../users');
var replies = require('../replies');

/* GET */
router.get('/', function (req, res) {
    var uid = req.query['uid'];

    if (!!uid) {
        // if the uid is sent, render the wink view
        res.render('winkOnboarding', { uid: uid });
    } else {
        res.render('error', { message: 'Missing UID', error: {} });
    }
});

/* POST: (with username and password in body)
   Initiates sign in to the WINK service for this uid
 */
router.post('/', function (req, res) {

    var postData = JSON.stringify({
        'client_id': settings.wink.clientId,
        'client_secret': settings.wink.clientSecret,
        'username': req.body.user.email,
        'password': req.body.user.password,
        'grant_type': 'password'
    });

    var postOptions = {
        protocol: 'https:',
        host: 'api.wink.com',
        path: '/oauth2/token',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': postData.length
        },
        method: 'POST'
    };

    // set up sign in request
    var signInRequest = https.request(postOptions, (signInResponse) => {

        var body = '';
        signInResponse.setEncoding('utf8');
        signInResponse.on('data', function (data) {
            body += data;
        });

        signInResponse.on('end', function () {

            if (signInResponse.statusCode != 200) {
                res.send('Error (failure status code ' + signInResponse.statusCode + ' ): ' + body);
            }
            else {
                // signed in, now enumerate devices and let the user pick one
                var winkUser = JSON.parse(body);
                
                users.signIn(req.body.user.uid, winkUser.access_token);
                
                replies.sendTextMessage(req.body.user.uid, 'Signed in to Wink, thanks! Now I can do stuff.');
                
                res.send('Signed in to Wink, thanks! You may close this window now and return to the bot that sent you here.');
            }
        });

        signInResponse.on('error', function (e) {
            res.send('Error (response error event): ' + JSON.stringify(e));
        });

    });

    signInRequest.on('error', (e) => {
        res.send('Error (request error event): ' + JSON.stringify(e)); Ó
    });

    // initiate sign in request
    signInRequest.write(postData);
    signInRequest.end();
});

module.exports = router;
