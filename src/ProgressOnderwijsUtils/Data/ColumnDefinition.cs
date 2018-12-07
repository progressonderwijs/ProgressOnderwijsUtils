using System;
using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public sealed class ColumnDefinition
    {
        public Type DataType { get; }
        public string Name { get; }
        public int Index { get; }
        public bool HasAutoIncrementIdentity { get; }
        public bool HasDefaultValue { get; }

        [NotNull]
        public static ColumnDefinition Create([NotNull] DataColumn col)
            => new ColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName, col.Ordinal, col.AutoIncrement, col.DefaultValue !=DBNull.Value);

        [NotNull]
        public static ColumnDefinition[] GetFromReader([NotNull] IDataRecord reader)
        {
            var retval = new ColumnDefinition[reader.FieldCount];
            for (var index = 0; index < retval.Length; index++) {
                retval[index] = new ColumnDefinition(reader.GetFieldType(index), reader.GetName(index), index, false, false);
            }
            return retval;            
        }

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType)
            => FromSqlXType(columnOrdinal, columnName, sqlXType, false,false);

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType, bool hasAutoIncrementIdentity, bool hasDefaultValue)
            => new ColumnDefinition(sqlXType.SqlUnderlyingTypeInfo().ClrType, columnName, columnOrdinal,hasAutoIncrementIdentity, hasDefaultValue);

        public ColumnDefinition(Type dataType, string name, int index, bool hasAutoIncrementIdentity, bool hasDefaultValue)
        {
            DataType = dataType;
            Name = name;
            Index = index;
            HasAutoIncrementIdentity = hasAutoIncrementIdentity;
            HasDefaultValue = hasDefaultValue;
        }

        public override string ToString()
            => DataType.ToCSharpFriendlyTypeName() + " " + Name;
    }
}
