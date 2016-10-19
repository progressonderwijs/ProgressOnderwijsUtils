using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.DomainUnits;
using Progress.Business.Filters;
using Progress.Business.Organisaties;
using Progress.Business.Test;
using Progress.Business.Text;
using ProgressOnderwijsUtils;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class FilterTest : TestSuiteBase
    {
        [Test]
        public void BasicChecks()
        {
#pragma warning disable 1720
            PAssert.That(() => default(FilterBase).ToParameterizedSql() == SQL($"1=1")); //shouldn't throw and should be equal
#pragma warning restore 1720

            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).ToParameterizedSql() == SQL($"test < {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3).ToParameterizedSql() == SQL($"test <= {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, 3).ToParameterizedSql() == SQL($"test = {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3).ToParameterizedSql() == SQL($"test >= {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3).ToParameterizedSql() == SQL($"test > {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3).ToParameterizedSql() == SQL($"test != {3}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, Taal.NL).ToParameterizedSql() == SQL($"test = {Taal.NL}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Contains, "world").ToParameterizedSql() == SQL($"test like {"%world%"}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.DoesNotContain, "world").ToParameterizedSql() == SQL($"test not like {"%world%"}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world").ToParameterizedSql() == SQL($"test like {"world%"}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.EndsWith, "world").ToParameterizedSql() == SQL($"test like {"%world"}"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNull, null).ToParameterizedSql() == SQL($"test is null"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, null).ToParameterizedSql() == SQL($"test is null"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.NotEqual, null).ToParameterizedSql() == SQL($"test is not null"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null).ToParameterizedSql() == SQL($"test is not null"));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.HasFlag, 3).ToParameterizedSql() == SQL($"(test & {3}) = {3}"));
        }

        [Test]
        public void InFiltersGenerateCorrectSql()
        {
            //can't compare directly because IEnumerables such as TVP's aren't IEquatable<>
            var exampleArray = new[] { 1, 2, 3, 4, 5 };
            PAssert.That(
                () =>
                    Filter.CreateCriterium("test", BooleanComparer.In, exampleArray).ToParameterizedSql().DebugText()
                        == SQL($"test in {exampleArray}").DebugText());
            PAssert.That(
                () =>
                    Filter.CreateCriterium("test", BooleanComparer.NotIn, exampleArray).ToParameterizedSql().DebugText()
                        == SQL($"test not in {exampleArray}").DebugText());
        }

        [Test]
        public void CurrentTime_IsBetweenNowAnd2millisecondsAgo()
        {
            var filter = Filter.CreateCriterium("test", BooleanComparer.Equal, CurrentTimeToken.Instance);
            var q = filter.ToParameterizedSql();
            var time = DateTime.Now;
            bool matcheSomeOldTime = Enumerable.Range(0, 20000).Select(offset => SQL($"test = {time.AddTicks(-offset)}")).Any(qIdeal => q == qIdeal);
            Assert.True(matcheSomeOldTime, "Kon geen tijd dichtbij DateTime.Now vinden die de CurrentTimeToken matched!");
        }

        [Test]
        public void BadInFilterThrows()
        {
            Assert.Throws<ArgumentException>(() => Filter.CreateCriterium("test", BooleanComparer.In, new[] { default(int?), 1, 2 }));
            Assert.Throws<ArgumentException>(() => Filter.CreateCriterium("test", BooleanComparer.NotIn, Enumerable.Range(1, 10)));
            Filter.CreateCriterium("test", BooleanComparer.NotIn, Enumerable.Range(1, 10).ToArray()); //shouldn't throw.
        }

        [Test]
        public void BooleanComparers()
        {
            var comparers = EnumHelpers.GetValues<BooleanComparer>();
            PAssert.That(() => comparers.Count() == comparers.Select(c => c.SerializationString()).Distinct().Count(), "all nicestrings don't throw and are distinct");
            PAssert.That(
                () => comparers.OrderBy(x => x).SequenceEqual(CriteriumFilter.NumericComparers.Concat(CriteriumFilter.StringComparers).Distinct().OrderBy(x => x)),
                "all comparers either numeric or string or both.");
        }

        [Test]
        public void CreateCombinedWithNulls()
        {
            var blaFilter = Filter.CreateCombined(
                BooleanOperator.And,
                Filter.CreateCriterium("test", BooleanComparer.LessThan, 3),
                Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3));

            PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, null, null) == null);
            PAssert.That(() => Filter.CreateCombined(BooleanOperator.And, blaFilter, null) == blaFilter);
        }

        [Test]
        public void ExtractInsertWaarden()
        {
            var blaFilter = Filter.CreateCombined(
                BooleanOperator.And,
                Filter.CreateCriterium("test", BooleanComparer.Equal, 3),
                Filter.CreateCriterium("test2", BooleanComparer.Equal, 37));
            var blaFilter2 = Filter.CreateCombined(
                BooleanOperator.Or,
                Filter.CreateCriterium("test", BooleanComparer.Equal, 3),
                Filter.CreateCriterium("test2", BooleanComparer.Equal, 37));

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
            PAssert.That(() => combFilter.ToParameterizedSql() == SQL($"( test < {3} and test2 < {3} )")); //initial check

            var modFilter =
                combFilter
                    .Replace(testFilter, Filter.CreateCriterium("ziggy", BooleanComparer.LessThan, 3))
                    .AddTo(test2Filter, BooleanOperator.And, Filter.CreateCriterium("stardust", BooleanComparer.GreaterThan, 37));

            PAssert.That(() => modFilter.ToParameterizedSql() == SQL($"( ziggy < {3} and test2 < {3} and stardust > {37} )")); //note no nested brackets!
            PAssert.That(() => combFilter.ToParameterizedSql() == SQL($"( test < {3} and test2 < {3} )")); // side-effect free
            PAssert.That(
                () =>
                    Filter.CreateCombined(BooleanOperator.And, combFilter, null, modFilter).ToParameterizedSql()
                        == SQL($"( test < {3} and test2 < {3} and ziggy < {3} and test2 < {3} and stardust > {37} )")); // no unnecessary brackets!

            PAssert.That(() => combFilter.Remove(test2Filter) == testFilter && combFilter.Remove(testFilter) == test2Filter); // no unnecessary brackets!

            PAssert.That(
                () =>
                    combFilter.AddTo(testFilter, BooleanOperator.Or, Filter.CreateCriterium("abc", BooleanComparer.GreaterThan, 42)).ToParameterizedSql()
                        == SQL($"( ( test < {3} or abc > {42} ) and test2 < {3} )")); //does include nested brackets!

            var colTypes = new[] { "stardust", "ziggy", "test", "test2" }.ToDictionary(n => n, n => typeof(int));
            PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => colTypes.GetOrDefault(s, null)) == modFilter);
            PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "stardust" ? null : colTypes.GetOrDefault(s)) == null);
            PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "test" ? null : colTypes.GetOrDefault(s)) == modFilter); //test was replaced, so ok

            PAssert.That(() => modFilter.ClearFilterWhenItContainsInvalidColumns(s => s == "stardust" ? typeof(string) : colTypes.GetOrDefault(s, null)) == null);
            //types don't match
        }

        [Test]
        public void ColRef()
        {
            PAssert.That(() => !Filter.ComparersThatCanReferenceColumns.Contains(BooleanComparer.In));
            PAssert.That(() => !Filter.ComparersThatCanReferenceColumns.Contains(BooleanComparer.NotIn));
            PAssert.That(() => Filter.ComparersThatCanReferenceColumns.Contains(BooleanComparer.Equal));
            Assert.Throws<ArgumentNullException>(() => new ColumnReference(null));
            Assert.Throws<ArgumentException>(() => new ColumnReference("a b"));
            Assert.Throws<ArgumentException>(() => new ColumnReference("a.b"));

            PAssert.That(
                () =>
                    Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant"))
                        .ClearFilterWhenItContainsInvalidColumns(s => s == "blabla" ? typeof(int) : typeof(string)) == null);
            PAssert.That(
                () =>
                    Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant"))
                        .ClearFilterWhenItContainsInvalidColumns(s => s == "relevant" || s == "blabla" ? typeof(int) : null) != null);
            PAssert.That(
                () => Filter.CreateCriterium("blabla", BooleanComparer.LessThan, new ColumnReference("relevant")).ToParameterizedSql() == SQL($"blabla < relevant"));
        }

        [Test]
        public void ParameterizedSqlSerializesOk()
        {
            var testFilter = Filter.CreateCriterium("test", BooleanComparer.LessThan, 3);
            var test2Filter = Filter.CreateCriterium("test2", BooleanComparer.LessThan, 3);
            var combFilter = Filter.CreateCombined(BooleanOperator.And, testFilter, test2Filter);

            var q = combFilter.ToParameterizedSql();
            var qAlt = SQL($"( test < {3} and test2 < {3} )");
            var qAltWrong = SQL($"( test < {3} and test2 < {3.0} )");

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

            PAssert.That(() => qAlt.CommandText() != qAltWrong.CommandText());
            PAssert.That(() => qAlt.DebugText() == qAltWrong.DebugText());
            PAssert.That(() => !qAlt.Equals(qAltWrong));
            PAssert.That(() => qAlt != qAltWrong);
            PAssert.That(() => qAlt.GetHashCode() != qAltWrong.GetHashCode());
            PAssert.That(() => qAlt.ToString() == qAltWrong.ToString());
        }

        [Test]
        public void FilterValidation()
        {
            Assert.Throws<InvalidOperationException>(() => Filter.CreateCriterium("test", (BooleanComparer)12345, 3).ToParameterizedSql());
            Assert.Throws<InvalidOperationException>(() => ((BooleanComparer)12345).SerializationString());
        }

        [Test]
        public void BasicSerializationWorks()
        {
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).SerializeToString() == @"test[<]i3*");
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, 3).Equals(Filter.TryParseSerializedFilter(@"test[<]i3*")));

            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, new ColumnReference("blablabla")).SerializeToString() == @"test[<]cblablabla*");
            PAssert.That(
                () => Filter.CreateCriterium("test", BooleanComparer.In, new GroupReference(12345, "blablablaGroup")).SerializeToString() == @"test[in]g12345:blablablaGroup*");
            PAssert.That(
                () =>
                    Filter.CreateCriterium("test", BooleanComparer.NotIn, new GroupReference(12345, "blablablaGroup")).SerializeToString()
                        == @"test[!in]g12345:blablablaGroup*");

            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.LessThan, CurrentTimeToken.Instance).SerializeToString() == @"test[<]n*");

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
        public void EnumsSerializeOk()
        {
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, Taal.NL).SerializeToString() == @"test[=]i1*");
        }

        [Test]
        public void BooleansSerializeOk()
        {
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, true).SerializeToString() == "test[=]bTrue*");
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, false).SerializeToString() == "test[=]bFalse*");
        }

        [Test]
        public void BooleansDeserializeOk()
        {
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, true).Equals(Filter.TryParseSerializedFilter("test[=]bTrue*")));
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.Equal, false).Equals(Filter.TryParseSerializedFilter("test[=]bFalse*")));
        }

        enum TestEnum
        {
            A = 1,
            B = 2,
        }

        [Test]
        public void EnumArraysSerializeAsIntegerArrays()
        {
            PAssert.That(() => Filter.CreateCriterium("test", BooleanComparer.In, new[] { TestEnum.A, TestEnum.B, }).SerializeToString() == "test[in]#i1#;2#;*");
        }

        [Test]
        public void StarsAndHashesInArraysSerializeOk()
        {
            var filters = new[] {
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { "1*", "", "#", "##", "***", }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { "*" }),
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { "**" }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { "#" }),
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { "##" }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { "" }),
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { "#;" }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { ";#" }),
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { ";#", "*", "**#*" }),
            };
            foreach (var filter in filters) {
                PAssert.That(() => filter.Equals(Filter.TryParseSerializedFilter(filter.SerializeToString())));
            }
        }

        [Test]
        public void CriteriumSerializationRoundTrips()
        {
            var filters = new[] {
                Filter.CreateCriterium("test", BooleanComparer.LessThan, 3),
                Filter.CreateCriterium("test", BooleanComparer.LessThanOrEqual, 3),
                Filter.CreateCriterium("test", BooleanComparer.Equal, 3),
                Filter.CreateCriterium("test", BooleanComparer.GreaterThanOrEqual, 3),
                Filter.CreateCriterium("test", BooleanComparer.GreaterThan, 3),
                Filter.CreateCriterium("test", BooleanComparer.NotEqual, 3),
                Filter.CreateCriterium("test", BooleanComparer.Contains, "world"),
                Filter.CreateCriterium("test", BooleanComparer.DoesNotContain, "world"),
                Filter.CreateCriterium("test", BooleanComparer.StartsWith, "world"),
                Filter.CreateCriterium("test", BooleanComparer.EndsWith, "world"),
                Filter.CreateCriterium("test", BooleanComparer.IsNull, null),
                Filter.CreateCriterium("test", BooleanComparer.IsNotNull, null),
                Filter.CreateCriterium("test", BooleanComparer.Equal, "1*2"),
                Filter.CreateCriterium("test", BooleanComparer.Equal, null),
                Filter.CreateCriterium("test", BooleanComparer.Equal, new ColumnReference("blablabla")),
                Filter.CreateCriterium("test", BooleanComparer.In, new GroupReference(12345, "blablablaGroup")),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new GroupReference(12345, "blablablaGroup")),
                Filter.CreateCriterium("test", BooleanComparer.Equal, ""),
                Filter.CreateCriterium("test", BooleanComparer.Equal, "*1"),
                Filter.CreateCriterium("test", BooleanComparer.Equal, CurrentTimeToken.Instance),
                Filter.CreateCriterium("test", BooleanComparer.Equal, "1*"),
                Filter.CreateCriterium("test", BooleanComparer.Equal, "1**2*"),
                Filter.CreateCriterium("test", BooleanComparer.In, new[] { 1, 2, 3, 4, 5, }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new[] { 1, 2, 3, 4, 5, }),
                Filter.CreateCriterium("test", BooleanComparer.In, new string[] { }),
                Filter.CreateCriterium("test", BooleanComparer.NotIn, new string[] { }),
                Filter.CreateCriterium("test", BooleanComparer.HasFlag, 3),
            };

            foreach (var filter in filters) {
                PAssert.That(() => filter.Equals(Filter.TryParseSerializedFilter(filter.SerializeToString())));
            }
        }

        [Test]
        public void ComplexFilterSerialization1()
        {
            var someFilter =
                Filter.CreateCombined(
                    BooleanOperator.And,
                    Filter.CreateCriterium("testA", BooleanComparer.LessThan, 3),
                    Filter.CreateCriterium("testB", BooleanComparer.LessThanOrEqual, 37),
                    Filter.CreateCombined(
                        BooleanOperator.Or,
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
                Filter.CreateCombined(
                    BooleanOperator.And,
                    Filter.CreateCombined(
                        BooleanOperator.Or,
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
                Filter.CreateCombined(
                    BooleanOperator.And,
                    Filter.CreateCriterium("testA", BooleanComparer.LessThan, 3),
                    Filter.CreateCombined(
                        BooleanOperator.Or,
                        Filter.CreateCriterium("testC", BooleanComparer.LessThan, 3),
                        Filter.CreateCriterium("testD", BooleanComparer.LessThanOrEqual, 37)
                        ),
                    Filter.CreateCriterium("testB", BooleanComparer.LessThanOrEqual, 37)
                    );
            PAssert.That(() => someFilter is CombinedFilter && ((CombinedFilter)someFilter).FilterLijst.OfType<CombinedFilter>().Any());
            PAssert.That(() => someFilter.Equals(Filter.TryParseSerializedFilter(someFilter.SerializeToString())));
        }

        static readonly BlaFilterObject[] data = new[] {
            new BlaFilterObject(1, 2, "3", BlaFilterEnumTest.Abc, BlaFilterEnumTest.Test),
            new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Xyz, null),
            new BlaFilterObject(100, 0, null, BlaFilterEnumTest.Xyz, null),
            new BlaFilterObject(null, 100, "100", BlaFilterEnumTest.Test, null),
            new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Xyz, BlaFilterEnumTest.Test),
            new BlaFilterObject(100, 100, null, BlaFilterEnumTest.Abc, BlaFilterEnumTest.Abc),
            new BlaFilterObject(null, 100, "100", BlaFilterEnumTest.Xyz, null),
            new BlaFilterObject(null, 1, null, BlaFilterEnumTest.Abc, null),
            new BlaFilterObject(1, 0, null, BlaFilterEnumTest.Abc, null),
            new BlaFilterObject(1, 0, null, BlaFilterEnumTest.Xyz, BlaFilterEnumTest.Abc),
            new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Xyz, BlaFilterEnumTest.Xyz),
        };

        static readonly IFilterFactory<BlaFilterObject> helper = null;
        Func<Id.StudentenGroep, Func<int, bool>> getStudentenGroupContainmentVerifier;
        IEnumerable<BlaFilterObject> run(FilterBase filter) => data.Where(filter.ToMetaObjectFilter<BlaFilterObject>(getStudentenGroupContainmentVerifier));

        [SetUp]
        public void initGroupLookup()
        {
            getStudentenGroupContainmentVerifier = InfoStudentenGroep.CachedGroupMembershipVerifier(conn);
        }

        [Test]
        public void MetaObjectFiltersWork()
        {
            var filter = helper.FilterOn(o => o.EnumNullable).Equal(BlaFilterEnumTest.Test);
            PAssert.That(() => filter.ToParameterizedSql() == SQL($"EnumNullable = {BlaFilterEnumTest.Test}"));

            PAssert.That(() => run(filter).Count() == 2);
        }

        [Test]
        public void MetaObject_StringEq()
        {
            var filterNull = helper.FilterOn(o => o.StringVal).Equal(null);
            var filter3 = helper.FilterOn(o => o.StringVal).Equal("3");
            var filter100 = helper.FilterOn(o => o.StringVal).Equal("100");
            PAssert.That(() => run(filterNull).Count() == 8);
            PAssert.That(() => run(filter3).Count() == 1);
            PAssert.That(() => run(filter100).Count() == 2);
            PAssert.That(() => run(filterNull).Count() == 8);
            PAssert.That(() => run(filter3).Count() == 1);
            PAssert.That(() => run(Filter.TryParseSerializedFilter(filter100.SerializeToString())).Count() == 2);
        }

        [Test]
        public void MetaObject_StringNeq()
        {
            var filterNull = helper.FilterOn(o => o.StringVal).NotEqual(null);
            var filter3 = helper.FilterOn(o => o.StringVal).NotEqual("3");
            var filter100 = helper.FilterOn(o => o.StringVal).NotEqual("100");
            PAssert.That(() => run(filterNull).Count() == 3);
            PAssert.That(() => run(filter3).Count() == 10);
            PAssert.That(() => run(filter100).Count() == 9);
            PAssert.That(() => run(filterNull).Count() == 3);
            PAssert.That(() => run(filter3).Count() == 10);
            PAssert.That(() => run(Filter.TryParseSerializedFilter(filter100.SerializeToString())).Count() == 9);
        }

        [Test]
        public void MetaObject_StringIn()
        {
            var filterIn = Filter.CreateCriterium("StringVal", BooleanComparer.In, new[] { "100", "4" });
            var filterNotIn = Filter.CreateCriterium("StringVal", BooleanComparer.NotIn, new[] { "1", "3" }); //TODO:verify SQL results too; in particular with DBNULL
            PAssert.That(() => run(filterIn).Count() == 2);
            PAssert.That(() => run(filterNotIn).Count() == 10);
            PAssert.That(() => run(Filter.TryParseSerializedFilter(filterNotIn.SerializeToString())).Count() == 10);
        }

        [Test]
        public void MetaObject_IntBasics()
        {
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).LessThan(100)).Count() == 3);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).LessThanOrEqual(100)).Count() == 5);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).GreaterThanOrEqual(100)).Count() == 2);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).GreaterThan(100)).Count() == 0);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).Equal(100)).Count() == 2);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNullable).NotEqual(100)).Count() == 9);
        }

        [Test]
        public void MetaObject_IntNonNullableBasics()
        {
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).LessThan(100)).Count() == 8);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).LessThanOrEqual(100)).Count() == 11);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).GreaterThanOrEqual(100)).Count() == 3);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).GreaterThan(100)).Count() == 0);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).Equal(100)).Count() == 3);
            PAssert.That(() => run(helper.FilterOn(o => o.IntNonNullable).NotEqual(100)).Count() == 8);
        }

        [Test]
        public void MetaObject_IntColRef()
        {
            var filterA = Filter.CreateCriterium("IntNullable", BooleanComparer.Equal, new ColumnReference("IntNonNullable"));
            var filterB = Filter.CreateCriterium("intNonNullable", BooleanComparer.Equal, new ColumnReference("intnullable"));

            PAssert.That(() => filterA.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() != null);
            PAssert.That(() => filterB.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() != null);

            PAssert.That(() => run(filterA).Count() == 1);
            PAssert.That(() => run(filterB).Count() == 1);
        }

        [Test]
        public void MetaObject_StrCompNullSafe()
        {
            var filterA = helper.FilterOn(o => o.StringVal).Contains("0");
            var filterB = helper.FilterOn(o => o.StringVal).StartsWith("10");
            var filterC = helper.FilterOn(o => o.StringVal).EndsWith("3");
            var filterA_null = helper.FilterOn(o => o.StringVal).Contains(null);
            var filterB_null = helper.FilterOn(o => o.StringVal).StartsWith(null);
            var filterC_null = helper.FilterOn(o => o.StringVal).EndsWith(null);

            PAssert.That(() => run(filterA).Count() == 2);
            PAssert.That(() => run(filterB).Count() == 2);
            PAssert.That(() => run(filterC).Count() == 1);
            PAssert.That(() => run(filterA_null).Count() == 0);
            PAssert.That(() => run(filterB_null).Count() == 0);
            PAssert.That(() => run(filterC_null).Count() == 0);
        }

        [Test]
        public void MetaObject_EnumGreaterThanComparison()
        {
            var filter = new FilterFactory<BlaFilterObject>().FilterOn(o => o.EnumVal).GreaterThan(BlaFilterEnumTest.Abc);
            var func = filter.ToMetaObjectFilter<BlaFilterObject>(getStudentenGroupContainmentVerifier);
            PAssert.That(() => func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, null)));
        }

        [Test]
        public void MetaObject_NullableEnumGreaterThanComparison()
        {
            var filter = new FilterFactory<BlaFilterObject>().FilterOn(o => o.EnumNullable).GreaterThan(BlaFilterEnumTest.Abc);
            var func = filter.ToMetaObjectFilter<BlaFilterObject>(getStudentenGroupContainmentVerifier);
            PAssert.That(() => func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, BlaFilterEnumTest.Test)));
            PAssert.That(() => !func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, null)));
        }

        [Test]
        public void MetaObject_EnumGreaterThanNullableKolom()
        {
            var filter = Filter.CreateCriterium("EnumVal", BooleanComparer.GreaterThan, new ColumnReference("EnumNullable"));
            var func = filter.ToMetaObjectFilter<BlaFilterObject>(getStudentenGroupContainmentVerifier);
            PAssert.That(() => func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Abc, BlaFilterEnumTest.Xyz)));
            PAssert.That(() => !func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, null)));
        }

        [Test]
        public void MetaObject_NullableEnumGreaterThanNonNullableKolom()
        {
            var filter = Filter.CreateCriterium("EnumNullable", BooleanComparer.GreaterThan, new ColumnReference("EnumVal"));
            var func = filter.ToMetaObjectFilter<BlaFilterObject>(getStudentenGroupContainmentVerifier);
            PAssert.That(() => !func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, BlaFilterEnumTest.Abc)));
            PAssert.That(() => !func(new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Test, null)));
        }

        [Test]
        public void MetaObject_MixedColRef()
        {
            var filterA = Filter.CreateCriterium("IntNullable", BooleanComparer.Equal, new ColumnReference("enumVal"));
            var filterB = Filter.CreateCriterium("intNonNullable", BooleanComparer.Equal, new ColumnReference("enumNullable"));

            PAssert.That(() => filterA.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() != null);
            PAssert.That(() => filterB.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() != null);

            PAssert.That(() => run(filterA).Count() == 2);
            PAssert.That(() => run(filterB).Count() == 2);
        }

        [Test]
        public void MetaObject_ClearsNonsense()
        {
            var filterNonSenseA = Filter.CreateCriterium("intNonNullable", BooleanComparer.Equal, new ColumnReference("StringVal"));
            var filterNonSenseB = Filter.CreateCriterium("StringVal", BooleanComparer.Equal, new ColumnReference("intNullable"));
            var filterNonSenseC = Filter.CreateCriterium("intNonNullable", BooleanComparer.Equal, null);
            var filterNonSenseD = Filter.CreateCriterium("StringVal", BooleanComparer.Equal, 1);
            var filterNonSenseE = Filter.CreateCriterium("intNonNullable", BooleanComparer.Equal, 1.0);
            var filterNonSenseF = Filter.CreateCriterium("enumVal", BooleanComparer.Equal, 1.0);
            var filterNonSenseG = Filter.CreateCriterium("enumVal", BooleanComparer.Equal, null);

            PAssert.That(() => filterNonSenseA.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseB.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseC.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseD.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseE.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseF.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
            PAssert.That(() => filterNonSenseG.ClearFilterWhenItContainsInvalidColumns<BlaFilterObject>() == null);
        }

        [Test]
        public void FilterCreator()
        {
            var filter = new FilterFactory<BlaFilterObject>.FilterCreator<int?>(o => o.IntNullable).IsNull();
            PAssert.That(() => filter.ToParameterizedSql() == SQL($"IntNullable is null"));

            var filter2 = new FilterFactory<BlaFilterObject>.FilterCreator<int?>(o => o.IntNullable).Equal(3);
            PAssert.That(() => filter2.ToParameterizedSql() == SQL($"IntNullable = {3}"));
        }

        static readonly BlaFilterObject blaFilterObject = new BlaFilterObject(null, 0, null, BlaFilterEnumTest.Xyz, null);

        static Func<BlaFilterObject, bool> CompileHasFlagFilterForMetaObject(string kolomNaam, object waarde)
        {
            var filter = Filter.CreateCriterium(kolomNaam, BooleanComparer.HasFlag, waarde);
            return Filter.ToMetaObjectFilterExpr<BlaFilterObject>(filter, y => x => false).Compile();
        }

        [Test]
        public void Compiled_meta_object_filter_doesnt_crash_when_has_flag_operand_is_Int32_0()
        {
            var filter = CompileHasFlagFilterForMetaObject(nameof(BlaFilterObject.IntNonNullable), 1);
            PAssert.That(() => !filter(blaFilterObject));
        }

        [Test]
        public void Compiled_meta_object_filter_doesnt_crash_when_has_flag_operand_is_nullable_Int32_null()
        {
            var filter = CompileHasFlagFilterForMetaObject(nameof(BlaFilterObject.IntNullable), 1);
            PAssert.That(() => !filter(blaFilterObject));
        }

        [Test]
        public void Compiled_meta_object_filter_doesnt_crash_when_has_flag_operand_is_enum_0()
        {
            var filter = CompileHasFlagFilterForMetaObject(nameof(BlaFilterObject.EnumVal), BlaFilterEnumTest.Abc);
            PAssert.That(() => !filter(blaFilterObject));
        }

        [Test]
        public void Compiled_meta_object_filter_crash_when_has_flag_operand_is_nullable_enum_null()
        {
            var filter = CompileHasFlagFilterForMetaObject(nameof(BlaFilterObject.EnumNullable), BlaFilterEnumTest.Abc);
            PAssert.That(() => !filter(blaFilterObject));
        }

        [Test]
        public void HasFlagHelper_null_null_returns_false()
        {
            PAssert.That(() => !CriteriumFilter.HasFlagHelper(null, null));
        }

        [Test]
        public void HasFlagHelper_null_0_returns_false()
        {
            PAssert.That(() => !CriteriumFilter.HasFlagHelper(null, 0));
        }

        [Test]
        public void HasFlagHelper_0_null_returns_false()
        {
            PAssert.That(() => !CriteriumFilter.HasFlagHelper(0, null));
        }

        [Test]
        public void HasFlagHelper_0_1_returns_false()
        {
            PAssert.That(() => !CriteriumFilter.HasFlagHelper(0, 1));
        }

        [Test]
        public void HasFlagHelper_1_0_returns_true()
        {
            PAssert.That(() => CriteriumFilter.HasFlagHelper(1, 0));
        }
    }

    public sealed class BlaFilterObject : ValueBase<BlaFilterObject>, IMetaObject
    {
        public BlaFilterObject(
            int? intNullable,
            int intNonNullable,
            string stringVal,
            BlaFilterEnumTest enumVal,
            BlaFilterEnumTest? enumNullable
            )
        {
            IntNullable = intNullable;
            IntNonNullable = intNonNullable;
            StringVal = stringVal;
            EnumVal = enumVal;
            EnumNullable = enumNullable;
        }

        public int? IntNullable { get; set; }
        public int IntNonNullable { get; set; }
        public string StringVal { get; set; }
        public BlaFilterEnumTest EnumVal { get; set; }
        public BlaFilterEnumTest? EnumNullable { get; set; }
    }

    public enum BlaFilterEnumTest
    {
        Xyz,

        [MpLabel("*")]
        Abc,

        [MpLabel(null)]
        Test,
    }
}
