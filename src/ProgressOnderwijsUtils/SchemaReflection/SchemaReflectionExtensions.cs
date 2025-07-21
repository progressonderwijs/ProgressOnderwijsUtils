namespace ProgressOnderwijsUtils.SchemaReflection;

public static class SchemaReflectionExtensions
{
    public static DataTable ToEmptyDataTable(this DatabaseDescription.Table tableDescription)
        => tableDescription.Columns.ToEmptyDataTable(tableDescription.QualifiedName);

    public static DataTable ToEmptyDataTable(this IEnumerable<IDbColumn> columns, string qualifiedTableName)
    {
        var table = new DataTable(DbQualifiedNameUtils.UnqualifiedObjectName(qualifiedTableName), DbQualifiedNameUtils.SchemaFromQualifiedName(qualifiedTableName));
        table.Columns.AddRange(columns.Select(col => col.ToDataColumn()).ToArray());
        return table;
    }
}
