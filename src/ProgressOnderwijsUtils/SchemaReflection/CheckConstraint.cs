namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record CheckConstraintSqlDefinition(DbObjectId TableObjectId, DbObjectId CheckConstraintObjectId, string Name, string Definition, bool IsNotTrusted, bool IsDisabled) : IWrittenImplicitly
{
    public static CheckConstraintSqlDefinition[] LoadAll(SqlConnection conn)
        => SQL(
            $@"
                select 
                    TableObjectId = cc.parent_object_id
                    , CheckConstraintObjectId = cc.object_id
                    , Name = cc.name
                    , Definition = cc.definition
                    , IsNotTrusted = cc.is_not_trusted
                    , IsDisabled = cc.is_disabled

                from sys.check_constraints cc
            "
        ).ReadPocos<CheckConstraintSqlDefinition>(conn);
}