using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.AllJoyn;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.ApplicationModel.AppService;
using org.OpenT2T.Sample.SuperPopular.Lamp;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloLamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<Lamp> Lamps { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            Lamps = new ObservableCollection<Lamp>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateDeviceWatcher();
            CreateServiceConnection();
        }

        LampWatcher m_LampWatcher;
        AllJoynBusAttachment m_Attachment;

        private void CreateDeviceWatcher()
        {
            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();

            m_LampWatcher = new LampWatcher(m_Attachment);
            m_LampWatcher.Added += Lamp_Added;
            m_LampWatcher.Start();
        }


        AppServiceConnection m_AppServiceConnection;

        private async void CreateServiceConnection()
        {
            m_AppServiceConnection = new AppServiceConnection();
            m_AppServiceConnection.AppServiceName = "VoiceCommandService";
            m_AppServiceConnection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            var connectStatus = await m_AppServiceConnection.OpenAsync();

            if (connectStatus != AppServiceConnectionStatus.Success)
            { 
                throw new Exception(String.Format("Failed to open connection to service: {0}",connectStatus));
            }

            // Register handler for request recived to handle callbacks from cortana
            m_AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        // If background voice service gets voice command to turn lights on and off, it sends a command to the app 
        // So that the UI is in sync with the status
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Dispatch the callback from cortana as the UI updates need to be on UI thread
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    switch ((string)args.Request.Message["Command"])
                    {
                        case "AllOn":
                            SetAllLampsState(true);
                            break;
                        case "AllOff":
                            SetAllLampsState(false);
                            break;
                    }
                });
        }

        private async void Lamp_Added(LampWatcher sender, AllJoynServiceInfo args)
        {
            LampJoinSessionResult joinSessionResult = await LampConsumer.JoinSessionAsync(args, sender);
            if(joinSessionResult.Status == AllJoynStatus.Ok)
            {
                LampConsumer lampConsumer = joinSessionResult.Consumer;

                // Create ViewModel object from LampConsumer and add to the Lamps collection
                // Using args.ObjectPath for the name of the device. One could use args.UniqueId for an
                // Id property for lamps lookup from a dictionary.
                Lamp lamp = new Lamp(args.ObjectPath,lampConsumer);
                Lamps.Add(lamp);

                lamp.SessionLost += Lamp_SessionLost;

            }
        }

        // If session is lost for the device remove it from the list
        private void Lamp_SessionLost(object sender, EventArgs e)
        {
            Lamps.Remove((Lamp)sender);
        }

        private void gridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var lamp = e.ClickedItem as Lamp;

            // Invert the lamp state when clicked
            lamp.IsOn = !lamp.IsOn;
        }

        private void AllOn_Clicked(object sender, RoutedEventArgs e)
        {
            SetAllLampsState(true);
        }

        private void AllOff_Clicked(object sender, RoutedEventArgs e)
        {
            SetAllLampsState(false);
        }

        private void SetAllLampsState(bool state)
        {
            foreach (var lamp in Lamps)
            {
                lamp.IsOn = state;
            }
        }

        private void textCortana_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(textCortana);
        }
    }
}
