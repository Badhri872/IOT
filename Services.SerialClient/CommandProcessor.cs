using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Services.SerialClient
{
    public class CommandProcessor
    {
        private readonly List<SerialCommand> _commands = new List<SerialCommand>();
        private SerialPort _serialPort = new SerialPort();
        private Thread _processCommand;
        private bool _isConnected;

        public void Connect(string portName, int baudRate)
        {
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Handshake = System.IO.Ports.Handshake.None;
            _serialPort.Parity = Parity.Even;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.ReadTimeout = 200;
            _serialPort.WriteTimeout = 50;
            _serialPort.Open();
            _isConnected = _serialPort.IsOpen;
            _processCommand = new Thread(processCommands);
            _processCommand.Start();
        }

        public void Stop()
        {
            _isConnected = false;
            _processCommand.Join();
            _serialPort.Close();
        }

        public void Send(SerialCommand command)
        {
            _commands.Add(command);
        }

        private void processCommands()
        {
            while (_isConnected)
            {
                _commands.ForEach(command =>
                {
                    _serialPort.Write(command.Command, 0, command.Command.Length);
                    Thread.Sleep(100); // Delay 100ms
                    if (_serialPort.BytesToRead >= 5)
                    {
                        byte[] bufferReceiver = new byte[_serialPort.BytesToRead];
                        _serialPort.Read(bufferReceiver, 0, _serialPort.BytesToRead);
                        command.ProcessResponse(bufferReceiver);
                        _serialPort.DiscardInBuffer();
                    } 
                });
            }
        }

    }
}
