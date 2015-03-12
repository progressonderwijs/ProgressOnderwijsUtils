using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Data;
using Progress.Business.GenericEdit;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using Progress.Business.Data.GenericLijst;

namespace ProgressOnderwijsUtilsTests
{
    [TestFixture]
    public class IdentifierTest : WebSessionTestSuiteBase
    {
        public class TestId : Identifier<TestId> { }

        [Test]
        public static void EqualityCheck()
        {
            const int value = 12;
            var id = (TestId)value;
            Assert.That(id == (TestId)value, Is.True);
            // ReSharper disable EqualExpressionComparison
#pragma warning disable 1718
            Assert.That(id == id, Is.True);
#pragma warning restore 1718
            // ReSharper restore EqualExpressionComparison
            Assert.That(id.Equals(value), Is.True);
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            TestId nullTestId = null;
            Assert.That(nullTestId == null, Is.True);
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        [Test]
        public static void Casting()
        {
            const int value = 12;
            Assert.That((int)((TestId)value), Is.EqualTo(value));
            Assert.That((int)((Identifier<TestId>)value), Is.EqualTo(value));
            Assert.That((int)(TestId)value, Is.EqualTo(value));
        }

        [Test]
        public static void Speed()
        {
            var value1 = (int?)12;
            var value2 = (int?)12;
            var id1 = (TestId)12;
            var id2 = (TestId)12;

            const int steps = 10000000;
            var stopwatch = new Stopwatch();

            stopwatch.Restart();
            for (var i = 0; i < steps; i++) {
                Assert.That(value1, Is.EqualTo(value2));
            }
            var time1 = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            for (var i = 0; i < steps; i++) {
                Assert.That(id1, Is.EqualTo(id2));
            }
            var time2 = stopwatch.ElapsedMilliseconds;
            var fraction = Math.Abs((time1 - time2) / time1);
            Assert.That(fraction, Is.LessThan(0.01));
        }

        [Test]
        public void DatabaseScalar() { DatabaseTest("select 1", (TestId)1); }

        [Test]
        public void DatabaseScalarNull() { DatabaseTest("select cast(null as int)", default(TestId)); }

        [Test]
        public void DatabaseScalarEmpty() { DatabaseTest("select 1 where 1 = 2", default(TestId)); }

        void DatabaseTest(string query, TestId expected) { Assert.That(QueryBuilder.Create(query).ReadScalar<TestId>(conn), Is.EqualTo(expected)); }

        // ReSharper disable MemberCanBePrivate.Global
        [UsedImplicitly]
        public class TestRow
        {
            [UsedImplicitly]
            public TestId testid;
        }

        // ReSharper restore MemberCanBePrivate.Global
        [Test]
        public void DatabasePlain()
        {
            var r = QueryBuilder.Create("select 1").ReadPlain<TestId>(conn);
            Assert.That(r.Count(), Is.EqualTo(1));
            Assert.That(r[0], Is.EqualTo((TestId)1));
        }

        public sealed class TestLijstRij : IMetaObject
        {
            public TestId TestId { get; set; }
        }

        public sealed class TestLijstMpConfig : LijstMpConfig<TestLijstRij>
        {
            protected override DataSourceMetaDataBuilder<TestLijstRij> BuildMetaData() { return Build("test"); }
        }

        // ReSharper disable MemberCanBePrivate.Global
        public class TestDatasource : AbstractDataSource<TestLijstRij, TestLijstMpConfig>
            // ReSharper restore MemberCanBePrivate.Global
        {
            protected override QueryBuilder Query() { return QueryBuilder.Create(@"select testid = 1"); }
        }

        [Test]
        public void Datasource()
        {
            var ds = new TestDatasource();
            ds.Init(conn, Session, false);
            var rows = ds.GetAllData(conn, true, false);
            Assert.That(rows.Count, Is.EqualTo(1));
            Assert.That(rows[0].TestId, Is.EqualTo((TestId)1));
        }

        [Test]
        public void ReadMetaObjects()
        {
            var query = QueryBuilder.Create(@"select testid = 1 union all select 2 union all select null");
            var result = query.ReadMetaObjects<TestLijstRij>(conn);
            Assert.That(result.Count(), Is.EqualTo(3));
            Assert.That(result.Count(r => r.TestId == (TestId)1), Is.EqualTo(1));
            Assert.That(result.Count(r => r.TestId == (TestId)2), Is.EqualTo(1));
            Assert.That(result.Count(r => r.TestId == (TestId)(int?)(null)), Is.EqualTo(1));
        }

        [Test]
        public void ReadWithParameters()
        {
            var parameter = (TestId)1;
            var query = QueryBuilder.Create(@"select testid = {0}", parameter);
            var result = query.ReadMetaObjects<TestLijstRij>(conn);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.Count(r => r.TestId == parameter), Is.EqualTo(1));
        }

        [Test]
        public void ReadWithNullParameters()
        {
            var parameter = (TestId)(int?)null;
            // ReSharper disable ExpressionIsAlwaysNull
            var query = QueryBuilder.Create(@"select testid = {0}", parameter);
            var result = query.ReadMetaObjects<TestLijstRij>(conn);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.Count(r => r.TestId == parameter), Is.EqualTo(1));
            // ReSharper restore ExpressionIsAlwaysNull
        }

        [Test]
        public void GenericBusinessEdit()
        {
            var be = new GenericBusinessEdit(Session, "student");
            be.ReadNieuw(conn);
            be.SetId2((RootOrganisatie)(int)RootOrganisatie.UVH);
            be.Values["studentnummer"] = 0;
            be.Values["naam"] = "nvt";
            be.Save(conn);
        }
    }
}
