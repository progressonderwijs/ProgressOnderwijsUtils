using ExpressionToCodeLib;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.SchemaReflection;
using System;
using System.Data;
using System.Linq;

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
            => Enumerable.Range(0, reader.FieldCount).Select(fI => new ColumnDefinition(reader.GetFieldType(fI), reader.GetName(fI), fI)).ToArray();

        [NotNull]
        public static ColumnDefinition[] GetFromTable([NotNull] DbColumnMetaData[] columns) 
            => columns.ArraySelect(column => new ColumnDefinition(column.User_Type_Id.SqlUnderlyingTypeInfo().ClrType, column.ColumnName, (int)column.ColumnId - 1));

        ColumnDefinition(Type dataType, string name, int index)
        {
            DataType = dataType;
            Name = name;
            Index = index;
        }

        public override string ToString() => DataType.ToCSharpFriendlyTypeName() + " " + Name;
    }
}
