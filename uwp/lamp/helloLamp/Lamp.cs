using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.OpenT2T.Sample.SuperPopular.Lamp;
using System.IO;
using Windows.Devices.AllJoyn;

namespace HelloLamp
{
    public class Lamp : INotifyPropertyChanged
    {
        private LampConsumer m_Consumer;

        public Lamp(string objectPath, LampConsumer consumer)
        {
            m_Consumer = consumer;
            // Use the file path api to extract name
            Name = Path.GetFileName(objectPath);
            m_IsOn = false;
            m_Brightness = 1.0;
            m_Consumer.SessionLost += Consumer_SessionLost;
        }

        private void Consumer_SessionLost(LampConsumer sender, AllJoynSessionLostEventArgs args)
        {
            SessionLost?.Invoke(this, new EventArgs());
        }

        public event EventHandler SessionLost;

        private string m_Name;

        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }


        private bool m_IsOn;

        public bool IsOn
        {
            get
            {
                return m_IsOn;
            }
            set
            {
                if (m_IsOn != value)
                {
                    m_IsOn = value;

                    if (value)
                        TurnLampOn();
                    else
                        TurnLampOff();

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOn"));
                }
            }
        }


        async void TurnLampOn()
        {
            var result = await m_Consumer.TurnOnAsync();
            if (result.Status != AllJoynStatus.Ok)
                throw new InvalidOperationException("TurnOn failed with code " + result.Status);
        }

        async void TurnLampOff()
        {
            var result = await m_Consumer.TurnOffAsync();
            if (result.Status != AllJoynStatus.Ok)
                throw new InvalidOperationException("TurnOff failed with code " + result.Status);
        }

        private double m_Brightness;
        public double Brightness
        {
            get
            {
                return m_Brightness;
            }
            set
            {
                if (m_Brightness != value)
                {
                    m_Brightness = value;
                    SetBrightnessLevel(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Brightness"));
                }
            }
        }

        async void SetBrightnessLevel(double brightness)
        {
            // Convert from 0.0 - 1.0 to 0 - 100 and invoke consumer method
            var result = await m_Consumer.SetBrightnessAsync((uint)(brightness * 100.0));
            if (result.Status != AllJoynStatus.Ok)
                throw new InvalidOperationException("SetBrightness failed with code " + result.Status);
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
