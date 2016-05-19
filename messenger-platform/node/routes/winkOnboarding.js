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
                // signed in, now set internal state
                var winkUser = JSON.parse(body);
                users.signIn(req.body.user.uid, winkUser.access_token);
                replies.sendTextMessage(req.body.user.uid, 'Signed in to Wink, thanks!');

                // import all lamp IDs so we can control them later
                var getOptions = {
                    protocol: 'https:',
                    host: 'api.wink.com',
                    path: '/users/me/wink_devices',
                    headers: {
                        'Authorization': 'Bearer ' + winkUser.access_token,
                        'Accept': 'application/json'
                    },
                    method: 'GET'
                };

                var winkReq = https.get(getOptions, function (winkRes) {
                    var body = '';
                    winkRes.setEncoding('utf8');
                    winkRes.on('data', function (data) {
                        body += data;
                    });

                    winkRes.on('end', function () {
                        if (winkRes.statusCode != 200) {
                            if (errorCallback) {
                                errorCallback(winkRes.statusCode, body);
                                return;
                            }
                        } else {
                            const idKeyFilter = 'light_bulb_id';
                            var devices = JSON.parse(body).data;

                            // apply the id key filter (lamps)
                            devices = devices.filter(function (device) {
                                return !!device[idKeyFilter];
                            });

                            if (!!devices && devices.length > 0) {
                                var deviceIds = devices.map(function (device) {
                                    return device[idKeyFilter];
                                });

                                // save device ids
                                users.setLampIds(req.body.user.uid, deviceIds);
                                
                                var lampsCount = deviceIds.length;
                                replies.sendTextMessage(req.body.user.uid, 'I can now control ' + lampsCount + ' lamps');
                
                                res.send('Signed in to Wink and discovered ' + lampsCount + ' lamps, thanks! You may close this window now and return to the bot that sent you here.');
                            } else {
                                if (errorCallback) {
                                    errorCallback('NotFound', 'No devices found.');
                                    return;
                                }
                            }
                        }
                    });

                    winkRes.on('error', function (e) {
                        if (errorCallback) {
                            errorCallback('enumerate', e.message);
                            return;
                        }
                    });
                });
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
