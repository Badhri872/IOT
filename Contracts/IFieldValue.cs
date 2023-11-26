using System;

namespace BL.Contracts
{
    public interface IFieldValue
    {
        string Name { get; }
        object Value { get; }
        Type DataType { get; }
    }
}
