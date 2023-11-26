using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public interface ISerialisable
    {
        SerialID ID { get; }
        IEnumerable<ISerialisationData> GetSerialisationData();
    }
}
