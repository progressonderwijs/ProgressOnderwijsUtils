using ProgressOnderwijsUtils.Tests.Data;

namespace ProgressOnderwijsUtils.Tests;

public sealed class UtilsTest
{
    [Fact]
    public void ToSortableString_MaintainsOrder()
    {
        static IEnumerable<long> Nums()
        {
            for (var sample = (double)long.MinValue; sample < long.MaxValue; sample += 1.0 + Math.Abs(sample) / 1000.0) {
                yield return (long)sample;
            }
            yield return long.MaxValue - 1;
        }

        foreach (var i in Nums()) {
            var j = i + 1;
            var a = Utils.ToSortableShortString(i);
            var b = Utils.ToSortableShortString(j);
            if (StringComparer.Ordinal.Compare(a, b) >= 0) {
                throw new($"numbers {i} and {j} produce out-of-order strings: {a} and {b}");
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
            from b in samplePoints.Cast<long?>().Append(default(long?))
            select new[] { a, b, }
        ).ToArray();

        foreach (var combo1 in combos) {
            foreach (var combo2 in combos) {
                var str1 = combo1.WhereNotNull().Select(Utils.ToSortableShortString).JoinStrings();
                var str2 = combo2.WhereNotNull().Select(Utils.ToSortableShortString).JoinStrings();

                var strComparison = Math.Sign(StringComparer.Ordinal.Compare(str1, str2));
                var seqComparison = ArrayOrderingComparer<long?>.Default.Compare(combo1, combo2);

                if (strComparison != seqComparison) {
                    throw new($"Comparisons don't match: {ObjectToCode.ComplexObjectToPseudoCode(combo1)} compared to {ObjectToCode.ComplexObjectToPseudoCode(combo2)} is {seqComparison} but after short string conversion {ObjectToCode.ComplexObjectToPseudoCode(str1)}.CompareTo({ObjectToCode.ComplexObjectToPseudoCode(str2)}) is {strComparison}");
                }
            }
        }
    }

    [Fact]
    public void LazyRetriesExceptions()
    {
        var count = 1;
        var nonFailingLazy = Utils.Lazy(() => ++count);
        var sometimesFailingLazy = Utils.Lazy(() => ++count % 4 == 0 ? count : throw new("gotcha!"));

        PAssert.That(() => count == 1);
        PAssert.That(() => nonFailingLazy() == 2);
        PAssert.That(() => count == 2);
        PAssert.That(() => nonFailingLazy() == 2, "A second read of the lazily computed value should not change the value");
        PAssert.That(() => count == 2, "A second read of the lazily computed value must not have side-effects");

        _ = Assert.Throws<Exception>(() => _ = sometimesFailingLazy());
        PAssert.That(() => count == 3);
        PAssert.That(() => sometimesFailingLazy() == 4);
    }

    [Fact]
    public void LazyRequiringContextDoesntRecomputeOnContextChange()
    {
        var count = 1;
        var nonFailingLazy = Utils.LazyRequiringContext((int baseline) => baseline + ++count);
        var sometimesFailingLazy = Utils.LazyRequiringContext((int baseline) => ++count % 4 == 0 ? baseline + count : throw new("gotcha!"));

        PAssert.That(() => count == 1);
        PAssert.That(() => nonFailingLazy(2) == 4);
        PAssert.That(() => count == 2);
        PAssert.That(() => nonFailingLazy(100) == 4, "A second read of the lazily computed value should not change the value");
        PAssert.That(() => count == 2, "A second read of the lazily computed value must not have side-effects");

        _ = Assert.Throws<Exception>(() => _ = sometimesFailingLazy(2));
        PAssert.That(() => count == 3);
        PAssert.That(() => sometimesFailingLazy(7) == 11);
    }

    [Fact]
    public void MaandSpanTest()
    {
        PAssert.That(() => Utils.MaandSpan(new(2000, 1, 1), new(2000, 1, 1)) == 0);
        PAssert.That(() => Utils.MaandSpan(new(2000, 5, 1), new(2000, 1, 1)) == 4);
        PAssert.That(() => Utils.MaandSpan(new(2000, 1, 1), new(2001, 1, 1)) == 12);
        PAssert.That(() => Utils.MaandSpan(new(2001, 1, 1), new(2000, 1, 1)) == 12);
        PAssert.That(() => Utils.MaandSpan(new(2000, 9, 1), new(2001, 2, 1)) == 5);
        PAssert.That(() => Utils.MaandSpan(new(2000, 9, 1), new(2001, 4, 1)) == 7);
        PAssert.That(() => Utils.MaandSpan(new(2001, 6, 1), new(2000, 9, 1)) == 9);
        PAssert.That(() => Utils.MaandSpan(new(2000, 12, 1), new(2001, 1, 1)) == 1);
    }

    [Fact]
    public void ComparisonComparer_supports_ordering_by_the_comparison()
    {
        var arr = new[] { (1, 200), (3, 7), (1, 2), (9, 3) };
        Array.Sort(arr, new ComparisonComparer<(int, int)>((a, b) => Math.Abs(a.Item1 - a.Item2).CompareTo(Math.Abs(b.Item1 - b.Item2))));

        var expected = new[] { (1, 2), (3, 7), (9, 3), (1, 200), };
        PAssert.That(() => arr.SequenceEqual(expected));
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
        => PAssert.That(() => Equals(default(CommandTimeout), CommandTimeout.DeferToConnectionDefault));

    [Fact]
    public void SimpleTransitiveClosureWorks()
    {
        var nodes = new[] { 2, 3, };

        PAssert.That(() => Utils.TransitiveClosure(nodes, num => new[] { num * 2 % 6, }).SetEquals(new[] { 2, 4, 0, 3, }));
    }

    [Fact]
    public void MultiTransitiveClosureWorks()
    {
        var nodes = new[] { 2, 3, };

        PAssert.That(
            () =>
                Utils.TransitiveClosure(nodes, nums => nums.Select(num => num * 2 % 6))
                    .SetEquals(new[] { 2, 4, 0, 3, })
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
        var buggyCleanup = () => {
            cleanupCalled++;
            throw new("42");
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
        var buggyComputation = () => {
            try {
                return 42;
            } finally {
#pragma warning disable CA2219 // Do not raise exceptions in finally clauses
                throw new("1337");
#pragma warning restore CA2219 // Do not raise exceptions in finally clauses
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
                        throw new("1337");
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
        var buggyCleanup = () => {
            cleanupCalled++;
            throw new("42");
        };
        var buggyComputation = () => {
            try {
                return 42;
            } finally {
#pragma warning disable CA2219 // Do not raise exceptions in finally clauses
                throw new("1337");
#pragma warning restore CA2219 // Do not raise exceptions in finally clauses
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
        var buggyCleanup = () => {
            cleanupCalled++;
            throw new("42");
        };
        Action buggyComputation = () => throw new("1337");

        var aggEx = Assert.ThrowsAny<AggregateException>(() => Utils.TryWithCleanup(buggyComputation, buggyCleanup));

        PAssert.That(() => aggEx.TargetSite != buggyCleanup.Method && aggEx.TargetSite != buggyComputation.Method);
        var innerExceptions = aggEx.InnerExceptions;
        PAssert.That(() => innerExceptions.Count == 2);
        PAssert.That(() => innerExceptions[0].Message == "1337" && innerExceptions[0].TargetSite == buggyComputation.Method);
        PAssert.That(() => innerExceptions[1].Message == "42" && innerExceptions[1].TargetSite == buggyCleanup.Method);
        PAssert.That(() => cleanupCalled == 1);
    }
}
