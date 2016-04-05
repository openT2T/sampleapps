using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation.Collections;
using org.OpenT2T.Sample.SuperPopular.Lamp;

namespace CortanaComponent
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        private VoiceCommandServiceConnection m_VoiceServiceConnection;
        private BackgroundTaskDeferral m_AppServiceDeferral;
        private BackgroundTaskDeferral m_VoiceServiceDeferral;
        static AppServiceConnection g_AppServiceConnection;
        static VoiceCommandHandler g_CommandHandler;

        const string cortanaFamilyId = "Microsoft.Windows.Cortana_cw5n1h2txyewy";

        static VoiceCommandService()
        {
            g_CommandHandler = new VoiceCommandHandler();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            AppServiceTriggerDetails triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                m_VoiceServiceDeferral = taskInstance.GetDeferral();
                taskInstance.Canceled += VoiceTaskInstance_Canceled;

                if (triggerDetails.CallerPackageFamilyName == cortanaFamilyId)
                {
                    // Being called from Cortana
                    m_VoiceServiceConnection =  VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    m_VoiceServiceConnection.VoiceCommandCompleted += VoiceServiceConnection_VoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await m_VoiceServiceConnection.GetVoiceCommandAsync();


                    switch (voiceCommand.CommandName)
                    {
                        case "setLightsOn":
                            var onProgressResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Turning all lights on", SpokenMessage = "Turning all lights on" });
                            await m_VoiceServiceConnection.ReportProgressAsync(onProgressResponse);

                            if (await g_CommandHandler.SetAllOnAsync())
                            {
                                // All operations succeeded
                                var successCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "All lights on", SpokenMessage = "All lights on" });
                                await m_VoiceServiceConnection.ReportSuccessAsync(successCommandResponse);
                                await NotifyAppOnStateChange(true);
                            }
                            else
                            {
                                var failureCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Failed to turn all lights on", SpokenMessage = "Something went wrong. Could not turn all lights on" });
                                await m_VoiceServiceConnection.ReportFailureAsync(failureCommandResponse);
                            }
                            break;
                        case "setLightsOff":
                            var offProgressResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Turning all lights off", SpokenMessage = "Turning all lights off" });
                            await m_VoiceServiceConnection.ReportProgressAsync(offProgressResponse);

                            if (await g_CommandHandler.SetAllOnAsync())
                            {
                                // All operations succeeded
                                var successCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "All lights off", SpokenMessage = "All lights off" });
                                await m_VoiceServiceConnection.ReportSuccessAsync(successCommandResponse);
                                await NotifyAppOnStateChange(false);
                            }
                            else
                            {
                                var failureCommandResponse = VoiceCommandResponse.CreateResponse(new VoiceCommandUserMessage() { DisplayMessage = "Failed to turn all lights off", SpokenMessage = "Something went wrong. Could not turn all lights off" });
                                await m_VoiceServiceConnection.ReportFailureAsync(failureCommandResponse);
                            }
                            break;
                    }
                }

                if (triggerDetails.CallerPackageFamilyName == Windows.ApplicationModel.Package.Current.Id.FamilyName)
                {
                    // Being called from the foreground application
                    m_AppServiceDeferral = taskInstance.GetDeferral();
                    taskInstance.Canceled += AppServiceTask_Canceled;
                    triggerDetails.AppServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
                    g_AppServiceConnection = triggerDetails.AppServiceConnection;
                }

            }

        }


        async Task NotifyAppOnStateChange(bool newState)
        {
            // If Appservice connection is existing, then send a state change command to the app
            if (g_AppServiceConnection != null)
            {
                ValueSet message = new ValueSet();
                message.Add("Command", newState ? "AllOn" : "AllOff");
                await g_AppServiceConnection.SendMessageAsync(message);
            }
        }

        private void AppServiceTask_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (m_AppServiceDeferral != null)
            {
                m_AppServiceDeferral.Complete();
                g_AppServiceConnection = null;
                m_AppServiceDeferral = null;
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

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (m_VoiceServiceDeferral != null)
            {
                // Complete the service deferral.
                m_VoiceServiceDeferral.Complete();
                m_VoiceServiceDeferral = null;
            }
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            g_AppServiceConnection = null;
            if (m_AppServiceDeferral != null)
            {
                // Complete the service deferral.
                m_AppServiceDeferral.Complete();
                m_AppServiceDeferral = null;
            }
        }
    }

}
