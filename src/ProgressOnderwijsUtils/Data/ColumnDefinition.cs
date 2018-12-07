using System;
using System.Data;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public enum ColumnAccessibility
    {
        Normal,
        NormalWithDefaultValue,
        AutoIncrement,
        Readonly,
    }

    public sealed class ColumnDefinition
    {
        public Type DataType { get; }
        public string Name { get; }
        public int Index { get; }
        public ColumnAccessibility ColumnAccessibility { get; }

        [NotNull]
        public static ColumnDefinition Create([NotNull] DataColumn col)
            => new ColumnDefinition(
                (col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType,
                col.ColumnName,
                col.Ordinal,
                col.AutoIncrement ? ColumnAccessibility.AutoIncrement
                : col.ReadOnly ? ColumnAccessibility.Readonly
                : col.DefaultValue != DBNull.Value ? ColumnAccessibility.NormalWithDefaultValue
                : ColumnAccessibility.Normal
            );

        [NotNull]
        public static ColumnDefinition[] GetFromReader([NotNull] IDataRecord reader)
        {
            var retval = new ColumnDefinition[reader.FieldCount];
            for (var index = 0; index < retval.Length; index++) {
                retval[index] = new ColumnDefinition(reader.GetFieldType(index), reader.GetName(index), index, ColumnAccessibility.Normal);
            }
            return retval;
        }

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType)
            => FromSqlXType(columnOrdinal, columnName, sqlXType, ColumnAccessibility.Normal);

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType, ColumnAccessibility columnAccessibility)
            => new ColumnDefinition(sqlXType.SqlUnderlyingTypeInfo().ClrType, columnName, columnOrdinal, columnAccessibility);

        public ColumnDefinition(Type dataType, string name, int index, ColumnAccessibility columnAccessibility)
        {
            DataType = dataType;
            Name = name;
            Index = index;
            ColumnAccessibility = columnAccessibility;
        }

        public override string ToString()
            => DataType.ToCSharpFriendlyTypeName() + " " + Name;

        public static ColumnDefinition FromDbColumnMetaData(DbColumnMetaData col, int colIdx)
            => FromSqlXType(colIdx, col.ColumnName, col.UserTypeId,
                col.HasAutoIncrementIdentity ? ColumnAccessibility.AutoIncrement
                : col.IsComputed || col.IsRowVersion ? ColumnAccessibility.Readonly
                : col.HasDefaultValue ? ColumnAccessibility.NormalWithDefaultValue
                : ColumnAccessibility.Normal
            );
    }
}
