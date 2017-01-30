using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using MoreLinq;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using System.Data;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class UtilsTest
    {
        [Test]
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
                    throw new Exception("numbers " + i + " and " + j + " produce out-of-order strings: " + a + " and " + b);
                }
            }
        }

        [Test]
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
                );

            foreach (var combo1 in combos) {
                foreach (var combo2 in combos) {
                    var str1 = combo1.Select(Utils.ToSortableShortString).JoinStrings();
                    var str2 = combo2.Select(Utils.ToSortableShortString).JoinStrings();

                    var strComparison = Math.Sign(StringComparer.Ordinal.Compare(str1, str2));
                    var seqComparison = Math.Sign(combo1.Cast<long?>().ZipLongest(combo2.Cast<long?>(), Comparer<long?>.Default.Compare).FirstOrDefault(x => x != 0));
                    if (strComparison != seqComparison) {
                        Assert.Fail(
                            "Comparisons don't match: {0} compared to {1} is {2} but after short string conversion {3}.CompareTo({4}) is {5}",
                            ObjectToCode.ComplexObjectToPseudoCode(combo1),
                            ObjectToCode.ComplexObjectToPseudoCode(combo2),
                            seqComparison,
                            ObjectToCode.ComplexObjectToPseudoCode(str1),
                            ObjectToCode.ComplexObjectToPseudoCode(str2),
                            strComparison
                            );
                    }
                }
            }
        }

        [Test]
        
        public void SwapValue()
        {
            var one = 1;
            var other = 2;
            Utils.Swap(ref one, ref other);
            Assert.That(one, Is.EqualTo(2));
            Assert.That(other, Is.EqualTo(1));
        }

        [Test]
        
        public void SwapReference()
        {
            var one = "1";
            var other = "2";
            Utils.Swap(ref one, ref other);
            Assert.That(one, Is.EqualTo("2"));
            Assert.That(other, Is.EqualTo("1"));
        }

        [Test]
        
        public void NUnitSession()
        {
            Assert.That(Utils.IsInUnitTest());
        }

        static IEnumerable<TestCaseData> MaandSpan()
        {
            yield return new TestCaseData(new DateTime(2000, 1, 1), new DateTime(2000, 1, 1)).Returns(0);
            yield return new TestCaseData(new DateTime(2000, 5, 1), new DateTime(2000, 1, 1)).Returns(4);
            yield return new TestCaseData(new DateTime(2000, 1, 1), new DateTime(2001, 1, 1)).Returns(12);
            yield return new TestCaseData(new DateTime(2001, 1, 1), new DateTime(2000, 1, 1)).Returns(12);
            yield return new TestCaseData(new DateTime(2000, 9, 1), new DateTime(2001, 2, 1)).Returns(5);
            yield return new TestCaseData(new DateTime(2000, 9, 1), new DateTime(2001, 4, 1)).Returns(7);
            yield return new TestCaseData(new DateTime(2001, 6, 1), new DateTime(2000, 9, 1)).Returns(9);
            yield return new TestCaseData(new DateTime(2000, 12, 1), new DateTime(2001, 1, 1)).Returns(1);
        }

        [Test]
        [TestCaseSource(nameof(MaandSpan))]
        
        public int MaandSpanTest(DateTime d1, DateTime d2)
        {
            return Utils.MaandSpan(d1, d2);
        }

        [Test]
        
        public void IsDbConnFailureTest()
        {
            PAssert.That(() => !Utils.IsDbConnectionFailure(new Exception()));
            PAssert.That(() => !Utils.IsDbConnectionFailure(new DataException()));
            PAssert.That(() => !Utils.IsDbConnectionFailure(new ParameterizedSqlExecutionException()));
            PAssert.That(() => Utils.IsDbConnectionFailure(new ParameterizedSqlExecutionException("bla", new DataException("The underlying provider failed on Open."))));
            PAssert.That(
                () =>
                    Utils.IsDbConnectionFailure(
                        new AggregateException(
                            new ParameterizedSqlExecutionException("bla", new DataException("The underlying provider failed on Open.")),
                            new DataException("The underlying provider failed on Open."))));
            PAssert.That(() => !Utils.IsDbConnectionFailure(new AggregateException()));
            PAssert.That(() => !Utils.IsDbConnectionFailure(null));
        }

        [Test]
        
        public void DateMaxTest()
        {
            DateTime? d1 = null;
            DateTime? d2 = null;

            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(null));

            d1 = DateTime.Today;
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

            d1 = null;
            d2 = DateTime.Today;
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));

            d1 = DateTime.Today;
            d2 = DateTime.Today;
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

            d1 = DateTime.Today.AddDays(-1);
            d2 = DateTime.Today;
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));

            d1 = DateTime.Today.AddDays(1);
            d2 = DateTime.Today;
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

            d1 = DateTime.Today;
            d2 = DateTime.Today.AddDays(-1);
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

            d1 = DateTime.Today;
            d2 = DateTime.Today.AddDays(1);
            Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));
        }

        public static IEnumerable<TestCaseData> RoundUpData()
        {
            yield return new TestCaseData(1.12m, 2, 1.12m);
            yield return new TestCaseData(1.0m, 2, 1.0m);
            yield return new TestCaseData(1.121m, 2, 1.13m);
            yield return new TestCaseData(1.129m, 2, 1.13m);
            yield return new TestCaseData(1000001.122m, 2, 1000001.13m);
            yield return new TestCaseData(1000001.129m, 2, 1000001.13m);
        }

        [Test]
        [TestCaseSource(nameof(RoundUpData))]
        public void RoundUp(decimal waarde, int posities, decimal resultaat)
        {
            Assert.That(Utils.RoundUp(waarde, posities), Is.EqualTo(resultaat));
        }

        [Test]
        public void SimpleTransitiveClosureWorks()
        {
            var nodes = new[] { 2, 3, };

            PAssert.That(() => Utils.TransitiveClosure(nodes, num => new[] { num * 2 % 6 }).SetEquals(new[] { 2, 4, 0, 3 }));
        }

        [Test]
        public void MultiTransitiveClosureWorks()
        {
            var nodes = new[] { 2, 3, };

            PAssert.That(() => Utils.TransitiveClosure(nodes, nums => nums.Select(num => num * 2 % 6)).SetEquals(new[] { 2, 4, 0, 3 }));
        }
    }
}
