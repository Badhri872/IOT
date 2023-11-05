using BL.Contracts;
using Services.SerialClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnergyMeter
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private readonly SerialClient _client;
        private readonly Current _current;
        private readonly Power _power;

        public MainWindowVM()
        {
            _client = new SerialClient("COM7", 19200);
            _current = new Current(_client);
            _power = new Power(_client);
            _client.Connect();
        }

        public Current Current => _current;
        public Power Power => _power;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Close()
        {
            _client.Stop();
        }

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
