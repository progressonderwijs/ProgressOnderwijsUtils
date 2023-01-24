namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class SelectIndexableTest
{
    [Fact]
    public void IndexableIsLazyAndMapsCorrectly()
    {
        var arr = new[] { 1, 2, 3, };
        var mapped = arr.SelectIndexable(CountCalls((int n) => n + 1, out var callCountGet));

        PAssert.That(() => callCountGet() == 0, "Should not have called the mapping function yet");
        _ = mapped.Skip(1).Take(1).Count();
        PAssert.That(() => callCountGet() == 2, "Should have called the mapper only as often as necessary");
        PAssert.That(() => mapped.SequenceEqual(new[] { 2, 3, 4 }));
    }

    [Fact]
    public void Indexable2IsLazyAndMapsCorrectly()
    {
        var arr = new[] { 1, 2, 3, };
        var mapped = arr.SelectIndexable(CountCalls((int n, int idx) => n + 1 + idx, out var callCountGet));

        PAssert.That(() => callCountGet() == 0, "Should not have called the mapping function yet");
        _ = mapped.Skip(1).Take(1).Count();
        PAssert.That(() => callCountGet() == 2, "Should have called the mapper only as often as necessary");
        PAssert.That(() => mapped.SequenceEqual(new[] { 2, 4, 6 }));
    }

    [Fact]
    public void CountableIsLazyAndMapsCorrectly()
    {
        var arr = new[] { 1, 2, 3, };
        var mapped = arr.SelectCountable(CountCalls((int n) => n + 1, out var callCountGet));

        PAssert.That(() => callCountGet() == 0, "Should not have called the mapping function yet");
        _ = mapped.Skip(1).Take(1).Count();
        PAssert.That(() => callCountGet() == 2, "Should have called the mapper only as often as necessary");
        PAssert.That(() => mapped.SequenceEqual(new[] { 2, 3, 4 }));
    }




    static Func<A, X> CountCalls<A, X>(Func<A, X> func, out Func<int> callCount)
    {
        var callCounter = 0;
        callCount = () => callCounter;
        return n => {
            callCounter++;
            return func(n);
        };
    }

    static Func<A, B, X> CountCalls<A, B, X>(Func<A, B, X> func, out Func<int> callCount)
    {
        var callCounter = 0;
        callCount = () => callCounter;
        return (a, b) => {
            callCounter++;
            return func(a, b);
        };
    }
}
