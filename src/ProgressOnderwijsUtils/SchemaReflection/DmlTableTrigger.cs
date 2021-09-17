using Microsoft.Data.SqlClient;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed record DmlTableTriggerSqlDefinition(DbObjectId ObjectId, string Name, DbObjectId TableObjectId, string Definition) : IWrittenImplicitly
    {
        public static DmlTableTriggerSqlDefinition[] LoadAll(SqlConnection conn)
            => SQL(
                $@"
                    select
                        ObjectId = tr.object_id
                        , tr.name
                        , TableObjectId = t.object_id
                        , Definition = OBJECT_DEFINITION(tr.object_id)
                    from sys.triggers tr
                    join sys.tables t on t.object_id = tr.parent_id
                    where 1=1
                        and tr.parent_class = 1
                "
            ).ReadPocos<DmlTableTriggerSqlDefinition>(conn);
    }
}
