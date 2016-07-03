const builder = require('botbuilder');
const prompts = require('./prompts');
const settings = require('./config/config.json');

/** Return a LuisDialog that points at our model and then add intent handlers. */
var model = 'https://api.projectoxford.ai/luis/v1/application?id=' + settings.luis.applicationId + '&subscription-key=' + settings.luis.subscriptionKey + '&q=';
var dialog = new builder.LuisDialog(model);
module.exports = dialog;

/** Answer unrecognized requests.  */
dialog.on('None', function (session, args, next) {
    console.log('** None Called');

    session.send(prompts.notUnderstood())
});

/** Answer users help requests. */
dialog.on('Help', function (session, args, next) {
    console.log('** Help Called');

    session.send(prompts.helpMessage())
});

/** Prompts a user for the power state and then sets it for the required device.  */
dialog.on('SetPowerState',
    function (session, args, next) {

        console.log('** SetPowerState Called');

        // See if we got the power state from our LUIS model.
        var powerState = builder.EntityRecognizer.findEntity(args.entities, 'powerState');

        if (!!powerState) {
            // act
            session.send(prompts.powerState());
        }
        else {
            session.send(prompts.notUnderstood())
        }
    }
);
