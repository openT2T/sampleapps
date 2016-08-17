var fs = require("fs");
var git = require("./git");
var ruleEngine = require("./ruleEngine");

/**
 * A simple wrapper that takes care of initializing the ruleEngine
 * 
 */
var settings = {};
var deviceCollection = null;

var filterEvents = function(eventHandler) {

    return function(idx, event) {
        //filter event
        var data = {

            device: {
                id: 'light1',
                type: 'org.openT2T.sample.superPopular.lamp'
            },
            state: {
                light: 'On',
                brightness: 5
            }
        };
        console.log("--> received:"+JSON.stringify(event.body));
        if (event.body.hasOwnProperty("device") && event.body.hasOwnProperty("state")) {
            console.log("****----> ready to process:" + +JSON.stringify(event.body));
            eventHandler(event.body);
        }
    }
}

module.exports = {
    /**
     * Initialize the ruleEngine
     * input parameters:
     *  configfile: location of the config file
     *  dCollection: the name of the file that contains the device groups that are going to be used
     *  in the rules
     *  initCompleteCallback: callback function that is going to be called when the Translators repo
     *  is succeffully cloned on a local directory.
     */
    init: function(configFile, dCollection, initCompletedCallback) {

        deviceCollection = dCollection;
        //check if config file exists
        if (fs.existsSync(configFile)) {
            settings = require(configFile);
            // clone the translators repo
            git.clone(settings.repoDir, settings.deviceRegistryUrl, function() {
                initCompletedCallback();
            });
        } else {
            var err = "cannot find config file" + settingsFile;
            console.error(err);
            initCompleteCallback(err);
        }
    },
    startPolling: function() {
        ruleEngine.init(deviceCollection, settings.repoDir)
        ruleEngine.startPolling(settings.pullInterval);
    },

/**
 * Register an eventHandler that is used by an event reader to send events to the ruleEngine
 */
    eventHandler: function(source, err) {
        ruleEngine.init(deviceCollection, settings.repoDir)
        source.reader(settings.iothub, filterEvents(ruleEngine.processEvent), this.errorHandler);
    },

/**
 * can be used to register an handler in case the event Reader (for instance from Azure IoTHub) returns an error
 */
    errorHandler: function(err) {
        console.error(err);
    }

}
