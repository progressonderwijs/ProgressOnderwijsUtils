using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Data;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture, NightlyOnly]
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
		public void BooleanComparers()
		{
			var comparers = Enum.GetValues(typeof(BooleanComparer)).Cast<BooleanComparer>();
			PAssert.That(() => comparers.Count() == comparers.Select(c => c.NiceString()).Distinct().Count());//all nicestrings don't throw and are distinct
			PAssert.That(() => comparers.OrderBy(x => x).SequenceEqual(CriteriumFilter.NumericComparers.Concat(CriteriumFilter.StringComparers).Distinct().OrderBy(x => x)));//all comparers either numeric or string or both.
		}

		[Test]
		public void CreateCombinedWithNulls()
		{
			var blaFilter = Filter.CreateCombined(BooleanOperator.And, Filter.CreateCriterium("test", BooleanComparer.LessThan, 3), Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3));

			PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, null, null) == null);
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
					.AddTo(test2Filter, BooleanOperator.And, Filter.CreateCriterium("stardust", BooleanComparer.GreaterThan, 37));

			PAssert.That(() => modFilter.ToSqlString(s => s) == QueryBuilder.Create("(ziggy<{0} And test2<{1} And stardust>{2})", 3, 3, 37)); //note no nested brackets!
			PAssert.That(() => combFilter.ToSqlString(s => s) == QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3)); // side-effect free
			PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, combFilter, null, modFilter).ToSqlString(s => s) == QueryBuilder.Create("(test<{0} And test2<{1} And ziggy<{2} And test2<{3} And stardust>{4})", 3, 3, 3, 3, 37)); // no unnecessary brackets!

			PAssert.That(() => combFilter.Remove(test2Filter) == testFilter && combFilter.Remove(testFilter) == test2Filter); // no unnecessary brackets!


			PAssert.That(() =>
				combFilter.AddTo(testFilter, BooleanOperator.Or, Filter.CreateCriterium("abc", BooleanComparer.GreaterThan, 42)).ToSqlString(s => s)
				== QueryBuilder.Create("((test<{0} Or abc>{1}) And test2<{2})", 3, 42, 3)); //does include nested brackets!

			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s != "stardust") == null);
			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s != "stardst") == modFilter);
		}

		[Test]
		public void ColRef()
		{
			PAssert.That(() => !BooleanComparer.In.CanReferenceColumn());
			PAssert.That(() => BooleanComparer.Equal.CanReferenceColumn());
		}

		[Test]
		public void QueryBuilderSerializesOk()
		{
			var testFilter = Filter.CreateCriterium("test", BooleanComparer.LessThan, 3);
			var test2Filter = Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3);
			var combFilter = Filter.CreateCombined(BooleanOperator.And, testFilter, test2Filter);

			var q = combFilter.ToSqlString(s => s);
			var qAlt = QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3);
			var qAltWrong = QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3.0);

			PAssert.That(() => q.Serialize() == qAlt.Serialize());
			PAssert.That(() => q.Serialize().GetHashCode() == qAlt.Serialize().GetHashCode());

			PAssert.That(() => q.Equals(qAlt));
			PAssert.That(() => q == qAlt);
			PAssert.That(() => q.GetHashCode() == qAlt.GetHashCode());
			PAssert.That(() => q.ToString() == qAlt.ToString());
			PAssert.That(() => q.Serialize().CommandText == qAlt.Serialize().CommandText);

			PAssert.That(() => qAlt.Serialize() != qAltWrong.Serialize());
			PAssert.That(() => qAlt.Serialize().GetHashCode() != qAltWrong.Serialize().GetHashCode());
			PAssert.That(() => !qAlt.Equals(qAltWrong));
			PAssert.That(() => qAlt != qAltWrong);
			PAssert.That(() => qAlt.GetHashCode() != qAltWrong.GetHashCode());
			PAssert.That(() => qAlt.ToString() != qAltWrong.ToString());
			PAssert.That(() => qAlt.Serialize().CommandText != qAltWrong.Serialize().CommandText);
		}

		[Test]
		public void EmptyQueryBuilders()
		{
			var q = QueryBuilder.Empty;
			var qZeroWidth = QueryBuilder.Create("");
			var qZeroWidthArg = QueryBuilder.Create("", 42);
			var qZeroWidth2 = QueryBuilder.Create(42.ToStringInvariant().Substring(42.ToStringInvariant().Length));
			PAssert.That(() => !ReferenceEquals(qZeroWidth2.CommandText(), qZeroWidth.CommandText()));
			//TODO
		}

		[Test]
		public void QueryBuilderValidation()
		{
			Assert.Throws<ArgumentNullException>(() => QueryBuilder.Create(null));
			Assert.Throws<ArgumentNullException>(() => QueryBuilder.Create(null));
			var abc = (QueryBuilder)"abc";
			Assert.Throws<ArgumentNullException>(() => { var _ = abc + default(string); });
			Assert.Throws<ArgumentNullException>(() => { var _ = default(QueryBuilder) + "abc"; });
		}
	}
}
