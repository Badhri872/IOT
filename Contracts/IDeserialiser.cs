using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Contracts
{
    public interface IDeserialiser
    {
        object Context { get; }
        T Deserialise<T> (SerialID id) where T : ISerialisable;

        IList<SerialID> ListIDs(Type serialisationDataType);
    }
}
