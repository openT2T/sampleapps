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
using Windows.ApplicationModel.AppService;
using System.Collections.ObjectModel;
using Windows.Devices.AllJoyn;
using org.OpenT2T.Sample.SuperPopular.Shades;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace helloShades
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<Shade> Shades { get; private set; }
        private AllJoynBusAttachment m_Attachment;
        ShadesWatcher m_Watcher;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            m_Attachment = new AllJoynBusAttachment();
            m_Attachment.Connect();

            m_Watcher = new ShadesWatcher(m_Attachment);
            m_Watcher.Added += Watcher_Added;
        }

        private async void Watcher_Added(ShadesWatcher sender, AllJoynServiceInfo args)
        {
            ShadesJoinSessionResult joinSessionResult = await ShadesConsumer.JoinSessionAsync(args, sender);
            if (joinSessionResult.Status == AllJoynStatus.Ok)
            {
                var shade = new Shade(System.IO.Path.GetDirectoryName(args.ObjectPath), joinSessionResult.Consumer);
                Shades.Add(shade);
                shade.SessionLost += Shade_SessionLost;
            }
        }

        private void Shade_SessionLost(object sender, EventArgs e)
        {
            Shades.Remove((Shade)sender);
        }

        private async void OpenAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var shade in Shades)
            {
                await shade.CloseAsync();
            }
        }

        private async void CloseAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var shade in Shades)
            {
                await shade.OpenAsync();
            }
        }

        private async void gridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Shade shade = (Shade)e.ClickedItem;
            if (shade.State == ShadeState.Closed || shade.State == ShadeState.Closing)
                await shade.OpenAsync();
            else
                await shade.CloseAsync();
        }

        private void textCortana_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(textCortana);
        }
    }
}
