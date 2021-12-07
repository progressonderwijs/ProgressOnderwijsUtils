using System;

namespace ProgressOnderwijsUtils.SchemaReflection;

public static class DbQualifiedNameUtils
{
    public static string UnqualifiedTableName(string table)
        => table.Substring(table.IndexOf('.') + 1);

    public static ParameterizedSql UnqualifiedTableName(ParameterizedSql table)
        => ParameterizedSql.CreateDynamic(UnqualifiedTableName(table.CommandText()));

    public static bool IsNameInSchema(string tabel, string schema)
        => tabel.StartsWith(schema + ".", StringComparison.OrdinalIgnoreCase);

    public static string SchemaFromQualifiedName(string qualifiedName)
        => qualifiedName.Substring(0, qualifiedName.IndexOf('.'));
}