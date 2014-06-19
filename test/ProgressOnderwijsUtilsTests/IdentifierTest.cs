using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using Progress.Business.Data.GenericLijst;
using Progress.Business.GenericLijst;

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
			var id = TestId.Create(value);
			Assert.That(id == (TestId)value, Is.True);
			// ReSharper disable EqualExpressionComparison
			Assert.That(id == id, Is.True);
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
			Assert.That((TestId)value, Is.EqualTo(value));
			Assert.That((Identifier<TestId>)value, Is.EqualTo(value));
			Assert.That(TestId.Create(value), Is.EqualTo(value));
		}

		[Test]
		public static void Speed()
		{
			var value1 = (int?)12;
			var value2 = (int?)12;
			var id1 = TestId.Create(12);
			var id2 = TestId.Create(12);

			const int steps = 10000000;
			var stopwatch = new Stopwatch();

			stopwatch.Restart();
			for (var i = 0; i < steps; i++)
			{
				Assert.That(value1, Is.EqualTo(value2));
			}
			var time1 = stopwatch.ElapsedMilliseconds;

			stopwatch.Restart();
			for (var i = 0; i < steps; i++)
			{
				Assert.That(id1, Is.EqualTo(id2));
			}
			var time2 = stopwatch.ElapsedMilliseconds;
			var fraction = Math.Abs((time1 - time2) / time1);
			Assert.That(fraction, Is.LessThan(0.01));
		}

		[Test]
		public void DatabaseScalar()
		{
			Assert.That(QueryBuilder.Create("select 1").ReadScalar<TestId>(conn), Is.EqualTo((TestId)1));
		}

		// ReSharper disable MemberCanBePrivate.Global
		[UsedImplicitly]
		public class TestRow { [UsedImplicitly] public TestId testid; }
		// ReSharper restore MemberCanBePrivate.Global

		[Test]
		public void DatabasePlain()
		{
			var r = QueryBuilder.Create("select 1").ReadPlain<TestRow>(conn);
			Assert.That(r.Count(), Is.EqualTo(1));
			Assert.That(r[0].testid, Is.EqualTo((TestId)1));
		}

		public sealed class TestLijstRij : IMetaObject
		{
			public TestId TestId { get; set; }
		}

		public sealed class TestLijstMpConfig : LijstMpConfig<TestLijstRij>
		{
			protected override LijstDescriptionBuilder StaticLijstDescription()
			{
				return LijstDescriptionBuilder.Build("test");
			}
		}

		public class TestDatasource : AbstractDataSource<TestLijstRij, TestLijstMpConfig>
		{
			protected override QueryBuilder Query()
			{
				return QueryBuilder.Create(@"select testid = 1");
			}
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
	}
}
