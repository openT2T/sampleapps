// defines some basic prompts for the bot
module.exports = {

    notUnderstood: function () {
        return "Sorry, I don't know what that means... I'm still a baby bot!";
    },
    helpMessage: function () {
        return "Here's what I can do:\n\n" +
            "* Turn lights on/off by saying something like 'turn on the lights!'\n" +
            "* Control the temperature, or read the current temperature, by saying something like 'how warm is it?' or 'turn up the heat'\n";
    },
    canceled: function () {
        return 'Sure... No problem.';
    },
    powerState: function () {
        return "power power power";
    }
}