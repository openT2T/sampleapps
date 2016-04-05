var system = null;
var ruleExecuted = 0;
module.exports = {

    init: function(devCollection) {
        system = devCollection;
    },

    checkTemperatureInLivingRoom: function(event) {
        //if it is a temperature sensor
        if (event.device.type.indexOf("org.openT2T.sample.superPopular.temperatureSensor") != -1) {
            if (event.state.temperature > 80) {
                console.log("==> Temperature is over 100 turning off light");
                system.myHome.LivingRoom.Light.device.turnOff();
            }
        }
        ruleExecuted += 1;
        console.log("    Executed:" + ruleExecuted.toString());
    },
    turnOffLights(event) {
        var eitherLights = event.device.id === "light1" || event.device.id === "consoleLight";
        if (eitherLights && event.state.light === "On") {
            system.myHome.LivingRoom.Light.device.turnOff();
            system.myHome.LivingRoom.ConsoleLight.device.turnOff();
        }
        ruleExecuted += 1;
        console.log("    Executed:" + ruleExecuted.toString());

    }

}   