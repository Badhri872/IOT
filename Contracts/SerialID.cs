using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public class SerialID
    {
        public Guid ID { get; set; }
        public Type Type { get; set; }
        public override string ToString()
        {
            return $"ID: {ID} Type: {Type}";
        }
    }
}
