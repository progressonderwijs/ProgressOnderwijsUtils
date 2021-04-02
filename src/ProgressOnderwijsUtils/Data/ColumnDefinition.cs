using System;
using System.Data;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public enum ColumnAccessibility
    {
        Normal,
        NormalWithDefaultValue,
        AutoIncrement,
        RowVersion,
        Readonly,
    }

    public sealed class ColumnDefinition
    {
        public readonly Type DataType;
        public readonly string Name;
        public readonly int Index;
        public readonly ColumnAccessibility ColumnAccessibility;

        public ColumnDefinition(Type dataType, string name, int index, ColumnAccessibility columnAccessibility)
        {
            DataType = dataType;
            Name = name;
            Index = index;
            ColumnAccessibility = columnAccessibility;
        }

        public static ColumnDefinition Create(DataColumn col)
            => new(DataColumnType(col), col.ColumnName, col.Ordinal, DataColumnAccessibility(col));

        static ColumnAccessibility DataColumnAccessibility(DataColumn col)
            => col.AutoIncrement ? ColumnAccessibility.AutoIncrement
                : col.ReadOnly ? ColumnAccessibility.Readonly
                : col.DefaultValue != DBNull.Value ? ColumnAccessibility.NormalWithDefaultValue
                : ColumnAccessibility.Normal;

        static Type DataColumnType(DataColumn col)
            => (col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType;

        public static ColumnDefinition[] GetFromReader(IDataRecord reader)
        {
            var retval = new ColumnDefinition[reader.FieldCount];
            for (var index = 0; index < retval.Length; index++) {
                retval[index] = new(reader.GetFieldType(index), reader.GetName(index), index, ColumnAccessibility.Readonly);
            }
            return retval;
        }

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType, ColumnAccessibility columnAccessibility)
            => new(sqlXType.SqlUnderlyingTypeInfo().ClrType, columnName, columnOrdinal, columnAccessibility);

        public override string ToString()
            => DataType.ToCSharpFriendlyTypeName() + " " + Name;

        public static ColumnDefinition FromDbColumnMetaData(DbColumnMetaData col, int colIdx)
            => FromSqlXType(colIdx, col.ColumnName, col.UserTypeId, DbColumnMetaDataAccessibility(col));

        static ColumnAccessibility DbColumnMetaDataAccessibility(DbColumnMetaData col)
            => col.HasAutoIncrementIdentity ? ColumnAccessibility.AutoIncrement
                : col.IsRowVersion ? ColumnAccessibility.RowVersion
                : col.IsComputed ? ColumnAccessibility.Readonly
                : col.HasDefaultValue ? ColumnAccessibility.NormalWithDefaultValue
                : ColumnAccessibility.Normal;
    }
}
