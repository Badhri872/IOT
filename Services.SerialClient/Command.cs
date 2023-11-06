using BL.Contracts;
using System;

namespace Services.SerialClient
{
    public class Command : ICommand
    {
        private readonly byte _slaveAddress, _function;
        private readonly uint _numberOfPoints = 2;
        private readonly ushort _register;
        private SerialCommand _serialCommand = new SerialCommand();
        public Command(byte slaveAddress, ushort register, byte function)
        {
            _slaveAddress = slaveAddress;
            _register = register;
            _function = function;
            _serialCommand.Received += onReceive;
            ProcessCommand();
        }

        public event EventHandler<CommandEventArgs<byte[]>> Received;

        public SerialCommand SerialCommand => _serialCommand;

        public void ProcessCommand()
        {
            var frame = readHoldingRegistersMsg();
            _serialCommand.Command = frame;
        }

        private byte[] readHoldingRegistersMsg()
        {
            byte[] frame = new byte[8];
            frame[0] = _slaveAddress;			    // Slave Address
            frame[1] = _function;				    // Function             
            frame[2] = (byte)(_register >> 8);	// Starting Address High
            frame[3] = (byte)(_register & 0xFF);		    // Starting Address Low            
            frame[4] = (byte)(_numberOfPoints >> 8);	// Quantity of Registers High
            frame[5] = (byte)(_numberOfPoints & 0xFF);		// Quantity of Registers Low
            byte[] crc = calculateCRC(frame);  // Calculate CRC.
            frame[frame.Length - 2] = crc[0];       // Error Check Low
            frame[frame.Length - 1] = crc[1];       // Error Check High
            return frame;
        }

        private byte[] calculateCRC(byte[] data)
        {
            ushort CRCFull = 0xFFFF; // Set the 16-bit register (CRC register) = FFFFH.
            char CRCLSB;
            byte[] CRC = new byte[2];
            for (int i = 0; i < (data.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ data[i]);

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1));

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = (byte)(CRCFull & 0xFF);
            return CRC;
        }

        private void onReceive(object sender, CommandEventArgs<byte[]> e)
        {
            Received?.Invoke(this, new CommandEventArgs<byte[]>(e.Value));
        }
    }
}
