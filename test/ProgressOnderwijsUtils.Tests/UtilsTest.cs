using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using MoreLinq;
using Xunit;
using System.Data;
using ProgressOnderwijsUtils.Tests.Data;
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
                    throw new Exception(
                        "numbers " + i + " and " + j + " produce out-of-order strings: " + a + " and " +
                        b
                    );
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
                                .FirstOrNullable(x => x != 0) ?? 0L
                        );
                    if (strComparison != seqComparison) {
                        throw new Exception($"Comparisons don't match: {ObjectToCode.ComplexObjectToPseudoCode(combo1)} compared to {ObjectToCode.ComplexObjectToPseudoCode(combo2)} is {seqComparison} but after short string conversion {ObjectToCode.ComplexObjectToPseudoCode(str1)}.CompareTo({ObjectToCode.ComplexObjectToPseudoCode(str2)}) is {strComparison}");
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
                    new ParameterizedSqlExecutionException(
                        "bla",
                        new DataException("The underlying provider failed on Open.")
                    ).IsRetriableConnectionFailure()
            );
            PAssert.That(
                () =>
                    new AggregateException(
                        new ParameterizedSqlExecutionException(
                            "bla",
                            new DataException("The underlying provider failed on Open.")
                        ),
                        new DataException("The underlying provider failed on Open.")
                    ).IsRetriableConnectionFailure()
            );
            PAssert.That(() => !new AggregateException().IsRetriableConnectionFailure());
            PAssert.That(() => !default(Exception).IsRetriableConnectionFailure());
        }

        [Fact]
        public void TimeoutDetectionAbortsWithInconclusiveAfterTimeout()
        {
            var slowQueryWithTimeout = SQL($"WAITFOR DELAY '00:00:02'").OfNonQuery().WithTimeout(CommandTimeout.AbsoluteSeconds(1));
            using var localdb = new TransactedLocalConnection();
            var ex = Assert.ThrowsAny<Exception>(() => slowQueryWithTimeout.Execute(localdb.Connection));
            PAssert.That(() => ex.IsRetriableConnectionFailure());
        }

        [Fact]
        public void ClrDefaultIsSemanticDefault()
        {
            PAssert.That(() => Equals(default(CommandTimeout), CommandTimeout.DeferToConnectionDefault));
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
                        .SetEquals(new[] { 2, 4, 0, 3 })
            );
        }

        [Fact]
        public void TryWithCleanup_CleansUpWhenThereAreNoExceptions()
        {
            var cleanupCalled = 0;
            var value = Utils.TryWithCleanup(() => 42, () => cleanupCalled++);
            PAssert.That(() => cleanupCalled == 1);
            PAssert.That(() => value == 42);
        }

        [Fact]
        public void TryWithCleanup_CleansUpOnceWhenCleanupFails()
        {
            var cleanupCalled = 0;
            var value = 0;
            Action buggyCleanup = () => {
                cleanupCalled++;
                throw new Exception("42");
            };

            var ex = Assert.ThrowsAny<Exception>(() => value = Utils.TryWithCleanup(() => 42, buggyCleanup));

            PAssert.That(() => ex.Message == "42" && ex.TargetSite == buggyCleanup.Method);
            PAssert.That(() => value == 0);
            PAssert.That(() => cleanupCalled == 1);
        }

        [Fact]
        public void TryWithCleanup_CleansUpOnceWhenComputationFails()
        {
            var cleanupCalled = 0;
            var value = 0;
            Action cleanup = () => cleanupCalled++;
            Func<int> buggyComputation = () => {
                try {
                    return 42;
                } finally {
                    throw new Exception("1337");
                }
            };

            var ex = Assert.ThrowsAny<Exception>(() => value = Utils.TryWithCleanup(buggyComputation, cleanup));

            PAssert.That(() => ex.Message == "1337");
            PAssert.That(() => value == 0);
            PAssert.That(() => cleanupCalled == 1);
        }

        [Fact]
        public void TryWithCleanup_CleanUpHappensAfterComputationFinally()
        {
            var finallyReached = false;
            var wasComputationFinallyReachedBeforeCleanup = false;
            try {
                _ = Utils.TryWithCleanup(
                    (Func<int>)(() => {
                        try {
                            throw new Exception("1337");
                        } finally {
                            finallyReached = true;
                        }
                    }),
                    () => wasComputationFinallyReachedBeforeCleanup = finallyReached
                );
            } catch {
                //the pointof this test is to test crash situations!
            }

            PAssert.That(() => finallyReached && wasComputationFinallyReachedBeforeCleanup);
        }

        [Fact]
        public void TryWithCleanup_CleansUpOnceWhenComputationAndCleanupFail()
        {
            var cleanupCalled = 0;
            var value = 0;
            Action buggyCleanup = () => {
                cleanupCalled++;
                throw new Exception("42");
            };
            Func<int> buggyComputation = () => {
                try {
                    return 42;
                } finally {
                    throw new Exception("1337");
                }
            };

            var aggEx = Assert.ThrowsAny<AggregateException>(() => value = Utils.TryWithCleanup(buggyComputation, buggyCleanup));

            PAssert.That(() => aggEx.TargetSite != buggyCleanup.Method && aggEx.TargetSite != buggyComputation.Method);
            var innerExceptions = aggEx.InnerExceptions;
            PAssert.That(() => innerExceptions.Count == 2);
            PAssert.That(() => innerExceptions[0].Message == "1337" && innerExceptions[0].TargetSite == buggyComputation.Method);
            PAssert.That(() => innerExceptions[1].Message == "42" && innerExceptions[1].TargetSite == buggyCleanup.Method);
            PAssert.That(() => value == 0);
            PAssert.That(() => cleanupCalled == 1);
        }

        [Fact]
        public void TryWithCleanup_ActionOverload_CleansUpOnceWhenComputationAndCleanupFail()
        {
            var cleanupCalled = 0;
            Action buggyCleanup = () => {
                cleanupCalled++;
                throw new Exception("42");
            };
            Action buggyComputation = () => throw new Exception("1337");

            var aggEx = Assert.ThrowsAny<AggregateException>(() => Utils.TryWithCleanup(buggyComputation, buggyCleanup));

            PAssert.That(() => aggEx.TargetSite != buggyCleanup.Method && aggEx.TargetSite != buggyComputation.Method);
            var innerExceptions = aggEx.InnerExceptions;
            PAssert.That(() => innerExceptions.Count == 2);
            PAssert.That(() => innerExceptions[0].Message == "1337" && innerExceptions[0].TargetSite == buggyComputation.Method);
            PAssert.That(() => innerExceptions[1].Message == "42" && innerExceptions[1].TargetSite == buggyCleanup.Method);
            PAssert.That(() => cleanupCalled == 1);
        }
    }
}
