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
        BackgroundTaskDeferral m_Deferral;
        private VoiceCommandServiceConnection m_VoiceServiceConnection;
        const string cortanaFamilyId = "Microsoft.Windows.Cortana_cw5n1h2txyewy";

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_Deferral = taskInstance.GetDeferral();

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
                        case "open":
                            var onProgressResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Opening all shades", SpokenMessage = "Please wait until I open all shades" });
                            await m_VoiceServiceConnection.ReportProgressAsync(onProgressResponse);

                            if (await g_CommandHandler.OpenAllAsync())
                            {
                                // All operations succeeded
                                var successCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Shades are now open", SpokenMessage = "Shades are now open" });
                                await m_VoiceServiceConnection.ReportSuccessAsync(successCommandResponse);
                            }
                            else
                            {
                                var failureCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Failed to open the shades", SpokenMessage = "Something went wrong. Could not open all shades" });
                                await m_VoiceServiceConnection.ReportFailureAsync(failureCommandResponse);
                            }
                            break;
                        case "close":
                            var offProgressResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Closing shades", SpokenMessage = "Please wait until I close all shades" });
                            await m_VoiceServiceConnection.ReportProgressAsync(offProgressResponse);

                            if (await g_CommandHandler.CloseAllAsync())
                            {
                                // All operations succeeded
                                var successCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Shades are closed", SpokenMessage = "Shades are now closed" });
                                await m_VoiceServiceConnection.ReportSuccessAsync(successCommandResponse);
                            }
                            else
                            {
                                var failureCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Failed to close all shades", SpokenMessage = "Something went wrong. Could not close all shades" });
                                await m_VoiceServiceConnection.ReportFailureAsync(failureCommandResponse);
                            }
                            break;
                    }
                }
            }
        }

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (m_Deferral != null)
                m_Deferral.Complete();
        }

        private void VoiceTaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (m_Deferral != null)
                m_Deferral.Complete();
        }
    }
}

