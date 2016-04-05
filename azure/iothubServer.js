var opent2t = require("./opent2t");
var iothubReader = require("./iothubReader")
/**
 * Sample of a server that use Azure's IoTHub to read events and execute a set of predefined rules
 * Once the ruleEngine is initialized, it registers an eventHandler to process events from IoTHub.
 * Rules defined in the rules/ directory are executed and the event (message Data field (payload) is pass into the rule) 
 * Rules use APIs available in the thingTranslator file are then used to interact with the device
 * without having to know the specifics on how to conneect and send a command. 
 */
opent2t.init(process.argv[2], process.argv[3], function(err) {
    if(err)
        console.error(err);
     else {
        console.log("launching ruleEngine ...."); 
        opent2t.eventHandler(iothubReader);
     }
     
});

