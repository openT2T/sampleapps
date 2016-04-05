using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.OpenT2T.Sample.SuperPopular.TemperatureSensor;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.AllJoyn;

namespace CortanaComponent
{
    public sealed class VoiceCommandHandler
    {
        TemperatureSensorWatcher m_Watcher;
        AllJoynBusAttachment m_Attachment;
        private List<TemperatureSensorConsumer> m_Sensors;

        public VoiceCommandHandler()
        {
            m_Sensors = new List<TemperatureSensorConsumer>();

            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();

            m_Watcher = new TemperatureSensorWatcher(m_Attachment);
            m_Watcher.Added += Sensor_Added;
        }

        private async void Sensor_Added(TemperatureSensorWatcher sender, AllJoynServiceInfo args)
        {
            TemperatureSensorJoinSessionResult joinSessionResult = await TemperatureSensorConsumer.JoinSessionAsync(args, sender);
            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                TemperatureSensorConsumer consumer = joinSessionResult.Consumer;
                m_Sensors.Add(consumer);
            }
        }

        private async Task ReportNoSensorsFoundAsync(VoiceCommandServiceConnection connection)
        {
            VoiceCommandResponse failureNoSensors = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "No sensors found", SpokenMessage = "Sorry I cannot see any sensors connected" });
            await connection.ReportFailureAsync(failureNoSensors);
        }

        internal async Task HandleQueryTemperatureCommand(VoiceCommandServiceConnection connection, VoiceCommand command)
        {
            if (m_Sensors.Count()>0)
            {
                await ReportTemperatureAsync(m_Sensors[0], connection);
            }
            else
            {
                await ReportNoSensorsFoundAsync(connection);
            }
        }

        private async Task ReportTemperatureAsync(TemperatureSensorConsumer consumer, VoiceCommandServiceConnection connection)
        {
            var getTempResult = await consumer.GetCurrentTemperatureAsync();
            if (getTempResult.Status == AllJoynStatus.Ok)
            {
                string text = String.Format("The temperature is {0:N1} degrees", getTempResult.Temp);
                VoiceCommandResponse tempResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage()
                { DisplayMessage = text, SpokenMessage = text });
                await connection.ReportSuccessAsync(tempResponse);
            }
            else
            {
                VoiceCommandResponse failure = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Failed to get temperature", SpokenMessage = "Sorry failed to get the temperature" });
                await connection.ReportFailureAsync(failure);
            }
        }

        internal async Task HandleQueryTrendCommand(VoiceCommandServiceConnection connection, VoiceCommand voiceCommand)
        {
            if (m_Sensors.Count() > 0)
            {

            }
            else
            {
                await ReportNoSensorsFoundAsync(connection);
            }
        }
    }
}
