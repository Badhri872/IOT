using BL.Contracts;
using System.Collections.Generic;

namespace Services
{
    public static class DatabaseExtensions
    {
        public static void Serialise(this Database database, List<IFieldValue> fields, string characteristics)
        {
            database.AddRecord(characteristics, fields);
        }

        public static void DeSerialise(this Database database, string characteristics)
        {
            database.ReadRecord(characteristics);
        }
    }    
}
