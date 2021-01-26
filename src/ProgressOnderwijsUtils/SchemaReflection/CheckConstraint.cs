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
        public readonly DbObjectId CheckConstraintObjectId;
        public readonly string Name;
        public readonly string Definition;
        public readonly DbObjectId tableId;

        public CheckConstraint(CheckConstraintEntry entry)
        {
            CheckConstraintObjectId = entry.CheckConstraintObjectId;
            Name = entry.Name;
            Definition = entry.Definition;
            tableId = entry.TableObjectId;
        }

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
