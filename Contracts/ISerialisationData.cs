using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public interface ISerialisationData
    {
        Guid ID { get; set; }
        object CreateObject(IDeserialiser deserialiser);
    }
}
