using BL.Contracts;
using System;

namespace Services
{
    public class FieldValue : IFieldValue
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public Type DataType { get; set; }
    }
}
