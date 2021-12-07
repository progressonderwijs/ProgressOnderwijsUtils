namespace ProgressOnderwijsUtils.Tests;

public sealed class ArrayExtensionsTest
{
    [Fact]
    public void AppendingNullToNullReturnsEmpty()
        => PAssert.That(() => default(int[]).ConcatArray(null) == Array.Empty<int>());

    [Fact]
    public void AppendingSomethingToNullOrViceVersaReturnsSomething()
    {
        var arr = new[] { "foo", "baar", };
        PAssert.That(() => default(string[]).ConcatArray(arr) == arr);
        PAssert.That(() => arr.ConcatArray(default(string[])) == arr);
    }

    [Fact]
    public void AppendingingArraysIsEquivalentToConcat()
    {
        var arrA = new[] { "paper", "scissors", "stone", };
        var arrB = new[] { "lizard", "spock", };
        PAssert.That(() => arrA.ConcatArray(arrB).SequenceEqual(new[] { "paper", "scissors", "stone", "lizard", "spock", }));
        PAssert.That(() => arrB.ConcatArray(arrA).SequenceEqual(new[] { "lizard", "spock", "paper", "scissors", "stone", }));
    }

    [Fact]
    public void AppendingingArraysIsEquivalentToConcat2()
    {
        var arrA = new[] { "paper", "scissors", "stone", };
        var arrB = new[] { "lizard", "spock", };
        PAssert.That(() => new[] { arrA, arrB, }.ConcatArrays().SequenceEqual(new[] { "paper", "scissors", "stone", "lizard", "spock", }));
        PAssert.That(() => new[] { arrB, arrA, }.ConcatArrays().SequenceEqual(new[] { "lizard", "spock", "paper", "scissors", "stone", }));
    }

    [Fact]
    public void ConcatArraysSupportsMixedTypes()
    {
        var obj = new object();
        var arrA = new[] { "string", };
        var arrB = new[] { obj, };
        PAssert.That(() => new[] { arrA, arrB, }.ConcatArrays().SequenceEqual(new[] { "string", obj, }));
        PAssert.That(() => new[] { arrB, arrA, }.ConcatArrays().SequenceEqual(new[] { obj, "string", }));
    }

    [Fact]
    public void AppendingingEmptyWorks()
    {
        var arr = new[] { "foo", "baar", };
        PAssert.That(() => new string[0].ConcatArray(arr).SequenceEqual(arr));
        PAssert.That(() => arr.ConcatArray(new string[0]).SequenceEqual(arr));
        PAssert.That(() => new string[0].ConcatArray(arr) == arr);
        PAssert.That(() => arr.ConcatArray(new string[0]) == arr);
    }

    [Fact]
    public void SelectManyFollowsLinqSemantics()
    {
        var arr = new[] { "foo", "baar", };
        var viaLinq = arr.AsEnumerable().SelectMany(a => a.ToCharArray());
        var viaArrays = arr.SelectMany(a => a.ToCharArray());

        PAssert.That(() => viaLinq.SequenceEqual(viaArrays));
    }
}
