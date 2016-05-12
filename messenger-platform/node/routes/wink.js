'use strict';

var https = require('https');
var express = require('express');
var router = express.Router();
var request = require('request');
var settings = require('../settings');
var users = require('../users');

/* GET */
router.get('/', function (req, res) {
    var uid = req.query['uid'];

    if (!!uid) {
        res.render('wink', { uid: uid });
    } else {
        res.send('Error, missing uid');
    }
});

/* POST */
router.post('/', function (req, res) {

    var postData = JSON.stringify({
        'client_id': settings.WINK_CLIENT_ID,
        'client_secret': settings.WINK_CLIENT_SECRET,
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
                res.send('Error (failure status code): ' + JSON.stringify(signInResponse));
            }
            else {
                // signed in, now enumerate devices and let the user pick one
                var winkUser = JSON.parse(body);
                
                users.signIn(req.body.user.uid, winkUser.access_token);
                
                res.send('Signed in to Wink, thanks. You may close this window now and return to the bot that sent you here.');
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
