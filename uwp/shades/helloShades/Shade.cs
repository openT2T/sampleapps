using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Devices.AllJoyn;
using org.OpenT2T.Sample.SuperPopular.Shades;

namespace helloShades
{
    public enum ShadeState
    {
        Unknown,
        Open,
        Opening,
        Closed,
        Closing,
        Error
    };

    public class Shade : DependencyObject, INotifyPropertyChanged
    {
        public string Name { get; private set; }


        private ShadeState m_State;

        public ShadeState State
        {
            get
            {
                return m_State;
            }
            set
            {
                if (m_State != value)
                {
                    m_State = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
                }
            }
        }

        ShadesConsumer m_Consumer;

        public Shade(string name,ShadesConsumer consumer)
        {
            State = ShadeState.Unknown;
            Name = name;
            m_Consumer = consumer;
            m_Consumer.SessionLost += Consumer_SessionLost;
            m_Consumer.Signals.ErrorReceived += Signals_ErrorReceived;
        }

        private async void Signals_ErrorReceived(ShadesSignals sender, ShadesErrorReceivedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { State = ShadeState.Error; });
        }

        private void Consumer_SessionLost(ShadesConsumer sender, AllJoynSessionLostEventArgs args)
        {
            SessionLost?.Invoke(this, new EventArgs());
        }

        public async Task OpenAsync()
        {
            // Do nothing if already open
            if (State == ShadeState.Open && State == ShadeState.Opening)
                return;

            State = ShadeState.Opening;

            var result = await m_Consumer.OpenAsync();

            if (result.Status == AllJoynStatus.Ok)
                State = ShadeState.Open;
            else
                State = ShadeState.Error;
        }

        public async Task CloseAsync()
        {
            // Do nothing if already closed
            if (State == ShadeState.Closed && State == ShadeState.Closing)
                return;

            State = ShadeState.Closing;

            var result = await m_Consumer.CloseAsync();

            if (result.Status == AllJoynStatus.Ok)
                State = ShadeState.Closed;
            else
                State = ShadeState.Error;
        }

        public event EventHandler SessionLost;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
