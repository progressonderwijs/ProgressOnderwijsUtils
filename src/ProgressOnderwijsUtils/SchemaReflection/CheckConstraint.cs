using System.Linq;
using Microsoft.Data.SqlClient;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public sealed record CheckConstraintEntry(DbObjectId CheckConstraintObjectId, string Name, string Definition, DbObjectId TableObjectId) : IWrittenImplicitly
    {
        public static CheckConstraintEntry[] LoadAll(SqlConnection conn)
            => SQL($@"
                select 
                    CheckConstraintObjectId = cc.object_id
                    , Name = cc.name
                    , Definition = cc.definition
                    , TableObjectId = cc.parent_object_id
                from sys.check_constraints cc
            ").ReadPocos<CheckConstraintEntry>(conn);
    }

    public sealed record CheckConstraint(CheckConstraintEntry CheckConstraintEntry, DatabaseDescription.Table Table)
    {
        public DbObjectId CheckConstraintObjectId
            => CheckConstraintEntry.CheckConstraintObjectId;

        public string Name
            => CheckConstraintEntry.Name;

        public string Definition
            => CheckConstraintEntry.Definition;
    }
}
