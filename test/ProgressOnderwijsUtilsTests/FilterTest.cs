using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture, NightlyOnly]
	public sealed class FilterTest
	{
		[Test]
		public void BasicChecks()
		{
#pragma warning disable 1720
			PAssert.That(() => default(FilterBase).ToQueryBuilder() == QueryBuilder.Create("1=1"));//shouldn't throw and should be equal
#pragma warning restore 1720


			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).ToQueryBuilder() == QueryBuilder.Create("test<{0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3).ToQueryBuilder() == QueryBuilder.Create("test<={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, 3).ToQueryBuilder() == QueryBuilder.Create("test={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3).ToQueryBuilder() == QueryBuilder.Create("test>={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3).ToQueryBuilder() == QueryBuilder.Create("test>{0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3).ToQueryBuilder() == QueryBuilder.Create("test!={0}", 3));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Contains, "world").ToQueryBuilder() == QueryBuilder.Create("test like {0}", "%world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world").ToQueryBuilder() == QueryBuilder.Create("test like {0}", "world%"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.EndsWith, "world").ToQueryBuilder() == QueryBuilder.Create("test like {0}", "%world"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNull, null).ToQueryBuilder() == QueryBuilder.Create("test is null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null).ToQueryBuilder() == QueryBuilder.Create("test is not null"));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.In, new[] { 1, 2, 3, 4, 5 }).ToQueryBuilder() == QueryBuilder.Create("test in (select val from {0})", Enumerable.Range(1, 5)));
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { 1, 2, 3, 4, 5 }).ToQueryBuilder() == QueryBuilder.Create("test not in (select val from {0})", Enumerable.Range(1, 5)));
		}
		[Test]
		public void CurrentTimeTest()
		{
			var filter = Filter.CreateCriterium("test", BooleanComparer.Equal, Filter.CurrentTimeToken.Instance);
			DateTime start = DateTime.Now;
			while (DateTime.Now == start) { }//wait until start of next tick
			var q = filter.ToQueryBuilder();
			var qIdeal = QueryBuilder.Create("test={0}", DateTime.Now);

			PAssert.That(() => q == qIdeal);
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
			PAssert.That(() => Filter.CreateCriterium("test2", BooleanComparer.Equal, 37).ExtractInsertWaarden().SequenceEqual(new[] { Tuple.Create("test2", (object)37) }));
			PAssert.That(() => !blaFilter2.ExtractInsertWaarden().Any());
		}


		[Test]
		public void FilterModification()
		{
			var testFilter = Filter.CreateCriterium("test", BooleanComparer.LessThan, 3);
			var test2Filter = Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3);
			var combFilter = Filter.CreateCombined(BooleanOperator.And, testFilter, test2Filter);
			PAssert.That(() => combFilter.ToQueryBuilder() == QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3)); //initial check

			var modFilter =
					combFilter
					.Replace(testFilter, Filter.CreateCriterium("ziggy", BooleanComparer.LessThan, 3))
					.AddTo(test2Filter, BooleanOperator.And, Filter.CreateCriterium("stardust", BooleanComparer.GreaterThan, 37));

			PAssert.That(() => modFilter.ToQueryBuilder() == QueryBuilder.Create("(ziggy<{0} And test2<{1} And stardust>{2})", 3, 3, 37)); //note no nested brackets!
			PAssert.That(() => combFilter.ToQueryBuilder() == QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3)); // side-effect free
			PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, combFilter, null, modFilter).ToQueryBuilder() == QueryBuilder.Create("(test<{0} And test2<{1} And ziggy<{2} And test2<{3} And stardust>{4})", 3, 3, 3, 3, 37)); // no unnecessary brackets!

			PAssert.That(() => combFilter.Remove(test2Filter) == testFilter && combFilter.Remove(testFilter) == test2Filter); // no unnecessary brackets!


			PAssert.That(() =>
				combFilter.AddTo(testFilter, BooleanOperator.Or, Filter.CreateCriterium("abc", BooleanComparer.GreaterThan, 42)).ToQueryBuilder()
				== QueryBuilder.Create("((test<{0} Or abc>{1}) And test2<{2})", 3, 42, 3)); //does include nested brackets!

			var colTypes = new[] { "stardust", "ziggy", "test", "test2" }.ToDictionary(n => n, n => typeof(int));
			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => colTypes.GetOrDefault(s, null)) == modFilter);
			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "stardust" ? null : colTypes.GetOrDefault(s)) == null);
			PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "test" ? null : colTypes.GetOrDefault(s)) == modFilter);//test was replaced, so ok
			//TODO: don't forget to reenable when filter type testing is on again:
			//PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "stardust" ? typeof(string) : colTypes.GetOrDefault(s, null)) == null);//types don't match
		}



		[Test]
		public void ColRef()
		{
			PAssert.That(() => !BooleanComparer.In.CanReferenceColumn());
			PAssert.That(() => !BooleanComparer.NotIn.CanReferenceColumn());
			PAssert.That(() => BooleanComparer.Equal.CanReferenceColumn());
			Assert.Throws<ArgumentNullException>(() => new ColumnReference(null));
			Assert.Throws<ArgumentException>(() => new ColumnReference("a b"));
			Assert.Throws<ArgumentException>(() => new ColumnReference("a.b"));

			PAssert.That(() => Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant")).ClearFilterWhenItContainsInvalidColumns(s => s == "blabla" ? typeof(int) : typeof(string)) == null);
			PAssert.That(() => Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant")).ClearFilterWhenItContainsInvalidColumns(s => s == "relevant" || s == "blabla" ? typeof(int) : null) != null);
			PAssert.That(() => Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant")).ToQueryBuilder() == QueryBuilder.Create("blabla<relevant"));
		}

		[Test]
		public void QueryBuilderSerializesOk()
		{
			var testFilter = Filter.CreateCriterium("test", BooleanComparer.LessThan, 3);
			var test2Filter = Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3);
			var combFilter = Filter.CreateCombined(BooleanOperator.And, testFilter, test2Filter);

			var q = combFilter.ToQueryBuilder();
			var qAlt = QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3);
			var qAltWrong = QueryBuilder.Create("(test<{0} And test2<{1})", 3, 3.0);

			PAssert.That(() => q.CommandText() == qAlt.CommandText());
			PAssert.That(() => q.CommandText().GetHashCode() == qAlt.CommandText().GetHashCode());

			PAssert.That(() => q.Equals(qAlt));
			PAssert.That(() => q.Equals((object)qAlt));
			PAssert.That(() => !q.Equals((object)null));
			PAssert.That(() => !Equals(q, null) && !Equals(null, qAlt) && Equals(q, qAlt));

			PAssert.That(() => q == qAlt);
			PAssert.That(() => q.GetHashCode() == qAlt.GetHashCode());
			PAssert.That(() => q.ToString() == qAlt.ToString());
			PAssert.That(() => q.DebugText() == qAlt.DebugText());

			PAssert.That(() => qAlt.CommandText() == qAltWrong.CommandText());
			PAssert.That(() => qAlt.DebugText() != qAltWrong.DebugText());
			PAssert.That(() => !qAlt.Equals(qAltWrong));
			PAssert.That(() => qAlt != qAltWrong);
			PAssert.That(() => qAlt.GetHashCode() != qAltWrong.GetHashCode());
			PAssert.That(() => qAlt.ToString() != qAltWrong.ToString());
		}

		[Test]
		public void FilterValidation()
		{
			Assert.Throws<InvalidOperationException>(() => Filter.CreateCriterium("test", (BooleanComparer)12345, 3).ToQueryBuilder());
			Assert.Throws<InvalidOperationException>(() => ((BooleanComparer)12345).NiceString());
		}


		[Test]
		public void BasicSerializationWorks()
		{
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).SerializeToString() == @"test[<]i3*");
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).Equals(Filter.TryParseSerializedFilter(@"test[<]i3*")));

			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, new ColumnReference("blablabla")).SerializeToString() == @"test[<]cblablabla*");
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.In, new GroupReference(12345, "blablablaGroup")).SerializeToString() == @"test[in]g12345:blablablaGroup*");
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotIn, new GroupReference(12345, "blablablaGroup")).SerializeToString() == @"test[!in]g12345:blablablaGroup*");


			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, Filter.CurrentTimeToken.Instance).SerializeToString() == @"test[<]n*");

			PAssert.That(() => Filter.TryParseSerializedFilter(@"test[<]i3* ") == null); //extra space!
			PAssert.That(() => Filter.TryParseSerializedFilter(@"test<]i3*") == null); //missing [
			PAssert.That(() => Filter.TryParseSerializedFilter(@"test[<i3*") == null); //missing ]
			PAssert.That(() => Filter.TryParseSerializedFilter(@"test[<<]i3*") == null); //invalid op
			PAssert.That(() => Filter.TryParseSerializedFilter(@"&test[<]i3*") == null); //unterminated &
			PAssert.That(() => Filter.TryParseSerializedFilter(@"|test[<]i3*") == null); //unterminated |
			PAssert.That(() => Filter.TryParseSerializedFilter(@"&test[<]i3*,") == null); //unterminated &
			PAssert.That(() => Filter.TryParseSerializedFilter(@"|test[<]i3*,") == null); //unterminated |
			PAssert.That(() => Filter.TryParseSerializedFilter(@"&test[<]i3*;") != null); //terminated &
			PAssert.That(() => Filter.TryParseSerializedFilter(@"|test[<]i3*;") != null); //terminated |
		}

		[Test]
		public void StarsInValuesSerializeOk()
		{
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, "1*2").SerializeToString() == @"test[=]s1**2*");
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, "1**2").SerializeToString() == @"test[=]s1****2*");
			PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, "1**2*").SerializeToString() == @"test[=]s1****2***");
		}

		[Test]
		public void StarsAndHashesInArraysSerializeOk()
		{
			var filters = new[]{
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{"1*","","#", "##","***",}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new[]{"*"}),
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{"**"}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new[]{"#"}),
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{"##"}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new[]{""}),
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{"#;"}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new[]{";#"}),
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{";#","*","**#*"}),
			};
			foreach (var filter in filters)
				PAssert.That(() => filter.Equals(Filter.TryParseSerializedFilter(filter.SerializeToString())));
		}

		[Test]
		public void CriteriumSerializationRoundTrips()
		{
			var filters = new[]{
								Filter.CreateCriterium("test", BooleanComparer.LessThan, 3),
								Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3),
								Filter.CreateCriterium("test", BooleanComparer.Equal, 3),
								Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3),
								Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3),
								Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3),
								Filter.CreateCriterium("test", BooleanComparer.Contains, "world"),
								Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world"),
								Filter.CreateCriterium("test", BooleanComparer.EndsWith, "world"),
								Filter.CreateCriterium("test", BooleanComparer.IsNull, null),
								Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null),
								Filter.CreateCriterium("test", BooleanComparer.Equal, "1*2"),
								Filter.CreateCriterium("test", BooleanComparer.Equal, null),
								Filter.CreateCriterium("test", BooleanComparer.Equal, new ColumnReference("blablabla")),
								Filter.CreateCriterium("test", BooleanComparer.In, new GroupReference(12345,"blablablaGroup")),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new GroupReference(12345,"blablablaGroup")),
								Filter.CreateCriterium("test", BooleanComparer.Equal, ""),
								Filter.CreateCriterium("test", BooleanComparer.Equal, "*1"),
								Filter.CreateCriterium("test", BooleanComparer.Equal, Filter.CurrentTimeToken.Instance),
								Filter.CreateCriterium("test", BooleanComparer.Equal, "1*"),
								Filter.CreateCriterium("test", BooleanComparer.Equal, "1**2*"),
								Filter.CreateCriterium("test", BooleanComparer.In, new[]{1, 2, 3, 4, 5,}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new[]{1, 2, 3, 4, 5,}),
								Filter.CreateCriterium("test", BooleanComparer.In, new string[]{}),
								Filter.CreateCriterium("test", BooleanComparer.NotIn, new string[]{}),
			};
			foreach (var filter in filters)
				PAssert.That(() => filter.Equals(Filter.TryParseSerializedFilter(filter.SerializeToString())));
		}

		[Test]
		public void ComplexFilterSerialization1()
		{
			var someFilter =
				Filter.CreateCombined(BooleanOperator.And,
									Filter.CreateCriterium("testA", BooleanComparer.LessThan, 3),
									Filter.CreateCriterium("testB", BooleanComparer.LessThanOrEqual, 37),
									Filter.CreateCombined(BooleanOperator.Or,
										Filter.CreateCriterium("testC", BooleanComparer.LessThan, 3),
										Filter.CreateCriterium("testD", BooleanComparer.LessThanOrEqual, 37)
										)
									);
			PAssert.That(() => someFilter is CombinedFilter && ((CombinedFilter)someFilter).FilterLijst.OfType<CombinedFilter>().Any());
			PAssert.That(() => someFilter.Equals(Filter.TryParseSerializedFilter(someFilter.SerializeToString())));
		}

		[Test]
		public void EmptyArraySerializationPreservesType()
		{
			var someFilter = Filter.CreateCriterium("test", BooleanComparer.In, new decimal[] { });
			PAssert.That(() => ((CriteriumFilter)Filter.TryParseSerializedFilter(someFilter.SerializeToString())).Waarde.GetType() == typeof(decimal[]));
			PAssert.That(() => ((CriteriumFilter)Filter.TryParseSerializedFilter(someFilter.SerializeToString())).Waarde.GetType() != typeof(double[]));
		}


		[Test]
		public void ComplexFilterSerialization2()
		{
			var someFilter =
				Filter.CreateCombined(BooleanOperator.And,
									Filter.CreateCombined(BooleanOperator.Or,
										Filter.CreateCriterium("testC", BooleanComparer.LessThan, 3),
										Filter.CreateCriterium("testD", BooleanComparer.LessThanOrEqual, 37)
										),
									Filter.CreateCriterium("testA", BooleanComparer.LessThan, 3),
									Filter.CreateCriterium("testB", BooleanComparer.LessThanOrEqual, 37)
									);
			PAssert.That(() => someFilter is CombinedFilter && ((CombinedFilter)someFilter).FilterLijst.OfType<CombinedFilter>().Any());
			PAssert.That(() => someFilter.Equals(Filter.TryParseSerializedFilter(someFilter.SerializeToString())));
		}
		[Test]
		public void ComplexFilterSerialization3()
		{
			var someFilter =
				Filter.CreateCombined(BooleanOperator.And,
									Filter.CreateCriterium("testA", BooleanComparer.LessThan, 3),
									Filter.CreateCombined(BooleanOperator.Or,
										Filter.CreateCriterium("testC", BooleanComparer.LessThan, 3),
										Filter.CreateCriterium("testD", BooleanComparer.LessThanOrEqual, 37)
										),
									Filter.CreateCriterium("testB", BooleanComparer.LessThanOrEqual, 37)
									);
			PAssert.That(() => someFilter is CombinedFilter && ((CombinedFilter)someFilter).FilterLijst.OfType<CombinedFilter>().Any());
			PAssert.That(() => someFilter.Equals(Filter.TryParseSerializedFilter(someFilter.SerializeToString())));
		}
	}
}
