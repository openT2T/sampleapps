var system = null;
module.exports = {
    init:function(devCollection) {
      system = devCollection;  
    },
    
    checkTemperatureInLivingRoom:function(event){
      //if it is a temperature sensor
      if(event.device.type.indexOf("org.openT2T.sample.superPopular.temperatureSensor") != -1) {
      if(event.state.temperature > 80) {
        console.log("==> Temperature is over 100 turning off light");
        system.myHome.LivingRoom.Light.device.turnOff();
      }
      }
    },
   turnOffLights(event) {
       var eitherLights = event.device.id === "light1" || event.device.id === "consoleLight";
       if(eitherLights && event.state.light === "On") { 
       system.myHome.LivingRoom.Light.device.turnOff();
       system.myHome.LivingRoom.ConsoleLight.device.turnOff();
       }
   }
}   