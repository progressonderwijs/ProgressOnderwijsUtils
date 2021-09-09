using Microsoft.Data.SqlClient;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed record ComputedColumnSqlDefinition(DbObjectId ObjectId, DbColumnId ColumnId, string Definition, bool IsPersisted) : IWrittenImplicitly
    {
        public static ComputedColumnSqlDefinition[] LoadAll(SqlConnection conn)
            => SQL(
                $@"
                    select 
                        ObjectId = c.object_id
                        , ColumnId = c.column_id
                        , c.Definition
                        , IsPersisted = c.is_persisted
                    from sys.computed_columns c
                "
            ).ReadPocos<ComputedColumnSqlDefinition>(conn);
    }
}
