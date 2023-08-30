namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record TriggerSqlDefinition(DbObjectId ObjectId, string Name, DbObjectId? TableObjectId, string Definition) : IWrittenImplicitly
{
    public static TriggerSqlDefinition[] LoadAllDmlTableTriggers(SqlConnection conn)
        => LoadAll(conn, 1);

    public static TriggerSqlDefinition[] LoadAllDatabaseTriggers(SqlConnection conn)
        => LoadAll(conn, 0);

    static TriggerSqlDefinition[] LoadAll(SqlConnection conn, int parentClass)
        => SQL(
            $@"
                    select
                        ObjectId = tr.object_id
                        , tr.name
                        , TableObjectId = t.object_id
                        , Definition = OBJECT_DEFINITION(tr.object_id)
                    from sys.triggers tr
                    left join sys.tables t on t.object_id = tr.parent_id
                    where 1=1
                        and tr.parent_class = {parentClass}
                "
        ).ReadPocos<TriggerSqlDefinition>(conn);
}
