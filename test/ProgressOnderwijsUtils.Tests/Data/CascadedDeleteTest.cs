using System;
using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data;

public sealed class CascadedDeleteTest : TransactedLocalConnection
{
    enum AId { One = 1, Two = 2, }

    [Fact]
    public void CascadedDeleteFollowsAForeignKey()
    {
        SQL(
            $@"
                create table T1 ( A int primary key, B int);
                create table T2 ( C int primary key, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333 }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two);

        var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalDependentValues.SetEqual(new[] { 333 }));
        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1" }));
    }

    [Fact]
    public void CascadedDeleteFollowsAUniqueConstraintBasedForeignKey()
    {
        SQL(
            $@"
                create table T1 ( A int not null unique, B int);
                create table T2 ( C int not null, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333 }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two);

        var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalDependentValues.SetEqual(new[] { 333 }));
        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1" }));
    }

    [Fact]
    public void CascadedDeleteDetectsCycles()
    {
        SQL(
            $@"
                create table T1 ( A int not null unique, B int);
                create table T2 ( B int not null unique, A int references T1 (A) );
                alter table T1 add constraint T1FK foreign key (B) references T2(B);

                insert into T1 values (1,null), (2,null), (3, null);
                insert into T2 values (111,1), (333,3);
                
                update T1 set B = 111 where A = 1;
                update T1 set B = 333 where A = 3;

            "
        ).ExecuteNonQuery(Connection);

        var initialDependentValues = SQL($"select B from T2").ReadPlain<int>(Connection);

        PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333 }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var ex = Assert.ThrowsAny<Exception>(() => CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two));
        PAssert.That(() => ex.Message.Contains("dbo.T2->dbo.T1->dbo.T2->dbo.T1"));
    }

    [Fact]
    public void CascadedDeleteWorksWithIdentityKey()
    {
        DataTable PksToDelete(string name, params int[] pks)
        {
            var table = new DataTable();
            _ = table.Columns.Add(name, typeof(int));
            foreach (var pk in pks) {
                _ = table.Rows.Add(pk);
            }
            return table;
        }

        SQL(
            $@"
                create table T1 ( A int primary key identity(1,1), B int);

                insert into T1 values (11), (22), (33);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, PksToDelete("A", 1, 2));
        var finalValues = SQL($"select B from T1").ReadPlain<int>(Connection);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T1" }));
        PAssert.That(() => finalValues.SetEqual(new[] { 33 }));
    }

    public struct RootId : IWrittenImplicitly, IReadImplicitly
    {
        public int Root { get; set; }
    }

    [Fact]
    public void CascadedDeleteFollowsADiamondOfForeignKey()
    {
        SQL(
            $@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table T2 ( D int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C), D int references T2 (D) );
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into TRoot values (1,11), (2,22), (3, 33);
                insert into T1 values (4,1), (5, 2);
                insert into T2 values (4,2), (5, 3);

                insert into TLeaf values (1, 4, null), (2, null, 4), (3, null, 5), (4, null, null);
            "
        ).ExecuteNonQuery(Connection);

        var initialTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);

        PAssert.That(() => initialTLeafKeys.SetEqual(new[] { 1, 2, 3, 4 }));

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.TRoot"), true, null, null, new RootId { Root = 1, }, new RootId { Root = 2 });

        var finalT2 = SQL($"select D from T2").ReadPlain<int>(Connection);
        PAssert.That(() => finalT2.SetEqual(new[] { 5 }));

        var finalTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);
        PAssert.That(() => finalTLeafKeys.SetEqual(new[] { 3, 4 }));

        var rowsFromT1 = deletionReport.Where(t => t.Table == "dbo.T1").ToArray();
        PAssert.That(() => rowsFromT1.Single().DeletedRows!.Rows.Cast<DataRow>().Select(dr => (int)dr["C"]).SetEqual(new[] { 4, 5 }));
    }

    [Fact]
    public void NonUniqueForeignKeyDoesNotLeadToExplosionOfDeletions()
    {
        SQL(
            $@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C));
            "
        ).ExecuteNonQuery(Connection);

        SQL(
            $@"
                insert into TRoot values (1, 11), (2, 22), (3, 33);
                insert into T1 values (1,1), (2, 2), (3, 3), (11,1), (22,2), (33,3);

                insert into TLeaf values (1, 1);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.TRoot"), true, null, null, new RootId { Root = 1, });

        var rowsFromTRoot = deletionReport.Single(t => t.Table == "dbo.TRoot");
        PAssert.That(() => rowsFromTRoot.DeletedAtMostRowCount == 1);
        var rowsFromT1 = deletionReport.Single(t => t.Table == "dbo.T1");
        PAssert.That(() => rowsFromT1.DeletedAtMostRowCount == 2);
        var rowsFromTLeaf = deletionReport.Single(t => t.Table == "dbo.TLeaf");
        PAssert.That(() => rowsFromTLeaf.DeletedAtMostRowCount == 1);
    }

    [Fact]
    public void CascadeDeleteBreaksOnSpecificTable()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1 (A) on delete set null);
                create table T3 (C int primary key, A int references T1 (A));

                insert into T1 values (1);
                insert into T2 values (2, 1);
                insert into T3 values (3, 1);
            "
        ).ExecuteNonQuery(Connection);

        bool StopCascading(string onTable)
            => onTable == "dbo.T2";

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, StopCascading, "A", AId.One);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T3", "dbo.T1" }));
    }

    [Fact]
    public void CascadeDeleteFollowsNullableForeignKeyAndThenSameTableForeignKey()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1, C int references T2);

                insert into T1 values (1);
                insert into T2 values (2, 1, null);
                insert into T2 values (3, null, 2);
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T2", "dbo.T1" }));
    }

    [Fact]
    public void CascadeDeleteAllowsSameTableCyclesWhenThoseAreSimultaneouslyDeletable()
    {
        SQL(
            $@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1, C int references T2);

                insert into T1 values (1);
                insert into T2 values (2, 1, null);
                insert into T2 values (3, 1, 2);
                update T2 set C=3 where B=2
            "
        ).ExecuteNonQuery(Connection);

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One);

        PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1" }));
    }

    [Fact]
    public void CascadeDelete_also_reports_on_tables_with_rowversion_column()
    {
        SQL($"create table T1 (A int primary key, V rowversion);").ExecuteNonQuery(Connection);
        SQL($"insert into T1 (A) values (1);").ExecuteNonQuery(Connection);

        var version = SQL($"select t.V from T1 t").ReadScalar<byte[]>(Connection).AssertNotNull();

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), true, null, null, "A", AId.One).Single();
        var deletedRow = deletionReport.DeletedRows.AssertNotNull().Rows.Cast<DataRow>().Single();

        PAssert.That(() => deletionReport.Table == "dbo.T1");
        PAssert.That(() => ((byte[])deletedRow["V"]).SequenceEqual(version));
    }

    [Fact]
    public void CascadeDelete_also_reports_on_tables_with_triggers_and_rowversion_column()
    {
        SQL($"create table T1 (A int primary key, V rowversion);").ExecuteNonQuery(Connection);
        SQL($"create trigger T1t on T1 after delete as insert into T1 (A) values (2);").ExecuteNonQuery(Connection);
        SQL($"insert into T1 (A) values (1);").ExecuteNonQuery(Connection);

        var version = SQL($"select t.V from T1 t").ReadScalar<byte[]>(Connection).AssertNotNull();

        var db = DatabaseDescription.LoadFromSchemaTables(Connection);
        var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), true, null, null, "A", AId.One).Single();
        var deletedRow = deletionReport.DeletedRows.AssertNotNull().Rows.Cast<DataRow>().Single();

        PAssert.That(() => deletionReport.Table == "dbo.T1");
        PAssert.That(() => ((byte[])deletedRow["V"]).SequenceEqual(version));
    }
}