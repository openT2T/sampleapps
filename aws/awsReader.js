/**
 * awsReader uses Kinesis Module to create a stream with the events submitted to Kinesis
 * The processing pipeline is composed of two stages:
 *  bufferify: from a record, extracts the payload (Data) and pass it to the next stage in the stream
 *  handler: converts the string into an object and pass it to the ruleEngine handler to execute the rules defined
 *  by the user.
 */
'use strict';
var fs = require('fs'),
    Transform = require('stream').Transform,
    kinesis = require('kinesis'),
    KinesisStream = kinesis.KinesisStream

module.exports = {

    reader: function(settings, messageHandler, errorHandler) {
        var kinesisSource = kinesis.stream(settings);

        // Data is retrieved as Record objects, so let's transform into Buffers
        var bufferify = new Transform({ objectMode: true })
        bufferify._transform = function(record, encoding, cb) {
            cb(null, record.Data)
        }
        /**
         * for debugging only: logs the event on the console
         */
        var log = new Transform({ objectMode: true });
        log._transform = function(record, encoding, cb) {
            console.log("   ==>" + record.toString());
            cb(null, record);
        };
        var handler = new Transform({ objectMode: true })
        handler._transform = function(record, encoding, cb) {
            messageHandler(JSON.parse(record.toString()));
            cb();
        }

        //kinesisSource.pipe(bufferify).pipe(log).pipe(handler); //for debugging
        kinesisSource.pipe(bufferify).pipe(handler);
    }
}

