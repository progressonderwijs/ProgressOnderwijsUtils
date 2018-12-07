using System;
using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils
{
    public interface IColumnDefinition
    {
        Type DataType { get; }
        string Name { get; }
        int Index { get; }
    }

    public sealed class ColumnDefinition : IColumnDefinition
    {
        public Type DataType { get; }
        public string Name { get; }
        public int Index { get; }

        [NotNull]
        public static ColumnDefinition Create([NotNull] DataColumn col)
            => new ColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName, col.Ordinal);

        [NotNull]
        public static ColumnDefinition[] GetFromReader([NotNull] IDataRecord reader)
        {
            var retval = new ColumnDefinition[reader.FieldCount];
            for (var index = 0; index < retval.Length; index++) {
                retval[index] = new ColumnDefinition(reader.GetFieldType(index), reader.GetName(index), index);
            }
            return retval;            
        }

        public static ColumnDefinition FromSqlXType(int columnOrdinal, string columnName, SqlXType sqlXType)
            => new ColumnDefinition(sqlXType.SqlUnderlyingTypeInfo().ClrType, columnName, columnOrdinal);

        public ColumnDefinition(Type dataType, string name, int index)
        {
            DataType = dataType;
            Name = name;
            Index = index;
        }

        public override string ToString()
            => DataType.ToCSharpFriendlyTypeName() + " " + Name;
    }
}
