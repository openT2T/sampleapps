# openT2T cloud-based RuleEngine Sample app
This is a sample app that shows how openT2T thingTranslators can be used to interact with multiple devices using a common schema.
The app receives events from Azure IoTHub and uses a ruleEngine to execute some rules. Rules are based on a predefined device hierachy that the user can create in order to define how devices are organized in logical groups, ex. LivingRoom, Kitchen, ...
This sample app also show how the same thingTranslator handler can be used on a cloud-based application (clearly with the assumption that the cloud-based application can connect to the device somehow).

## Device Collection & Device Profiles
In order to define groups of devices that can then be used to write simple rules using the thingTranslator's schemas the app needs two files:
* device collection file: this is the file that is used to define the hierarchy of devices so they can be easily identified in the rules, for instance, LivingRoom.Light1, LivingRoom.Thermostat, Kitchen.HumiditySensor, ...

 ```json
{
    "LivingRoom": {
        "Light": {
            "deviceId": "light1"
        },
        "ConsoleLight": {
            "deviceId": "consoleLight1"
        },
        "SensorTag": {
            "deviceId": "sensorTag1"
        }
    }
}
```
The only required property is `deviceId` that is used to identity the device properties in the device profiles file.
    
* device profiles file: the file contains the properties that can be used to connect to the device (maybe via a 3rd party service such as WINK).

```json
[
    {
        "deviceId": "light1",
        "deviceType": "org.openT2T.sample.superPopular.lamp/Test Light",
        "initDevice": {
            "name": "WINK",
            "controlId": "<device controlId>",
            "controlString": "<device control String>",
            "props": "<device props`>"
        }
    },
       {
        "deviceId": "consoleLight1",
        "deviceType": "org.openT2T.sample.superPopular.lamp/Test Light",
        "initDevice": {
            "name": "WINK",
            "controlId": "<device controlId>",
            "controlString": "<device control String>",
            "props": "<device props`>"
        }
    },
       {
        "deviceId": "sensorTag1",
        "deviceType": "org.openT2T.sample.superPopular.temperatureSensor/Test Thermostat",
        "initDevice": {
            "name": "Test Thermostat",
            "controlId": "<device controlId>",
            "controlString": "<device control String>",
            "props": "<device props`>"
        }
    }
]
```     
The device profiles file is an array of objects with the following properties:
* `deviceId`: it is the property that is used in the Device Collection file to uniquely indentify the device
* ` deviceType`: defines the type of device according to the openT2T translators schema
* `initDevice`: defines the set of properties that are required to connect to the device either directly or via a 3rd party service
    
## config file structure

* deviceRegistryUrl: url of the openT2T's Translators repo (it should be set to https://github.com/openT2T/translators.git    
* repoDir: local directory where the Translators repo is going to be cloned
* iothub: these are the params that are used to connect to Azure IoTHub. All parameters are available through Azure Portal
 * SASKeyName: this is usually set to 'service'
 * SASKey: The key can be found in the Azure Portal, it needs to match the SASKeyName so it needs to be the key for the 'service' profile.
 * serviceBusHost: this is the name of iothub hostname, it can be found in the Azure Portal under 'EventHub compatible name'  
 * eventHubName: IoTHub name, it is the name used when the IoTHub is created.
 * partitions: number of partitions in the IoTHub. By default IoTHub is created with 4 partitions.
 * offsetFilter: offset to use when reading the IoTHub event stream. "now" means events are used based on datetime so only future events are read. To read from the beginning use -1
    
