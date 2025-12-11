using ProgressOnderwijsUtils.SchemaReflection;

namespace ProgressOnderwijsUtils;

public enum ColumnAccessibility
{
    Normal, AutoIncrementIdentity, Readonly,
}

public sealed record ColumnDefinition(Type DataType, string Name, int Index, ColumnAccessibility ColumnAccessibility)
{
    public static ColumnDefinition Create(DataColumn col)
        => new(DataColumnType(col), col.ColumnName, col.Ordinal, DataColumnAccessibility(col));

    static ColumnAccessibility DataColumnAccessibility(DataColumn col)
        => col switch {
            { AutoIncrement: true, } => ColumnAccessibility.AutoIncrementIdentity,
            { ReadOnly: true, } => ColumnAccessibility.Readonly,
            _ => ColumnAccessibility.Normal,
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

    static ColumnDefinition FromSqlSystemTypeId(int columnOrdinal, string columnName, SqlSystemTypeId sqlSystemTypeId, ColumnAccessibility columnAccessibility)
        => new(sqlSystemTypeId.SqlUnderlyingTypeInfo().ClrType, columnName, columnOrdinal, columnAccessibility);

    public override string ToString()
        => $"{DataType.ToCSharpFriendlyTypeName()} {Name}";

    public static ColumnDefinition FromDbColumnMetaData(IDbColumn col, int colIdx)
        => FromSqlSystemTypeId(colIdx, col.ColumnName, col.UserTypeId, DbColumnMetaDataAccessibility(col));

    static ColumnAccessibility DbColumnMetaDataAccessibility(IDbColumn col)
        => col switch {
            { HasAutoIncrementIdentity : true, } => ColumnAccessibility.AutoIncrementIdentity,
            { IsRowVersion : true, } => ColumnAccessibility.Readonly,
            { IsComputed : true, } => ColumnAccessibility.Readonly,
            _ => ColumnAccessibility.Normal,
        };
}
