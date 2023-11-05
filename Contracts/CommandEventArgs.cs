using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public class CommandEventArgs<T>
    {
        public T Value { get; }
        public CommandEventArgs(T value)
        {
            Value = value;
        }
    }
}
