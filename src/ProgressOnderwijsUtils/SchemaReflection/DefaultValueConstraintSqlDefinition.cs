namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record DefaultValueConstraintSqlDefinition(DbObjectId ParentObjectId, DbColumnId ParentColumnId, string Name, string Definition, string SchemaName, string TableName) : IWrittenImplicitly
{
    public static DefaultValueConstraintSqlDefinition[] LoadAll(SqlConnection conn)
        => SQL(
            $"""
            select
            ParentObjectId = d.parent_object_id
            , ParentColumnId = d.parent_column_id
            , d.name
            , d.definition
            , SchemaName = s.name
            , TableName = t.name
            from sys.default_constraints d
            inner join sys.schemas s on s.schema_id = d.schema_id
            inner join sys.tables t on t.object_id = d.parent_object_id
            """
        ).ReadPocos<DefaultValueConstraintSqlDefinition>(conn);
}
