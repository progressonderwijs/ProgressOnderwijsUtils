#nullable disable
using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SchemaReflection;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public sealed class CascadedDeleteTest : TransactedLocalConnection
    {
        enum AId
        {
            One = 1,
            Two = 2,
        }

        [Fact]
        public void CascadedDeleteFollowsAForeignKey()
        {
            SQL($@"
                create table T1 ( A int primary key, B int);
                create table T2 ( C int primary key, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            ").ExecuteNonQuery(Connection);

            var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);

            PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333 }));

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);
            var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, null, "A", AId.One, AId.Two);

            var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Connection);
            PAssert.That(() => finalDependentValues.SetEqual(new[] { 333 }));
            PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1" }));
        }

        [Fact]
        public void CascadedDeleteWorksWithIdentityKey()
        {
            DataTable PksToDelete(string name, params int[] pks)
            {
                var table = new DataTable();
                table.Columns.Add(name, typeof(int));
                foreach (var pk in pks) {
                    table.Rows.Add(pk);
                }
                return table;
            }

            SQL($@"
                create table T1 ( A int primary key identity(1,1), B int);

                insert into T1 values (11), (22), (33);
            ").ExecuteNonQuery(Connection);

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
            SQL($@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table T2 ( D int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C), D int references T2 (D) );
            ").ExecuteNonQuery(Connection);

            SQL($@"
                insert into TRoot values (1,11), (2,22), (3, 33);
                insert into T1 values (4,1), (5, 2);
                insert into T2 values (4,2), (5, 3);

                insert into TLeaf values (1, 4, null), (2, null, 4), (3, null, 5), (4, null, null);
            ").ExecuteNonQuery(Connection);

            var initialTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);

            PAssert.That(() => initialTLeafKeys.SetEqual(new[] { 1, 2, 3, 4 }));

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);
            var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.TRoot"), true, null, null, new RootId { Root = 1, }, new RootId { Root = 2 });

            var finalT2 = SQL($"select D from T2").ReadPlain<int>(Connection);
            PAssert.That(() => finalT2.SetEqual(new[] { 5 }));

            var finalTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Connection);
            PAssert.That(() => finalTLeafKeys.SetEqual(new[] { 3, 4 }));

            var rowsFromT1 = deletionReport.Where(t => t.Table == "dbo.T1").ToArray();
            PAssert.That(() => rowsFromT1.Single().DeletedRows.Rows.Cast<DataRow>().Select(dr => (int)dr["C"]).SetEqual(new[] { 4, 5 }));
        }

        [Fact]
        public void CascadeDeleteBreaksOnSpecificTable()
        {
            SQL($@"
                create table T1 (A int primary key);
                create table T2 (B int primary key, A int references T1 (A) on delete set null);
                create table T3 (C int primary key, A int references T1 (A));

                insert into T1 values (1);
                insert into T2 values (2, 1);
                insert into T3 values (3, 1);
            ").ExecuteNonQuery(Connection);

            bool StopCascading(string onTable)
                => onTable == "dbo.T2";

            var db = DatabaseDescription.LoadFromSchemaTables(Connection);
            var deletionReport = CascadedDelete.RecursivelyDelete(Connection, db.GetTableByName("dbo.T1"), false, null, StopCascading, "A", AId.One);

            PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T3", "dbo.T1" }));
        }
    }
}
