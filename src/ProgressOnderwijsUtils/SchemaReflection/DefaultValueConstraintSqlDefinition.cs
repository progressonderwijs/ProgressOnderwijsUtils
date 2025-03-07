namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record DefaultValueConstraintSqlDefinition(DbObjectId ParentObjectId, DbColumnId ParentColumnId, string Name, string Definition) : IWrittenImplicitly
{
    public static DefaultValueConstraintSqlDefinition[] LoadAll(SqlConnection conn)
        => SQL(
            $"""
            select
                ParentObjectId = d.parent_object_id
                , ParentColumnId = d.parent_column_id
                , d.name
                , d.definition
            from sys.default_constraints d
            """
        ).ReadPocos<DefaultValueConstraintSqlDefinition>(conn);
}
