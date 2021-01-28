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
    }

    public sealed class CheckConstraint
    {
        readonly CheckConstraintEntry checkConstraintEntry;
        readonly DatabaseDescription.Table table;

        public CheckConstraint(CheckConstraintEntry entry, DatabaseDescription.Table table)
        {
            checkConstraintEntry = entry;
            this.table = table;
        }

        public DbObjectId CheckConstraintObjectId
            => checkConstraintEntry.CheckConstraintObjectId;

        public string Name
            => checkConstraintEntry.Name;

        public string Definition
            => checkConstraintEntry.Definition;

        public DatabaseDescription.Table Table
            => table;

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
}
