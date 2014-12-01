using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
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
        public static ColumnDefinition Create(DataColumn col)
        {
            return new ColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName);
        }

        public static ColumnDefinition Create(IMetaProperty col) { return new ColumnDefinition(col.DataType, col.Name); }

        public static ColumnDefinition[] GetFromReader(IDataRecord reader)
        {
            return Enumerable.Range(0, reader.FieldCount).Select(fI => new ColumnDefinition(reader.GetFieldType(fI), reader.GetName(fI))).ToArray();
        }

        public static ColumnDefinition[] GetFromTable(SqlConnection sqlconn, string tableName)
        {
            using (SqlCommand cmd = sqlconn.CreateCommand()) {
                cmd.CommandText = "SET FMTONLY ON; select * from " + tableName + "; SET FMTONLY OFF";
                using (var fmtReader = cmd.ExecuteReader())
                    return GetFromReader(fmtReader);
            }
        }

        readonly Type dataType;
        readonly string name;

        public ColumnDefinition(Type dataType, string name)
        {
            this.dataType = dataType;
            this.name = name;
        }

        public override string ToString() { return ObjectToCode.GetCSharpFriendlyTypeName(DataType) + " " + Name; }
        public Type DataType { get { return dataType; } }
        public string Name { get { return name; } }
    }
}
