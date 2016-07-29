using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
    public interface IColumnDefinition
    {
        Type DataType { get; }
        string Name { get; }
    }

    public sealed class ColumnDefinition : IColumnDefinition
    {
        public Type DataType { get; }
        public string Name { get; }

        public static ColumnDefinition Create(DataColumn col) 
            => new ColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName);

        public static ColumnDefinition[] GetFromReader(IDataRecord reader) 
            => Enumerable.Range(0, reader.FieldCount).Select(fI => new ColumnDefinition(reader.GetFieldType(fI), reader.GetName(fI))).ToArray();

        public static ColumnDefinition[] GetFromTable(SqlConnection sqlconn, string tableName)
        {
            using (var cmd = sqlconn.CreateCommand()) {
                cmd.CommandText = "SET FMTONLY ON; select * from " + tableName + "; SET FMTONLY OFF";
                using (var fmtReader = cmd.ExecuteReader())
                    return GetFromReader(fmtReader);
            }
        }

        ColumnDefinition(Type dataType, string name)
        {
            DataType = dataType;
            Name = name;
        }

        public override string ToString() => DataType.ToCSharpFriendlyTypeName() + " " + Name;
    }
}
