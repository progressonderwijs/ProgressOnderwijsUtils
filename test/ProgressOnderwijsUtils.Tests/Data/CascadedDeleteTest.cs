﻿using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests.Data
{
    public class CascadedDeleteTest : TransactedLocalConnection
    {
        public struct AId : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int A { get; set; }
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

            var deletionReport = CascadedDelete.RecursivelyDelete(Context, SQL($"T1"), false, null, new AId { A = 1 }, new AId { A = 2 });

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

        public struct RootId : IMetaObject, IPropertiesAreUsedImplicitly
        {
            public int Root { get; set; }
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

            var deletionReport = CascadedDelete.RecursivelyDelete(Context, SQL($"TRoot"), true, null, new RootId { Root = 1, }, new RootId { Root = 2 });

            var finalT2 = SQL($"select D from T2").ReadPlain<int>(Context);
            PAssert.That(() => finalT2.SetEqual(new[] { 5 }));

            var finalTLeafKeys = SQL($"select Z from TLeaf").ReadPlain<int>(Context);
            PAssert.That(() => finalTLeafKeys.SetEqual(new[] { 3, 4 }));

            var rowsFromT1 = deletionReport.Where(t => t.Table == "dbo.T1").ToArray();
            PAssert.That(() => rowsFromT1.Single().DeletedRows.Rows.Cast<DataRow>().Select(dr => (int)dr["C"]).SetEqual(new[] { 4, 5 }));
        }
    }
}
