using BL.Contracts;
using Services;
using Services.SerialClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Services.Parser;

namespace EnergyMeter
{
    public class Power
    {
        private readonly ushort _primaryRegister = (ushort)Register.PrimaryPower;
        private readonly ushort _secondaryRegister = (ushort)Register.SecondaryPower;
        private readonly ushort _tertiaryRegister = (ushort)Register.TertiaryPower;
        private readonly Command _primaryCmd, _secondaryCmd, _tertiaryCmd;
        private readonly ParameterVM _primary, _secondary, _tertiary;
        private readonly SerialClient _client;

        public Power(SerialClient client)
        {
            _client = client;

            // Create and subscribe primary current
            _primaryCmd = new Command(1, _primaryRegister, 3);
            _primaryCmd.Received += onPrimaryPowerReceived;
            _client.SendCommand(_primaryCmd.SerialCommand);
            _primary = new ParameterVM("Primary Power(KW)");

            // Create and subscribe secondary current
            _secondaryCmd = new Command(1, _secondaryRegister, 3);
            _secondaryCmd.Received += onSecondaryPowerReceived;
            _client.SendCommand(_secondaryCmd.SerialCommand);
            _secondary = new ParameterVM("Secondary Power(KW)");

            // Create and subscribe tertiary current
            _tertiaryCmd = new Command(1, _tertiaryRegister, 3);
            _tertiaryCmd.Received += onTertiaryPowerReceived;
            _client.SendCommand(_tertiaryCmd.SerialCommand);
            _tertiary = new ParameterVM("Tertiary Power(KW)");
        }
        public Power(double primary, double secondary, double tertiary)
        {
            _primary = new ParameterVM("Primary Power(KW)");
            _secondary = new ParameterVM("Secondary Power(KW)");
            _tertiary = new ParameterVM("Tertiary Power(KW)");
            _primary.Value = primary.ToString();
            _secondary.Value = secondary.ToString();
            _tertiary.Value = tertiary.ToString();
        }
        public Power(List<IFieldValue> powerData)
        {
            _primary = new ParameterVM("Primary Power(KW)");
            _secondary = new ParameterVM("Secondary Power(KW)");
            _tertiary = new ParameterVM("Tertiary Power(KW)");

            _primary.Value = powerData[0].Value.ToString();
            _secondary.Value = powerData[1].Value.ToString();
            _tertiary.Value = powerData[2].Value.ToString();
        }

        public ObservableCollection<ParameterVM> PowerCollection =>
            new ObservableCollection<ParameterVM>
            {
                _primary, _secondary, _tertiary
            };
        public double Primary
        {
            get
            {
                return double.Parse(_primary.Value);
            }
            set
            {
                _primary.Value = value.ToString();
            }
        }

        public double Secondary
        {
            get
            {
                return double.Parse(_secondary.Value);
            }
            set
            {
                _secondary.Value = value.ToString();
            }
        }

        public double Tertiary
        {
            get
            {
                return double.Parse(_tertiary.Value);
            }
            set
            {
                _tertiary.Value = value.ToString();
            }
        }
        public List<IFieldValue> GetValues()
        {
            return new List<IFieldValue>
            {
                new FieldValue
                {
                    Name = nameof(Primary),
                    Value = Primary,
                    DataType = Primary.GetType(),
                },
                new FieldValue
                {
                    Name = nameof(Secondary),
                    Value = Secondary,
                    DataType = Secondary.GetType(),
                },
                new FieldValue
                {
                    Name = nameof(Tertiary),
                    Value = Tertiary,
                    DataType = Tertiary.GetType(),
                }
            };
        }

        private void onTertiaryPowerReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _tertiary.Value = Float32(e.Value).ToString();
        }

        private void onSecondaryPowerReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _secondary.Value = Float32(e.Value).ToString();
        }

        private void onPrimaryPowerReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _primary.Value = Float32(e.Value).ToString();
        }
    }
}
