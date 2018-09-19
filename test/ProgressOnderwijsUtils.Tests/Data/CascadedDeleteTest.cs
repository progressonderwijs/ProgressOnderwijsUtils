using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public class CascadedDeleteTest : TransactedLocalConnection
    {
        [NotNull]
        static DataTable PksToDelete(string name, [NotNull] params int[] pks)
        {
            var table = new DataTable();
            table.Columns.Add(name, typeof(int));
            foreach (var pk in pks) {
                table.Rows.Add(pk);
            }
            return table;
        }

        [Fact]
        public void CascadedDeleteFollowsAForeignKey()
        {
            SQL($@"
                create table T1 ( A int primary key, B int);
                create table T2 ( C int primary key, A int references T1 (A) );

                insert into T1 values (1,11), (2,22), (3, 33);
                insert into T2 values (111,1), (333,3);
            ").ExecuteNonQuery(Context);

            var initialDependentValues = SQL($"select C from T2").ReadPlain<int>(Context);

            PAssert.That(() => initialDependentValues.SetEqual(new[] { 111, 333 }));

            var deletionReport = CascadedDelete.RecursivelyDelete(Context, SQL($"T1"), false, null, PksToDelete("A", 1, 2));

            var finalDependentValues = SQL($"select C from T2").ReadPlain<int>(Context);
            PAssert.That(() => finalDependentValues.SetEqual(new[] { 333 }));
            PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T2", "dbo.T1" }));
        }

        void CreateDiamondFkTableSet()
        {
            SQL($@"
                create table TRoot ( root int primary key, B int);
                create table T1 ( C int primary key, root int references TRoot (root));
                create table T2 ( D int primary key, root int references TRoot (root));
                create table TLeaf ( Z int primary key, C int references T1 (C), D int references T2 (D) );
            ").ExecuteNonQuery(Context);
        }

        [Fact]
        public void CascadedDeleteWorksWithIdentityKey()
        {
            SQL($@"
                create table T1 ( A int primary key identity(1,1), B int);

                insert into T1 values (11), (22), (33);
            ").ExecuteNonQuery(Context);

            var deletionReport = CascadedDelete.RecursivelyDelete(Context, SQL($"T1"), false, null, PksToDelete("A", 1, 2));
            var finalValues = SQL($"select B from T1").ReadPlain<int>(Context);

            PAssert.That(() => deletionReport.Select(t => t.Table).SequenceEqual(new[] { "dbo.T1" }));
            PAssert.That(() => finalValues.SetEqual(new[] { 33 }));
        }

        [Fact]
        public void CascadedDeleteFollowsADiamondOfForeignKey()
        {
            CreateDiamondFkTableSet();

            SQL($@"
                insert into TRoot values (1,11), (2,22), (3, 33);
                insert into T1 values (4,1), (5, 2);
                insert into T2 values (4,2), (5, 3);

                insert into TLeaf values (1, 4, null), (2, null, 4), (3, null, 5), (4, null, null);
            ").ExecuteNonQuery(Context);

            var initialTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Context);

            PAssert.That(() => initialTLeafKeys.SetEqual(new[] { 1, 2, 3, 4 }));

            var deletionReport = CascadedDelete.RecursivelyDelete(Context, SQL($"TRoot"), true, null, PksToDelete("Root", 1, 2));

            var finalT2 = SQL($"select D from T2").ReadPlain<int>(Context);
            PAssert.That(() => finalT2.SetEqual(new[] { 5 }));

            var finalTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Context);
            PAssert.That(() => finalTLeafKeys.SetEqual(new[] { 3, 4 }));

            var rowsFromT1 = deletionReport.Where(t => t.Table == "dbo.T1").ToArray();
            PAssert.That(() => rowsFromT1.Single().DeletedRows.Rows.Cast<DataRow>().Select(dr => (int)dr["C"]).SetEqual(new[] { 4, 5 }));
        }
    }
}
