using System;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class ForeignKeyLookupTest : TransactedLocalConnection
    {
        [Fact]
        public void AllDependantTables_works_recursively()
        {
            SQL($@"
                create table dbo.ForeignKeyLookupRoot (
                    IdRoot int not null primary key
                );

                create table dbo.ForeignKeyLookupLevel (
                    IdLevel int not null primary key
                    , IdRoot int not null foreign key references dbo.ForeignKeyLookupRoot(IdRoot)
                );

                create table dbo.ForeignKeyLookupLeaf (
                    IdLeaf int not null primary key
                    , IdLevel int not null foreign key references dbo.ForeignKeyLookupLevel(IdLevel)
                );
            ").ExecuteNonQuery(Connection);
            var db = DatabaseDescription.LoadFromSchemaTables(Connection);

            var dependencies = db.GetTableByName("dbo.ForeignKeyLookupRoot").AllDependantTables;

            PAssert.That(() => dependencies.Select(dependency => dependency.QualifiedName).SetEqual(new[] { "dbo.ForeignKeyLookupRoot", "dbo.ForeignKeyLookupLevel", "dbo.ForeignKeyLookupLeaf" }, StringComparer.OrdinalIgnoreCase));
        }
    }
}
