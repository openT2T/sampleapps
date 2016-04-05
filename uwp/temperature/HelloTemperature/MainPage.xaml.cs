using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using org.OpenT2T.Sample.SuperPopular.TemperatureSensor;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloTemperature
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<TemperatureSensor> Sensors { get; private set; }
        public MainPage()
        {
            this.InitializeComponent();
            Sensors = new ObservableCollection<TemperatureSensor>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateDeviceWatcher();
            /*var m_AppServiceConnection = new AppServiceConnection();
            m_AppServiceConnection.AppServiceName = "VoiceCommandService";
            m_AppServiceConnection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            var connectStatus = await m_AppServiceConnection.OpenAsync();

            if (connectStatus != AppServiceConnectionStatus.Success)
            {
                throw new Exception(String.Format("Failed to open connection to service: {0}", connectStatus));
            }*/

        }

        AllJoynBusAttachment m_Attachment;
        TemperatureSensorWatcher m_SensorWatcher;

        void CreateDeviceWatcher()
        {
            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();
            m_SensorWatcher = new TemperatureSensorWatcher(m_Attachment);
            m_SensorWatcher.Added += SensorWatcher_Added;
            m_SensorWatcher.Start();
        }

        private async void SensorWatcher_Added(TemperatureSensorWatcher sender, AllJoynServiceInfo args)
        {
            TemperatureSensorJoinSessionResult joinResult = await TemperatureSensorConsumer.JoinSessionAsync(args, sender);
            if (joinResult.Status == AllJoynStatus.Ok)
            {
                // Use file system path api to extract object name from path
                var sensor = new TemperatureSensor(System.IO.Path.GetFileName(args.ObjectPath), joinResult.Consumer);
                sensor.SessionLost += Sensor_SessionLost;
                Sensors.Add(sensor);

            }
        }

        private void Sensor_SessionLost(object sender, EventArgs e)
        {
            TemperatureSensor sensor = (TemperatureSensor)sender;
            Sensors.Remove(sensor);
        }

        private async void Refresh_Clicked(object sender, RoutedEventArgs e)
        {
            await RefreshAllItems();
        }

        private async Task RefreshAllItems()
        {
            foreach (var item in Sensors)
            {
                await item.RefreshAsync();
            }
        }

        private void textCortana_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(textCortana);
        }

    }
}
