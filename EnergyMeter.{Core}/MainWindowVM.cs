using BL.Contracts;
using Services.Parser;
using Services.SerialClient;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnergyMeter
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private float _ampere;
        private float _power;
        private float _frequency;
        private Command<float> _ampereCmd, _powerCmd, _frequenceCmd;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowVM()
        {
            _ampereCmd = new Command<float>(1, 3003, 3, Parser.Float32);
            _powerCmd = new Command<float>(1, 3057, 3, Parser.Float32);
            _frequenceCmd = new Command<float>(1, 2016, 3, Parser.Float32);
            _ampereCmd.Received += onCurrentChanged;
            _powerCmd.Received += onPowerChanged;
            _frequenceCmd.Received += onFrequencyChanged;
            Command<float>.Connect("COM7", 19200);
        }

        ~MainWindowVM()
        {
            Command<float>.Stop();
        }

        private void onPowerChanged(object sender, CommandEventArgs<float> e)
        {
            Power = e.Value;
        }

        private void onCurrentChanged(object sender, CommandEventArgs<float> e)
        {
            Ampere = e.Value;
        }

        private void onFrequencyChanged(object sender, CommandEventArgs<float> e)
        {
            Frequency = e.Value;
        }

        public float Ampere
        {
            get
            {
                return _ampere;
            }
            set
            {
                _ampere = value;
                onPropertyChanged();
            }
        }
        public float Power
        {
            get
            {
                return _power;
            }
            set
            {
                _power = value;
                onPropertyChanged();
            }
        }

        public float Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                _frequency = value;
                onPropertyChanged();
            }
        }

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
