using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public interface ICommand<T>
    {
        void SendCommand();
        event EventHandler<CommandEventArgs<T>> Received;
    }
}
