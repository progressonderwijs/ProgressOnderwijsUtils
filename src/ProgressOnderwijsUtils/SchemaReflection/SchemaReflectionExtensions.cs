using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProgressOnderwijsUtils.SchemaReflection;

public static class SchemaReflectionExtensions
{
    public static DataTable ToEmptyDataTable(this DatabaseDescription.Table tableDescription)
        => tableDescription.Columns.Select(col => col.ColumnMetaData).ToEmptyDataTable(tableDescription.QualifiedName);

    public static DataTable ToEmptyDataTable(this IEnumerable<DbColumnMetaData> columns, string qualifiedTableName)
    {
        var table = new DataTable(DbQualifiedNameUtils.UnqualifiedTableName(qualifiedTableName), DbQualifiedNameUtils.SchemaFromQualifiedName(qualifiedTableName));
        table.Columns.AddRange(columns.Select(col => col.ToDataColumn()).ToArray());
        return table;
    }
}