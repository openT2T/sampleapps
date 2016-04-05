using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using org.OpenT2T.Sample.SuperPopular.Lamp;
using Windows.Devices.AllJoyn;

namespace CortanaComponent
{
    public sealed class VoiceCommandHandler
    {
        LampWatcher m_Watcher;
        AllJoynBusAttachment m_Attachment;
        List<LampConsumer> m_Lamps;

        public VoiceCommandHandler()
        {
            m_Lamps = new List<LampConsumer>();

            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();

            m_Watcher = new LampWatcher(m_Attachment);
            m_Watcher.Added += Lamp_Added;
        }

        private async void Lamp_Added(LampWatcher sender, AllJoynServiceInfo args)
        {
            LampJoinSessionResult joinSessionResult = await LampConsumer.JoinSessionAsync(args, sender);
            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                m_Lamps.Add(joinSessionResult.Consumer);
                joinSessionResult.Consumer.SessionLost += Consumer_SessionLost;
            }
        }

        private void Consumer_SessionLost(LampConsumer sender, AllJoynSessionLostEventArgs args)
        {
            m_Lamps.Remove(sender);
        }

        internal async Task<bool> SetAllOnAsync()
        {
            List<Task<LampTurnOnResult>> tasks = new List<Task<LampTurnOnResult>>();
            foreach (var lamp in m_Lamps)
            {
                tasks.Add(lamp.TurnOnAsync().AsTask());
            }
            var results = await Task.WhenAll(tasks);
            return results.All(r => r.Status == AllJoynStatus.Ok);
        }

        internal async Task<bool> SetAllOffAsync()
        {
            List<Task<LampTurnOffResult>> tasks = new List<Task<LampTurnOffResult>>();
            foreach (var lamp in m_Lamps)
            {
                tasks.Add(lamp.TurnOffAsync().AsTask());
            }
            var results = await Task.WhenAll(tasks);
            return results.All(r => r.Status == AllJoynStatus.Ok);
        }
    }
}
