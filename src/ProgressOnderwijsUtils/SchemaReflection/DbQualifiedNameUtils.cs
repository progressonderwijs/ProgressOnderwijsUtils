namespace ProgressOnderwijsUtils.SchemaReflection;

public static class DbQualifiedNameUtils
{
    public static string QualifiedObjectName(string schema, string unqualifiedName)
        => $"{schema}.{unqualifiedName}";

    public static ParameterizedSql QualifiedObjectName(ParameterizedSql schema, ParameterizedSql unqualifiedName)
        => SQL($"{schema}.{unqualifiedName}");

    public static string UnqualifiedObjectName(string qualifiedName)
        => qualifiedName[(qualifiedName.IndexOf('.') + 1)..];

    public static ParameterizedSql UnqualifiedObjectName(ParameterizedSql qualifiedName)
        => ParameterizedSql.UnescapedSqlIdentifier(UnqualifiedObjectName(qualifiedName.CommandText()));

    public static bool IsNameInSchema(string qualifiedName, string schema)
        => qualifiedName.StartsWith($"{schema}.", StringComparison.OrdinalIgnoreCase);

    public static string SchemaFromQualifiedName(string qualifiedName)
        => qualifiedName[..qualifiedName.IndexOf('.')];
}
