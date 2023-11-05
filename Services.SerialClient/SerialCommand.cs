using System;
using BL.Contracts;

namespace Services.SerialClient
{
    public class SerialCommand
    {
        public byte[] Command { get; set; }
        public event EventHandler<CommandEventArgs<byte[]>> Received;
        public void ProcessResponse(byte[] buffer) =>
                        Received?.Invoke(this, new CommandEventArgs<byte[]>(buffer));


    }
}
