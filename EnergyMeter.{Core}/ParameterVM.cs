using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EnergyMeter
{
    public class ParameterVM : INotifyPropertyChanged
    {
        private string _parameterName;
        private string _value;

        public ParameterVM(string name)
        {
            _parameterName = name;
        }

        public string ParameterName
        {
            get
            {
                return _parameterName;
            }
            set
            {
                _parameterName = value;
                onPropertyChanged();
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                onPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
