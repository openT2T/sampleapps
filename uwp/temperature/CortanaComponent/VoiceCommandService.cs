using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace CortanaComponent
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        static VoiceCommandHandler g_CommandHandler;

        static VoiceCommandService()
        {
            g_CommandHandler = new VoiceCommandHandler();
        }

        const string cortanaFamilyId = "Microsoft.Windows.Cortana_cw5n1h2txyewy";


        BackgroundTaskDeferral m_VoiceServiceDeferral;
        VoiceCommandServiceConnection m_VoiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_VoiceServiceDeferral = taskInstance.GetDeferral();

            AppServiceTriggerDetails triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                taskInstance.Canceled += VoiceTaskInstance_Canceled;

                if (triggerDetails.CallerPackageFamilyName == cortanaFamilyId)
                {
                    // Being called from Cortana
                    m_VoiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    m_VoiceServiceConnection.VoiceCommandCompleted += VoiceServiceConnection_VoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await m_VoiceServiceConnection.GetVoiceCommandAsync();


                    switch (voiceCommand.CommandName)
                    {
                        case "queryTemperature":
                            await g_CommandHandler.HandleQueryTemperatureCommand(m_VoiceServiceConnection,voiceCommand);
                            break;
                        case "queryTrend":
                            await g_CommandHandler.HandleQueryTrendCommand(m_VoiceServiceConnection, voiceCommand);
                            break;
                    }
                }

            }
        }

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (m_VoiceServiceDeferral != null)
            {
                m_VoiceServiceDeferral.Complete();
                m_VoiceServiceDeferral = null;
                m_VoiceServiceConnection = null;
            }
        }

        private void VoiceTaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (m_VoiceServiceDeferral != null)
            {
                m_VoiceServiceDeferral.Complete();
                m_VoiceServiceDeferral = null;
            }
        }
    }
}