using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Data
{
	public sealed class DbColumnDefinition
	{
		public static DbColumnDefinition Create(DataColumn col) { return new DbColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName); }
		public static DbColumnDefinition Create(IMetaProperty col) { return new DbColumnDefinition(col.DataType, col.Naam); }

		public static DbColumnDefinition[] GetFromReader(IDataRecord reader) { return Enumerable.Range(0, reader.FieldCount).Select(fI => new DbColumnDefinition(reader.GetFieldType(fI), reader.GetName(fI))).ToArray(); }

		public static DbColumnDefinition[] GetFromTable(SqlConnection sqlconn, string tableName)
		{
			using (SqlCommand cmd = sqlconn.CreateCommand())
			{
				cmd.CommandText = "SET FMTONLY ON; select * from " + tableName + "; SET FMTONLY OFF";
				using (var fmtReader = cmd.ExecuteReader())
					return GetFromReader(fmtReader);
			}
		}

		public readonly Type Type;
		public readonly string ColumnName;
		public DbColumnDefinition(Type type, string columnName) { Type = type; ColumnName = columnName; }
		public override string ToString()
		{
			return ObjectToCode.GetCSharpFriendlyTypeName(Type) + " " + ColumnName;
		}
	}
}