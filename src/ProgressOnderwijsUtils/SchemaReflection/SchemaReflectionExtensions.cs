namespace ProgressOnderwijsUtils.SchemaReflection;

public static class SchemaReflectionExtensions
{
    public static DataTable ToEmptyDataTable(this DatabaseDescription.Table tableDescription, SqlTypeToClrType sqlTypeToClrType)
        => tableDescription.Columns.ToEmptyDataTable(tableDescription.QualifiedName, sqlTypeToClrType);

    public static DataTable ToEmptyDataTable(this IEnumerable<IDbColumn> columns, string qualifiedTableName, SqlTypeToClrType sqlTypeToClrType)
    {
        var table = new DataTable(DbQualifiedNameUtils.UnqualifiedObjectName(qualifiedTableName), DbQualifiedNameUtils.SchemaFromQualifiedName(qualifiedTableName));
        table.Columns.AddRange([.. columns.Select(col => col.ToDataColumn(sqlTypeToClrType)),]);
        return table;
    }
}
