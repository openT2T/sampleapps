/**
 * iothubReader code is based on the EventHub sample included in the ampq10 module
 * Note: the sample code has been changed to use enqueued time as filter option so it
 * starts reading the message stream on the EventHub starting from the current time
 * To read from the beginning of the stream set the filterOption param in the config.json file 
 * to -1 
 */
'use strict';
var Promise = require('bluebird'),
    AMQPClient  = require('amqp10').Client,
    Policy = require('amqp10').Policy,
    translator = require('amqp10').translator;

module.exports = {
reader: function(settings, messageHandler, errorHandler) {

// Set the offset for the EventHub - this is where it should start receiving from, and is typically different for each partition
// Here, I'm setting a global offset, just to show you how it's done. See node-sbus-amqp10 for a wrapper library that will
// take care of this for you.

var filterOption; // todo:: need a x-opt-offset per partition.
// start reading the enqueued messages from now
var filterOffset

if (settings.offsetFilter) {
  var filter = settings.offsetFilter === "now" ? new Date().getTime() : settings.offsetFilter;  
  filterOption = {
    attach: { source: { filter: {
      'apache.org:selector-filter:string': translator(
        ['described', ['symbol', 'apache.org:selector-filter:string'], ['string', "amqp.annotation.x-opt-enqueuedtimeutc > '" + filter + "'"]])
    } } }
  };
}

if (!settings.serviceBusHost || !settings.eventHubName || !settings.SASKeyName || !settings.SASKey || !settings.partitions) {
  console.warn('Must provide either settings json file or appropriate environment variables.');
  process.exit(1);
}

var protocol = settings.protocol || 'amqps';
var serviceBusHost = settings.serviceBusHost + '.servicebus.windows.net';
if (settings.serviceBusHost.indexOf(".") !== -1) {
  serviceBusHost = settings.serviceBusHost;
}
var sasName = settings.SASKeyName;
var sasKey = settings.SASKey;
var eventHubName = settings.eventHubName;
var numPartitions = settings.partitions;

var uri = protocol + '://' + encodeURIComponent(sasName) + ':' + encodeURIComponent(sasKey) + '@' + serviceBusHost;
var recvAddr = eventHubName + '/ConsumerGroups/$default/Partitions/';


var client = new AMQPClient(Policy.EventHub);

function range(begin, end) {
  return Array.apply(null, new Array(end - begin)).map(function(_, i) { return i + begin; });
}

var createPartitionReceiver = function(curIdx, curRcvAddr, filterOption) {
  return client.createReceiver(curRcvAddr, filterOption)
    .then(function (receiver) {
      receiver.on('message', messageHandler.bind(null, curIdx));
      receiver.on('errorReceived', errorHandler.bind(null, curIdx));
    });
};

client.connect(uri)
  .then(function () {
    return Promise.all([

      // TODO:: filterOption-> checkpoints are per partition.
      Promise.map(range(0, numPartitions), function(idx) {
        return createPartitionReceiver(idx, recvAddr + idx, filterOption);
      })
    ]);
  })
  .error(function (e) {
    console.warn('connection error: ', e);
  });
 }
}