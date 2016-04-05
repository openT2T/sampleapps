using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using org.OpenT2T.Sample.SuperPopular.Shades;

namespace CortanaComponent
{
    public sealed class VoiceCommandHandler
    {
        ShadesWatcher m_Watcher;
        AllJoynBusAttachment m_Attachment;
        List<ShadesConsumer> m_Shades;

        public VoiceCommandHandler()
        {
            m_Shades = new List<ShadesConsumer>();

            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();

            m_Watcher = new ShadesWatcher(m_Attachment);
            m_Watcher.Added += Shade_Added;
        }

        private async void Shade_Added(ShadesWatcher sender, AllJoynServiceInfo args)
        {
            ShadesJoinSessionResult joinSessionResult = await ShadesConsumer.JoinSessionAsync(args, sender);
            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                m_Shades.Add(joinSessionResult.Consumer);
                joinSessionResult.Consumer.SessionLost += Consumer_SessionLost;
            }
        }

        private void Consumer_SessionLost(ShadesConsumer sender, AllJoynSessionLostEventArgs args)
        {
            m_Shades.Remove(sender);
        }

        internal async Task<bool> OpenAllAsync()
        {
            List<Task<ShadesOpenResult>> tasks = new List<Task<ShadesOpenResult>>();
            foreach (var shade in m_Shades)
            {
                tasks.Add(shade.OpenAsync().AsTask());
            }
            var results = await Task.WhenAll(tasks);
            return results.All(r => r.Status == AllJoynStatus.Ok);
        }

        internal async Task<bool> CloseAllAsync()
        {
            List<Task<ShadesCloseResult>> tasks = new List<Task<ShadesCloseResult>>();
            foreach (var shade in m_Shades)
            {
                tasks.Add(shade.CloseAsync().AsTask());
            }
            var results = await Task.WhenAll(tasks);
            return results.All(r => r.Status == AllJoynStatus.Ok);
        }
    }
}


