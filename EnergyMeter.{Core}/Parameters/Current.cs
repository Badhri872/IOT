﻿using BL.Contracts;
using Services.SerialClient;
using System.Collections.ObjectModel;
using static Services.Parser;

namespace EnergyMeter
{
    public class Current
    {
        private readonly ushort _primaryRegister = (ushort)Register.PrimaryCurrent;
        private readonly ushort _secondaryRegister = (ushort)Register.SecondaryCurrent;
        private readonly ushort _tertiaryRegister = (ushort)Register.TertiaryCurrent;
        private readonly Command _primaryCmd, _secondaryCmd, _tertiaryCmd;
        private readonly ParameterVM _primary, _secondary, _tertiary;
        private readonly SerialClient _client;

        public Current(SerialClient client)
        {
            _client = client;

            // Create and subscribe primary current
            _primaryCmd = new Command(1, _primaryRegister, 3);
            _primaryCmd.Received += onPrimaryCurrentReceived;
            _client.SendCommand(_primaryCmd.SerialCommand);
            _primary = new ParameterVM("Primary Current(A)");

            // Create and subscribe secondary current
            _secondaryCmd = new Command(1, _secondaryRegister, 3);
            _secondaryCmd.Received += onSecondaryCurrentReceived;
            _client.SendCommand(_secondaryCmd.SerialCommand);
            _secondary = new ParameterVM("Secondary Current(A)");

            // Create and subscribe tertiary current
            _tertiaryCmd = new Command(1, _tertiaryRegister, 3);
            _tertiaryCmd.Received += onTertiaryCurrentReceived;
            _client.SendCommand(_tertiaryCmd.SerialCommand);
            _tertiary = new ParameterVM("Tertiary Current(A)");
        }

        public ObservableCollection<ParameterVM> CurrentCollection =>
            new ObservableCollection<ParameterVM>
            {
                _primary, _secondary, _tertiary
            };

        private void onTertiaryCurrentReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _tertiary.Value = Float32(e.Value).ToString();
        }

        private void onSecondaryCurrentReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _secondary.Value = Float32(e.Value).ToString();
        }

        private void onPrimaryCurrentReceived(object sender, CommandEventArgs<byte[]> e)
        {
            _primary.Value = Float32(e.Value).ToString();
        }
    }
}
