using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class FilterTest
	{
		[Test]
		public void BasicChecks()
		{
#pragma warning disable 1720
			PAssert.That(() => default(FilterBase).ToSqlString(default(Dictionary<string, string>)) == QueryBuilder.Create("1=1"));//shouldn't throw and should be equal
#pragma warning restore 1720

			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).ToSqlString(s => s) == QueryBuilder.Create("test<{0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test<={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, 3).ToSqlString(s => s) == QueryBuilder.Create("test={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test>={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3).ToSqlString(s => s) == QueryBuilder.Create("test>{0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3).ToSqlString(s => s) == QueryBuilder.Create("test!={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Contains, "world").ToSqlString(s => s) == QueryBuilder.Create("test like {0}", "%world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world").ToSqlString(s => s) == QueryBuilder.Create("test like {0}", "world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNull, null).ToSqlString(s => s) == QueryBuilder.Create("test is null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null).ToSqlString(s => s) == QueryBuilder.Create("test is not null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.In, "1 2 3 4 5 ").ToSqlString(s => s) == QueryBuilder.Create("test in ({0}, {1}, {2}, {3}, {4})", Enumerable.Range(1, 5).Select(i => i.ToString()).Cast<object>().ToArray()));
		}

		[Test]
		public void ColRename()
		{
			var combFilter = Filter.CreateCombined(BooleanOperator.And, Filter.CreateCriterium("test", BooleanComparer.LessThan, 3), Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3));

			PAssert.That(() => combFilter.ToSqlString(new Dictionary<string, string> { { "test", "ziggy" } }) == QueryBuilder.Create("(ziggy<{0} And test2<{1})", 3, 3));
		}



		[Test]
		public void CreateCombinedWithNulls()
		{
			var blaFilter = Filter.CreateCombined(BooleanOperator.And, Filter.CreateCriterium("test", BooleanComparer.LessThan, 3), Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3));

			PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, null,null) ==null);
			PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, blaFilter, null) == blaFilter);
		}

		[Test]
		public void ExtractInsertWaarden()
		{
			var blaFilter = Filter.CreateCombined(BooleanOperator.And, Filter.CreateCriterium("test", BooleanComparer.Equal, 3), Filter.CreateCriterium("test2", BooleanComparer.Equal, 37));
			var blaFilter2 = Filter.CreateCombined(BooleanOperator.Or, Filter.CreateCriterium("test", BooleanComparer.Equal, 3), Filter.CreateCriterium("test2", BooleanComparer.Equal, 37));

			PAssert.That(() => blaFilter.ExtractInsertWaarden().SequenceEqual(new[] { Tuple.Create("test", (object)3), Tuple.Create("test2", (object)37) }));
			PAssert.That(() => !blaFilter2.ExtractInsertWaarden().Any());
		}


		[Test]
		public void FilterModification()
		{
			var testFilter = Filter.CreateCriterium("test", BooleanComparer.LessThan, 3);
			var test2Filter = Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3);
			var combFilter = Filter.CreateCombined(BooleanOperator.And, testFilter, test2Filter);
			PAssert.That(() => combFilter.ToSqlString(s => s) == QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3)); //initial check

			var modFilter =
					combFilter
					.Replace(testFilter, Filter.CreateCriterium("ziggy", BooleanComparer.LessThan, 3))
					.AddTo(test2Filter, BooleanOperator.And, Filter.CreateCriterium("stardust",BooleanComparer.GreaterThan,37));

			PAssert.That(() => modFilter.ToSqlString(s => s) == QueryBuilder.Create("(ziggy<{0} And test2<{1} And stardust>{2})", 3, 3,37)); //note no nested brackets!
			PAssert.That(() => combFilter.ToSqlString(s => s) == QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3)); // side-effect free

			PAssert.That(() =>
				combFilter.AddTo(testFilter, BooleanOperator.Or, Filter.CreateCriterium("abc", BooleanComparer.GreaterThan, 42)).ToSqlString(s=>s)
				== QueryBuilder.Create("((test<{0} Or abc>{1}) And test2<{2})", 3, 42, 3)); //does include nested brackets!

			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s != "stardust") == null);
			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s != "stardst") == modFilter);
		}


	}
}
