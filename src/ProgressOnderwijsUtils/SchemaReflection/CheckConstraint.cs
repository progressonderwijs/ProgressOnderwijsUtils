using System.Linq;
using Microsoft.Data.SqlClient;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.SchemaReflection
{
    public struct CheckConstraintEntry : IWrittenImplicitly
    {
        public DbObjectId CheckConstraintObjectId { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public DbObjectId TableObjectId { get; set; }

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
