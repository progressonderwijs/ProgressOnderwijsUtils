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
            var table = db.TryGetTableById(constraint.TableObjectId).AssertNotNull();

            PAssert.That(() => table.UnqualifiedName == "CheckConstraintTest");
            PAssert.That(() => constraint.Definition == "([Test]<>(0))");
            PAssert.That(() => constraint.Name == "ck_TestConstraint");
        }

        [Fact]
        public void CheckIsStringAndIsUnicode_works()
        {
            SQL($@"
                create table dbo.CheckIsString (
                    IdRoot int not null primary key,
                    Testint int not null,
                    Testchar char(10) not null,
                    Testvarchar varchar(10) not null,
                    Testnchar nchar(10) not null,
                    Testnvarchar nvarchar(10) not null,
                    Testxml xml not null
                );
            ").ExecuteNonQuery(Connection);

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);

            var table = db.TryGetTableByName("dbo.CheckIsString").AssertNotNull();

            PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testint").Is_String);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testchar").Is_String);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testvarchar").Is_String);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnchar").Is_String);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnvarchar").Is_String);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testxml").Is_String);

            PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testint").Is_Unicode);
            PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testchar").Is_Unicode);
            PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testvarchar").Is_Unicode);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnchar").Is_Unicode);
            PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnvarchar").Is_Unicode);
        }
    }
}
