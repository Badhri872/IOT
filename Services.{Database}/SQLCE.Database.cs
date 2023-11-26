using BL.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class Database : IDisposable
    {
        private struct ConnectionInfo
        {
            public string DataSource { get; set; }
            public string Password { get; set; }
            public bool PersistSecurityInfo { get; set; }

            public string ConnectionString
            {
                get
                {
                    var result =
                        "Data Source = " + DataSource + ";" +
                        "Persist Security Info = " + PersistSecurityInfo.ToString() + ";";
                    if (Password.Length > 0)
                    {
                        result += "Password = " + Password + ";";
                    }
                    return result;
                }
            }
        }

        private ConnectionInfo _connectionInfo;
        private SqlCeConnection _connection;
        private bool _modified = false;
        private List<string> _tableNames = new List<string>();

        #region Construction/disposal
        public Database(string directoryPath, string password = "")
        {
            string filePath = Path.Combine(directoryPath, "CharacteristicsTests.sdf");
            _connectionInfo = new ConnectionInfo()
            {
                DataSource = filePath,
                Password = password,
                PersistSecurityInfo = false,
            };

            if (!File.Exists(_connectionInfo.DataSource))
            {
                createEmptyDB(directoryPath);
            }

            openConnection();

        }

        ~Database()
        {
            Dispose();
        }

        public void Dispose()
        {
            closeConnection();
            GC.SuppressFinalize(this);
        }

        private void openConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlCeConnection(_connectionInfo.ConnectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                try
                {
                    _connection.Open();

                    //_tableNames = getTableNames();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void closeConnection()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _tableNames.Clear();
                }
                _connection.Dispose();
                _connection = null;
            }
        }
        #endregion

        #region System type to Sql type conversion
        private static readonly Dictionary<Type, SqlDbType> _sqlDBTypeMap = new Dictionary<Type, SqlDbType>()
        {
            {typeof(string),        SqlDbType.NVarChar},
            {typeof(char[]),        SqlDbType.NVarChar},
            {typeof(byte[]),        SqlDbType.Image},
            {typeof(byte),        SqlDbType.TinyInt},
            {typeof(short),        SqlDbType.SmallInt},
            {typeof(int),        SqlDbType.Int},
            {typeof(long),        SqlDbType.BigInt},
            {typeof(bool),        SqlDbType.Bit},
            {typeof(float),        SqlDbType.Real},
            {typeof(double),        SqlDbType.Float},
            {typeof(Guid),        SqlDbType.UniqueIdentifier},
            {typeof(DateTime),        SqlDbType.DateTime},
        };

        public static string GetDBType(Type systemType)
        {
            systemType = Nullable.GetUnderlyingType(systemType) ?? systemType;
            if (_sqlDBTypeMap.ContainsKey(systemType))
            {
                var sqlType = _sqlDBTypeMap[systemType];
                if (sqlType == SqlDbType.NVarChar)
                {
                    return "NVarChar(4000)";
                }
                else
                {
                    return sqlType.ToString();
                }
            }

            throw new Exception("Database type not supported");
        }

        public static bool IsSupportedType(Type systemType)
        {
            systemType = Nullable.GetUnderlyingType(systemType) ?? systemType;
            return _sqlDBTypeMap.ContainsKey(systemType);
        }
        #endregion

        #region Creation of database and tables
        private void createEmptyDB(string dirPath)
        {
            var filePath = _connectionInfo.DataSource;

            if (Directory.Exists(filePath)) { return; }
            Directory.CreateDirectory(dirPath);
            //createDirectoryAndFileWithFullControl(dirPath, filePath);

            using (var eng = new SqlCeEngine(_connectionInfo.ConnectionString))
            {
                eng.CreateDatabase();
            }
        }
        private static void createDirectoryAndFileWithFullControl(string directoryPath, string filePath)
        {
            try
            {
                // Create the directory if it doesn't exist
                Directory.CreateDirectory(directoryPath);

                // Specify the access control rules for the directory
                DirectorySecurity directorySecurity = new DirectorySecurity();
                FileSystemAccessRule directoryRule = new FileSystemAccessRule(
                    Environment.UserName,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                );
                directorySecurity.AddAccessRule(directoryRule);

                // Apply the directory access rules
                Directory.SetAccessControl(directoryPath, directorySecurity);

                // Create the file in the specified directory
                using (FileStream fileStream = File.Create(filePath))
                {
                    // Specify the access control rules for the file
                    FileSecurity fileSecurity = new FileSecurity();
                    FileSystemAccessRule fileRule = new FileSystemAccessRule(
                        Environment.UserName,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow
                    );
                    fileSecurity.AddAccessRule(fileRule);

                    // Apply the file access rules
                    File.SetAccessControl(filePath, fileSecurity);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory and file: {ex.Message}");
            }
        }

        public void CreateEmptyTable(string tableName, IReadOnlyList<IFieldValue> fields)
        {
            if (tableExists(tableName)) { return; }
            try
            {
                using (var cmd = new SqlCeCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = _connection;

                    cmd.CommandText = "CREATE TABLE [" + tableName + "] (" + getCreationString(fields[0]);
                                    //"_ID" + "uniqueidentifier NOT NULL CONSTRAINT PK_ID PRIMARY KEY";

                    for (int i = 1; i < fields.Count; ++i)
                    {
                        cmd.CommandText += "," + getCreationString(fields[i]);
                    }
                    cmd.CommandText += ")";
                    cmd.ExecuteNonQuery();
                }
                _tableNames.Add(tableName);
            }
            catch (Exception ex)
            {
                if (tableExists(tableName)) { throw new Exception("Attempted to create duplicate table"); }
            }
        }

        private static string getCreationString(IFieldValue field)
        {
            string result = "[_" + field.Name + "] " + Database.GetDBType(field.DataType);
            if (!isNullable(field.DataType)) { result += " NOT NULL"; }
            return result;
        }

        private static bool isNullable(Type t) => Nullable.GetUnderlyingType(t) != null;
        private bool tableExists(string tableName) => _tableNames.Count > 0 && _tableNames.Contains(tableName);

        public void CreatePopulatedListTable(string tableName, IReadOnlyList<IReadOnlyList<IFieldValue>> records)
        {
            CreateEmptyListTable(tableName, records[0]);
            AddList(tableName, records);
        }

        public void CreateEmptyListTable(string tableName, IReadOnlyList<IFieldValue> fields)
        {
            try
            {
                using (var cmd = new SqlCeCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = _connection;

                    cmd.CommandText = "CREATE TABLE " + tableName + " (_Index int NOT NULL";

                    for (int i = 0; i < fields.Count; ++i)
                    {
                        cmd.CommandText += "," + getCreationString(fields[i]);
                    }

                    cmd.CommandText += ")";
                    cmd.ExecuteNonQuery();
                }

                _tableNames.Add(tableName);
            }
            catch (Exception ex)
            {
                if (tableExists(tableName)) { throw new Exception("Attempted to create duplicate table"); }
            }
        }
        #endregion

        public void AddRecord(string tableName, List<IFieldValue> record)
        {
            try
            {

                var fieldList = record.Select(value => "[_" + value.Name + "]");
                var fields = string.Join(", ", fieldList);

                var valueList = record.Select(value => value.Value);
                var values = string.Join(", ", valueList);

                using (var cmd = new SqlCeCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = _connection;

                    cmd.CommandText = "INSERT INTO [" + tableName +
                                        "] (" + fields + ") " +
                                        "VALUES(" + values + ")";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public List<string> GetTableNames()
        {
            var result = new List<string>();

            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;

                cmd.CommandText = "SELECT TABLE_NAME FROM" +
                    " INFORMATION_SCHEMA.TABLES";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetValue(0).ToString());
                    }

                    return result;
                }
            }
        }

        public List<List<IFieldValue>> ReadRecord(string tableName)
        {
            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;

                cmd.CommandText = "SELECT *" +
                                  " FROM [" + tableName +"]";

                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var schemaTable = reader.GetSchemaTable();
                    var results = new List<List<IFieldValue>>();

                    while (reader.Read())
                    {
                        var result = new List<IFieldValue>(reader.FieldCount);
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            var value = reader.GetValue(i);
                            if (value is DBNull) { value = null; }
                            var name = reader.GetName(i);
                            name = getFieldName(name);

                            var type = reader.GetFieldType(i);

                            var fieldValue = new FieldValue
                            {
                                Name = name,
                                Value = value,
                                DataType = type
                            };

                            result.Add(fieldValue);
                        }
                        results.Add(result);
                    }

                    return results;
                }
            }
        }

        public void AddList(string tableName, IReadOnlyList<IReadOnlyList<IFieldValue>> records)
        {
            string fields = "_Index";
            string values = "@Index";

            for (int i = 0; i < records[0].Count; ++i)
            {
                var fieldValue = records[0][i];
                var dbFieldName = "_" + fieldValue.Name;
                fields += ", " + dbFieldName;
                values += ", @" + dbFieldName;
            }

            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;

                cmd.CommandText = "INSERT INTO " + tableName + " " +
                                    "(" + fields + ") " +
                                    "values(" + values + ")";

                for (int row = 0; row < records.Count; ++row)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@Index", row.ToString());

                    var fieldValues = records[row];
                    for (int i = 0; i < fieldValues.Count; ++i)
                    {
                        cmd.Parameters.Add(
                            new SqlCeParameter(
                                "@_" + fieldValues[i].Name,
                                fieldValues[i].Value ?? DBNull.Value));
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateRecord(string tableName, Guid id, IReadOnlyList<IFieldValue> fieldValues)
        {
            string fieldValuePairs = string.Join(", ", fieldValues.Select(fv => "_" + fv.Name + " =@" +
                                                    "_" + fv.Name));
            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;
                cmd.CommandText = "UPDATE " + tableName + " set " + fieldValuePairs +
                                    "WHERE " + "_ID" + " =@ID";
                cmd.Parameters.AddWithValue("@ID", id);

                for (int i = 0; i < fieldValues.Count; ++i)
                {
                    cmd.Parameters.Add(
                        new SqlCeParameter(
                            "@" + "_" + fieldValues[i].Name,
                            fieldValues[i].Value ?? DBNull.Value));
                }
                cmd.ExecuteNonQuery();
            }
        }

        public bool TableExists(string tableName)
        {
            return _tableNames.Contains(tableName);
        }

        public bool RecordExists(string tableName, Guid id)
        {
            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;
                cmd.CommandText = "SELECT COUNT(*)" +
                                  " FROM " + tableName +
                                  " WHERE _ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);
                int result = (int)cmd.ExecuteScalar();

                return result == 1;
            }
        }

        private static string getValueString(IFieldValue field)
        {
            if (field.Value == null) { return "NULL"; }

            if (field.DataType == typeof(string) ||
                field.DataType == typeof(Guid) ||
                field.DataType == typeof(Guid?) ||
                field.DataType == typeof(bool) ||
                field.DataType == typeof(bool?) ||
                field.DataType == typeof(DateTime) ||
                field.DataType == typeof(DateTime?))
            {
                var quotedValue = "'" + field.Value.ToString() + "'";
                return quotedValue;
            }
            return field.Value.ToString();
        }

        public IReadOnlyList<IFieldValue> ReadRecord(string tableName, Guid id)
        {
            return ReadRecord(tableName, id, "*");
        }

        public IReadOnlyList<IFieldValue> ReadRecord(string tableName, Guid id, params string[] fields)
        {
            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;
                var prefixedFields = fields.Select(field => "_" + field);
                var fieldNames = string.Join(",", prefixedFields);

                cmd.CommandText = "SELECT " + fieldNames +
                                  " FROM " + tableName +
                                  " WHERE " + "_ID = @ID";
                cmd.Parameters.AddWithValue("@ID", id);

                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    var schemaTable = reader.GetSchemaTable();
                    var result = new List<IFieldValue>(reader.FieldCount);

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            var value = reader.GetValue(i);
                            if (value is DBNull) { value = null; }
                            var name = reader.GetName(i);
                            name = getFieldName(name);
                            var type = reader.GetFieldType(i);
                            var isNullable = (bool)schemaTable.Rows[i].ItemArray[schemaTable.Columns["AllowDBNull"].Ordinal];
                            if (isNullable && type.IsValueType)
                            {
                                type = typeof(Nullable<>).MakeGenericType(type);
                            }
                            //Type fieldType = typeof(FieldValue).MakeGenericType(type);
                            //var fieldValue = (IFieldValue)Activator.CreateInstance(fieldType);
                            //fieldType.GetProperty("Name").SetValue(fieldValue, name);
                            //fieldType.GetProperty("Value").SetValue(fieldValue, value);

                            //result.Add(fieldValue);
                        }
                    }

                    return result;
                }
            }
        }

        public IReadOnlyList<IReadOnlyList<IFieldValue>> ReadRecords(string tableName, IReadOnlyList<IFieldValue> whereConditions = null, string orderBy = "")
        {
            return readRecords(tableName, true, new string[] { "*" }, whereConditions, orderBy);
        }

        public IReadOnlyList<IReadOnlyList<IFieldValue>> ReadRecords(string tableName, string[] fields, IReadOnlyList<IFieldValue> whereConditions = null, string orderBy = "")
        {
            return readRecords(tableName, true, fields, whereConditions, orderBy);
        }

        private IReadOnlyList<IReadOnlyList<IFieldValue>> readRecords(string tableName, bool prefixNames, string[] fields, IReadOnlyList<IFieldValue> whereConditions, string orderBy)
        {
            using (var cmd = new SqlCeCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _connection;

                var fieldNames = "*";

                if (prefixNames)
                {
                    var prefixedFields = fields.Select(field => "_" + field);
                    fieldNames = string.Join(",", prefixedFields);
                }
                else
                {
                    fieldNames = string.Join(",", fields);
                }

                cmd.CommandText = "SELECT " + fieldNames +
                                  " FROM " + tableName;

                if (whereConditions != null && whereConditions.Count > 0)
                {
                    cmd.CommandText += " WHERE(";

                    for (int i = 0; i < whereConditions.Count; ++i)
                    {
                        if (i > 0) { cmd.CommandText += " AND"; }

                        if (whereConditions[i].Value == null)
                        {
                            cmd.CommandText += "_" + whereConditions[i].Name + " IS NULL";
                            cmd.Parameters.Add(
                                new SqlCeParameter(
                                    "@_" + whereConditions[i].Name,
                                    whereConditions[i].Value));
                        }
                    }
                    cmd.CommandText += ")";
                }

                if (orderBy.Length > 0)
                {
                    if (prefixNames) { orderBy = "_" + orderBy; }
                    cmd.CommandText += " ORDER BY " + orderBy;
                }

                using (var reader = cmd.ExecuteReader())
                {
                    var schemaTable = reader.GetSchemaTable();
                    var result = new List<List<IFieldValue>>();
                    while (reader.Read())
                    {
                        var record = new List<IFieldValue>(reader.FieldCount);
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            var value = reader.GetValue(i);
                            if (value is DBNull) { value = null; }
                            var name = reader.GetName(i);
                            if (prefixNames) { name = getFieldName(name); }
                            var type = reader.GetFieldType(i);
                            var isNullable = (bool)schemaTable.Rows[i].ItemArray[schemaTable.Columns["AllowDBNull"].Ordinal];
                            if (isNullable && type.IsValueType)
                            {
                                type = typeof(Nullable<>).MakeGenericType(type);
                            }
                            //Type fieldType = typeof(FieldValue).MakeGenericType(type);
                            //var fieldValue = (IFieldValue)Activator.CreateInstance(fieldType);
                            //fieldType.GetProperty("Name").SetValue(fieldValue, name);
                            //fieldType.GetProperty("Value").SetValue(fieldValue, value);

                            //record.Add(fieldValue);

                        }
                        result.Add(record);
                    }

                    return result;
                }
            }
        }

        private static string getFieldName(string dbFieldName) => dbFieldName.Substring(1);

    }
}
