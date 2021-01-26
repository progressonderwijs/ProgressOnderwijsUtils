using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class DatabaseDescriptionTest : TransactedLocalConnection
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
                    , SomeDataWithDefault nvarchar(max) not null default('test')
                    , SomeDataWithoutDefault nvarchar(max) not null
                );

                create table dbo.ForeignKeyLookupLeaf (
                    IdLeaf int not null primary key
                    , IdLevel int not null foreign key references dbo.ForeignKeyLookupLevel(IdLevel)
                );
            ").ExecuteNonQuery(Connection);
            var db = DatabaseDescription.LoadFromSchemaTables(Connection);

            var SomeDataWithDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithDefault").ColumnMetaData;
            var SomeDataWithoutDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithoutDefault").ColumnMetaData;

            PAssert.That(() => SomeDataWithDefault_metadata.HasDefaultValue);
            PAssert.That(() => !SomeDataWithoutDefault_metadata.HasDefaultValue);
        }

        [Fact]
        public void CheckConstraintWithTable_works()
        {
            SQL($@"
                create table dbo.CheckConstraintTest (
                    IdRoot int not null primary key,
                    Test int not null
                );

                alter table dbo.CheckConstraintTest add constraint ck_TestConstraint check (Test <> 0);
            ").ExecuteNonQuery(Connection);

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);

            var allConstraints = db.AllCheckConstraints;

            var constraint = allConstraints.Single(c => c.Name == "ck_TestConstraint");
            var table = db.TryGetTableById(constraint.tableId).AssertNotNull();

            PAssert.That(() => table.UnqualifiedName == "CheckConstraintTest");
            PAssert.That(() => constraint.Definition == "([Test]<>(0))");
            PAssert.That(() => constraint.Name == "ck_TestConstraint");
        }
    }
}
