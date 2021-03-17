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
            var db = CreateSampleData();

            var SomeDataWithDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithDefault").ColumnMetaData;
            var SomeDataWithoutDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithoutDefault").ColumnMetaData;

            PAssert.That(() => SomeDataWithDefault_metadata.HasDefaultValue);
            PAssert.That(() => !SomeDataWithoutDefault_metadata.HasDefaultValue);
        }

        DatabaseDescription CreateSampleData()
        {
            SQL(
                $@"
                create table dbo.ForeignKeyLookupRoot (
                    IdRoot int not null primary key
                );

                create table dbo.ForeignKeyLookupLevel (
                    IdLevel int not null primary key
                    , IdRoot int not null constraint LevelToRoot foreign key references dbo.ForeignKeyLookupRoot(IdRoot)
                    , SomeDataWithDefault nvarchar(max) not null default('test')
                    , SomeDataWithoutDefault nvarchar(max) not null
                );

                create table dbo.ForeignKeyLookupLeaf (
                    IdLeaf int not null primary key
                    , IdLevel int not null constraint LeafToLevel foreign key references dbo.ForeignKeyLookupLevel(IdLevel) on delete cascade
                );
            "
            ).ExecuteNonQuery(Connection);
            var db = DatabaseDescription.LoadFromSchemaTables(Connection);
            return db;
        }

        [Fact]
        public void CanFindForeignKeysByUnqualifiedName()
        {
            var db = CreateSampleData();
            var fk1 = db.ForeignKeyConstraintsByUnqualifiedName["LevelToRoot"].Single();
            PAssert.That(() => fk1.ReferencedParentTable.QualifiedName == "dbo.ForeignKeyLookupRoot");
            PAssert.That(() => fk1.Columns.Single().ReferencedParentColumn.ColumnName == "IdRoot");
            PAssert.That(() => fk1.Columns.Single().ReferencingChildColumn.ColumnName == "IdRoot");
            var fk2 = fk1.ReferencingChildTable.KeysFromReferencingChildren.Single();
            PAssert.That(() => fk2.ReferencedParentTable.QualifiedName == "dbo.ForeignKeyLookupLevel");
            PAssert.That(() => fk2.Columns.Single().ReferencedParentColumn.ColumnName == "IdLevel");
            PAssert.That(() => fk2.Columns.Single().ReferencingChildColumn.ColumnName == "IdLevel");
        }

        [Fact]
        public void CheckConstraintWithTable_works()
        {
            SQL(
                $@"
                create table dbo.CheckConstraintTest (
                    IdRoot int not null primary key,
                    Test int not null
                );

                alter table dbo.CheckConstraintTest add constraint ck_TestConstraint check (Test <> 0);
            "
            ).ExecuteNonQuery(Connection);

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);

            var allConstraints = db.AllCheckConstraints;

            var constraint = allConstraints.Single(c => c.Name == "ck_TestConstraint");

            PAssert.That(() => constraint.Table.UnqualifiedName == "CheckConstraintTest");
            PAssert.That(() => constraint.Definition == "([Test]<>(0))");
            PAssert.That(() => constraint.Name == "ck_TestConstraint");

            var table = db.AllTables.Single(t => t.QualifiedName == "dbo.CheckConstraintTest");
            var constraint2 = table.CheckConstraints.Single(c => c.Name == "ck_TestConstraint");

            PAssert.That(() => constraint2.Definition == "([Test]<>(0))");
        }

        [Fact]
        public void CheckIsStringAndIsUnicode_works()
        {
            SQL(
                $@"
                create table dbo.CheckIsString (
                    IdRoot int not null primary key,
                    Testint int not null,
                    Testchar char(10) not null,
                    Testvarchar varchar(10) not null,
                    Testnchar nchar(10) not null,
                    Testnvarchar nvarchar(10) not null,
                    Testxml xml not null
                );
            "
            ).ExecuteNonQuery(Connection);

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
