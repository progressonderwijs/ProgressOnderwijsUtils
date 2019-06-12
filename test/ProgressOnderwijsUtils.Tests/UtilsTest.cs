using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using MoreLinq;
using Xunit;
using System.Data;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class UtilsTest
    {
        [Fact]
        public void ToSortableString_MaintainsOrder()
        {
            var cmp = StringComparer.Ordinal;
            var samplePoints = MoreEnumerable
                .Generate((double)long.MinValue, sample => sample + (1.0 + Math.Abs(sample) / 1000.0))
                .TakeWhile(sample => sample < long.MaxValue)
                .Select(d => (long)d)
                .Concat(new[] { long.MinValue, long.MaxValue - 1, -1, 0, 1 });

            foreach (var i in samplePoints) {
                var j = i + 1;
                var a = Utils.ToSortableShortString(i);
                var b = Utils.ToSortableShortString(j);
                if (cmp.Compare(a, b) >= 0) {
                    throw new Exception("numbers " + i + " and " + j + " produce out-of-order strings: " + a + " and " +
                        b);
                }
            }
        }

        [Fact]
        public void ToSortableString_ConcatenatedStringsMaintainOrder()
        {
            var samplePoints = new long[] {
                -10000,
                -1000,
                -100,
                -50,
                -25,
                -12,
                -6,
                0,
                6,
                12,
                25,
                50,
                100,
                1000,
                10000,
            };

            var combos = (
                from a in samplePoints
                from b in samplePoints
                select new[] { a, b }
            ).Concat(
                from a in samplePoints
                select new[] { a }
            ).ToArray();

            foreach (var combo1 in combos) {
                foreach (var combo2 in combos) {
                    var str1 = combo1.Select(Utils.ToSortableShortString).JoinStrings();
                    var str2 = combo2.Select(Utils.ToSortableShortString).JoinStrings();

                    var strComparison = Math.Sign(StringComparer.Ordinal.Compare(str1, str2));
                    var seqComparison =
                        Math.Sign(
                            combo1.Cast<long?>()
                                .ZipLongest(combo2.Cast<long?>(), Comparer<long?>.Default.Compare)
                                .FirstOrNullable(x => x != 0) ?? 0L);
                    if (strComparison != seqComparison) {
                        throw new Exception(
                            $"Comparisons don't match: {ObjectToCode.ComplexObjectToPseudoCode(combo1)} compared to {ObjectToCode.ComplexObjectToPseudoCode(combo2)} is {seqComparison} but after short string conversion {ObjectToCode.ComplexObjectToPseudoCode(str1)}.CompareTo({ObjectToCode.ComplexObjectToPseudoCode(str2)}) is {strComparison}");
                    }
                }
            }
        }

        [Fact]
        public void SwapValue()
        {
            var one = 1;
            var other = 2;
            Utils.Swap(ref one, ref other);
            PAssert.That(() => one == 2);
            PAssert.That(() => other == 1);
        }

        [Fact]
        public void SwapReference()
        {
            var one = "1";
            var other = "2";
            Utils.Swap(ref one, ref other);
            PAssert.That(() => one == "2");
            PAssert.That(() => other == "1");
        }

        [Fact]
        public void MaandSpanTest()
        {
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 1, 1), new DateTime(2000, 1, 1)) == 0);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 5, 1), new DateTime(2000, 1, 1)) == 4);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 1, 1), new DateTime(2001, 1, 1)) == 12);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2001, 1, 1), new DateTime(2000, 1, 1)) == 12);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 9, 1), new DateTime(2001, 2, 1)) == 5);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 9, 1), new DateTime(2001, 4, 1)) == 7);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2001, 6, 1), new DateTime(2000, 9, 1)) == 9);
            PAssert.That(() => Utils.MaandSpan(new DateTime(2000, 12, 1), new DateTime(2001, 1, 1)) == 1);
        }

        [Fact]
        public void IsDbConnFailureTest()
        {
            PAssert.That(() => !new Exception().IsRetriableConnectionFailure());
            PAssert.That(() => !new DataException().IsRetriableConnectionFailure());
            PAssert.That(() => !new ParameterizedSqlExecutionException().IsRetriableConnectionFailure());
            PAssert.That(
                () =>
                    new ParameterizedSqlExecutionException("bla",
                        new DataException("The underlying provider failed on Open.")).IsRetriableConnectionFailure());
            PAssert.That(
                () =>
                    new AggregateException(
                        new ParameterizedSqlExecutionException("bla",
                            new DataException("The underlying provider failed on Open.")),
                        new DataException("The underlying provider failed on Open.")).IsRetriableConnectionFailure());
            PAssert.That(() => !new AggregateException().IsRetriableConnectionFailure());
            PAssert.That(() => !default(Exception).IsRetriableConnectionFailure());
        }

        [Fact]
        public void TimeoutDetectionAbortsWithInconclusiveAfterTimeout()
        {
            using (var localdb = new TransactedLocalConnection()) {
                var ex = Assert.ThrowsAny<Exception>(() => { SQL($"WAITFOR DELAY '00:00:02'").OfNonQuery(BatchTimeout.AbsoluteSeconds(1)).Execute(localdb.Connection); });
                PAssert.That(() => ex.IsRetriableConnectionFailure());
            }
        }

        [Fact]
        public void ClrDefaultIsSemanticDefault()
        {
            PAssert.That(() => Equals(default(BatchTimeout), BatchTimeout.DeferToConnectionDefault));
        }

        [Fact]
        public void DateMaxTest()
        {
            DateTime? d1 = null;
            DateTime? d2 = null;

            PAssert.That(() => Utils.DateMax(d1, d2) == null);

            d1 = DateTime.Today;
            PAssert.That(() => Utils.DateMax(d1, d2) == d1);

            d1 = null;
            d2 = DateTime.Today;
            PAssert.That(() => Utils.DateMax(d1, d2) == d2);

            d1 = DateTime.Today;
            d2 = DateTime.Today;
            PAssert.That(() => Utils.DateMax(d1, d2) == d1);

            d1 = DateTime.Today.AddDays(-1);
            d2 = DateTime.Today;
            PAssert.That(() => Utils.DateMax(d1, d2) == d2);

            d1 = DateTime.Today.AddDays(1);
            d2 = DateTime.Today;
            PAssert.That(() => Utils.DateMax(d1, d2) == d1);

            d1 = DateTime.Today;
            d2 = DateTime.Today.AddDays(-1);
            PAssert.That(() => Utils.DateMax(d1, d2) == d1);

            d1 = DateTime.Today;
            d2 = DateTime.Today.AddDays(1);
            PAssert.That(() => Utils.DateMax(d1, d2) == d2);
        }

        [Fact]
        public void RoundUp()
        {
            PAssert.That(() => Utils.RoundUp(1.12m, 2) == 1.12m);
            PAssert.That(() => Utils.RoundUp(1.0m, 2) == 1.0m);
            PAssert.That(() => Utils.RoundUp(1.121m, 2) == 1.13m);
            PAssert.That(() => Utils.RoundUp(1.129m, 2) == 1.13m);
            PAssert.That(() => Utils.RoundUp(1000001.122m, 2) == 1000001.13m);
            PAssert.That(() => Utils.RoundUp(1000001.129m, 2) == 1000001.13m);
        }

        [Fact]
        public void SimpleTransitiveClosureWorks()
        {
            var nodes = new[] { 2, 3, };

            PAssert.That(() => Utils.TransitiveClosure(nodes, num => new[] { num * 2 % 6 }).SetEquals(new[] { 2, 4, 0, 3 }));
        }

        [Fact]
        public void MultiTransitiveClosureWorks()
        {
            var nodes = new[] { 2, 3, };

            PAssert.That(
                () =>
                    Utils.TransitiveClosure(nodes, nums => nums.Select(num => num * 2 % 6))
                        .SetEquals(new[] { 2, 4, 0, 3 }));
        }
    }
}
