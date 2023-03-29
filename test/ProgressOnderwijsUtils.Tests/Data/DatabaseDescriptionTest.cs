namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class DatabaseDescriptionTest : TransactedLocalConnection
{
    [Fact]
    public void AllDependantTables_works_recursively()
    {
        var db = CreateSampleData();

        var SomeDataWithDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithDefault");
        var SomeDataWithoutDefault_metadata = db.GetTableByName("dbo.ForeignKeyLookupLevel").Columns.Single(c => c.ColumnName == "SomeDataWithoutDefault");

        PAssert.That(() => SomeDataWithDefault_metadata.DefaultValueConstraint != null);
        PAssert.That(() => SomeDataWithoutDefault_metadata.DefaultValueConstraint == null);
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
        PAssert.That(() => fk2.QualifiedName == "dbo.LeafToLevel");
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

        PAssert.That(() => db.TryGetTableById(constraint.TableObjectId).AssertNotNull().UnqualifiedName == "CheckConstraintTest");
        PAssert.That(() => constraint.Definition == "([Test]<>(0))");
        PAssert.That(() => constraint.Name == "ck_TestConstraint");

        var table = db.AllTables.Single(t => t.QualifiedName == "dbo.CheckConstraintTest");
        var constraint2 = table.CheckConstraints.Single(c => c.Name == "ck_TestConstraint");

        PAssert.That(() => constraint2.Definition == "([Test]<>(0))");
    }

    [Fact]
    public void CheckTableTriggers_works()
    {
        SQL($"create table dbo.TableTriggerTest (Iets int null)").ExecuteNonQuery(Connection);
        var definition = @"create trigger dbo.EenTrigger on dbo.TableTriggerTest for insert as
begin
    do_nothing:
end;";
        SQL($"{ParameterizedSql.CreateDynamic(definition)}").ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var table = db.GetTableByName("dbo.TableTriggerTest");
        var trigger = table.Triggers.Single();

        PAssert.That(() => trigger.Name == "EenTrigger");
        PAssert.That(() => trigger.TableObjectId == table.ObjectId);
        PAssert.That(() => trigger.Definition == definition);
    }

    [Fact]
    public void SequencesAreLoadedAsExpected()
    {
        SQL($"create sequence dbo.TestSeq as Int minvalue 1").ExecuteNonQuery(Connection);
        var db = DatabaseDescription.LoadFromSchemaTables(Connection);

        var expected = new SequenceSqlDefinition("dbo.TestSeq", SqlSystemTypeId.Int, false, true, null, 1, 1, int.MaxValue);

        PAssert.That(() => db.Sequences["DBO.TESTSEQ"] == expected);
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

        PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testint").IsString);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testchar").IsString);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testvarchar").IsString);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnchar").IsString);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnvarchar").IsString);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testxml").IsString);

        PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testint").IsUnicode);
        PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testchar").IsUnicode);
        PAssert.That(() => !table.Columns.Single(c => c.ColumnName == "Testvarchar").IsUnicode);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnchar").IsUnicode);
        PAssert.That(() => table.Columns.Single(c => c.ColumnName == "Testnvarchar").IsUnicode);
    }

    [Fact]
    public void DefaultValueConstraint_LoadOK()
    {
        SQL(
            $@"
                create table dbo.CheckDefaultValues (
                    SomeValue int not null
                );
                alter table dbo.CheckDefaultValues add constraint df_SomeValue default (42) for SomeValue;
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);

        var table = db.TryGetTableByName("dbo.CheckDefaultValues").AssertNotNull();
        var column = table.Columns.Single();
        var constraint = column.DefaultValueConstraint.AssertNotNull();

        PAssert.That(() => constraint.Name == "df_SomeValue");
        PAssert.That(() => constraint.Definition == "((42))");
    }

    [Fact]
    public void ComputedColumns_LoadOK()
    {
        SQL(
            $@"
                create table dbo.CheckComputedValues (
                    Bla int not null,
                    SomeValue as (42) persisted not null,
                    SomeOtherValue as (cast(null as nvarchar(max)))
                );
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);

        var columns = db.TryGetTableByName("dbo.CheckComputedValues").AssertNotNull().Columns;

        PAssert.That(() => columns[1].ComputedAs.AssertNotNull().Definition == "((42))");
        PAssert.That(() => columns[1].ComputedAs.AssertNotNull().IsPersisted);
        PAssert.That(() => columns[1].UserTypeId == SqlSystemTypeId.Int);
        PAssert.That(() => columns[1].IsNullable == false);
        PAssert.That(() => columns[2].ComputedAs.AssertNotNull().Definition == "(CONVERT([nvarchar](max),NULL))");
        PAssert.That(() => columns[2].ComputedAs.AssertNotNull().IsPersisted == false);
        PAssert.That(() => columns[2].UserTypeId == SqlSystemTypeId.NVarChar);
        PAssert.That(() => columns[2].IsNullable == true);
    }

    [Fact]
    public void User_table_dbo_dtproperties_is_not_part_of_the_db_description()
    {
        var db = DatabaseDescription.LoadFromSchemaTables(Connection);

        PAssert.That(() => db.TryGetTableByName("dbo.dtproperties") == null);
    }
}
