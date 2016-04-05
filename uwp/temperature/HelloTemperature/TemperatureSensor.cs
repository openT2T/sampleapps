using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.OpenT2T.Sample.SuperPopular.TemperatureSensor;

namespace HelloTemperature
{
    public class TemperatureSensor : INotifyPropertyChanged
    {
        private TemperatureSensorConsumer m_Consumer;

        public TemperatureSensor(string name,TemperatureSensorConsumer consumer)
        {
            Name = name;
            m_Consumer = consumer;
            m_Consumer.SessionLost += Consumer_SessionLost;
        }

        private void Consumer_SessionLost(TemperatureSensorConsumer sender, Windows.Devices.AllJoyn.AllJoynSessionLostEventArgs args)
        {
            SessionLost?.Invoke(this, new EventArgs());
        }

        public event EventHandler SessionLost;

        public string Name { get; private set; }

        private double m_Trend;

        public double Trend
        {
            get { return m_Trend; }
            private set
            {
                if (value != m_Trend)
                {
                    m_Trend = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Trend"));
                }
            }
        }

        private double m_Temperature;

        public double Temperature
        {
            get { return m_Temperature; }
            private set
            {
                if (value != m_Temperature)
                {
                    m_Temperature = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Temperature"));
                }
            }
        }

        public async Task RefreshAsync()
        {
            var getTempResult = await m_Consumer.GetCurrentTemperatureAsync();
            if (getTempResult.Status != 0)
                throw new InvalidOperationException("Getting temperature failed " + getTempResult.Status);
            Temperature = getTempResult.Temp;

            var getTrendResult = await m_Consumer.GetTemperatureTrendAsync();
            if (getTrendResult.Status != 0)
                throw new InvalidOperationException("Getting temperature trend failde " + getTrendResult.Status);

            Trend = getTrendResult.Temp;    
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
