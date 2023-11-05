using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public interface ICommand
    {
        void ProcessCommand();
        event EventHandler<CommandEventArgs<byte[]>> Received;
    }
}
