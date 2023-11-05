using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SerialClient
{
    public class SerialClient
    {
        private CommandProcessor _processor = new CommandProcessor();
        private string _portName;
        private int _baudRate;
        public SerialClient(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public void Connect() => _processor.Connect(_portName, _baudRate);

        public void SendCommand(SerialCommand command)
        {
            _processor.Send(command);
        }

        public void Stop() => _processor.Stop();

        ~SerialClient()
        {
            _processor.Stop();
        }
    }
}
