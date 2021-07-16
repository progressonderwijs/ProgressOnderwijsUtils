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

    public sealed record ColumnDefinition(Type DataType, string Name, int Index, ColumnAccessibility ColumnAccessibility)
    {
        public static ColumnDefinition Create(DataColumn col)
            => new(DataColumnType(col), col.ColumnName, col.Ordinal, DataColumnAccessibility(col));

        static ColumnAccessibility DataColumnAccessibility(DataColumn col)
            => col switch {
                { AutoIncrement: true, } => ColumnAccessibility.AutoIncrement,
                { ReadOnly: true, } => ColumnAccessibility.Readonly,
                { DefaultValue : DBNull, } => ColumnAccessibility.Normal,
                _ => ColumnAccessibility.NormalWithDefaultValue,
            };

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
            => col switch {
                { HasAutoIncrementIdentity : true, } => ColumnAccessibility.AutoIncrement,
                { IsRowVersion : true, } => ColumnAccessibility.RowVersion,
                { IsComputed : true, } => ColumnAccessibility.Readonly,
                { HasDefaultValue : true, } => ColumnAccessibility.NormalWithDefaultValue,
                _ => ColumnAccessibility.Normal,
            };
    }
}
