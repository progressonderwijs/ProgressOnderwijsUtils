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
    }

    public sealed class ColumnDefinition : IColumnDefinition
    {
        public Type DataType { get; }
        public string Name { get; }

        [NotNull]
        public static ColumnDefinition Create([NotNull] DataColumn col)
            => new ColumnDefinition((col.AllowDBNull ? col.DataType.MakeNullableType() : null) ?? col.DataType, col.ColumnName);

        [NotNull]
        public static ColumnDefinition[] GetFromReader([NotNull] IDataRecord reader)
            => Enumerable.Range(0, reader.FieldCount).Select(fI => new ColumnDefinition(reader.GetFieldType(fI), reader.GetName(fI))).ToArray();

        [NotNull]
        public static ColumnDefinition[] GetFromTable([NotNull] DbColumnMetaData[] columns) 
            => columns.ArraySelect(column => new ColumnDefinition(column.User_Type_Id.SqlUnderlyingTypeInfo().ClrType, column.ColumnName));

        ColumnDefinition(Type dataType, string name)
        {
            DataType = dataType;
            Name = name;
        }

        public override string ToString() => DataType.ToCSharpFriendlyTypeName() + " " + Name;
    }
}
