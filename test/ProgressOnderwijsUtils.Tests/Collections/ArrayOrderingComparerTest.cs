namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class ArrayOrderingComparerTest
{
    static int Compare(int[]? a, int[]? b)
        => ArrayOrderingComparer<int>.Default.Compare(a, b);

    static void AssertLessThan(int[]? a, int[]? b)
        => PAssert.That(() => Compare(a, b) < 0 && Compare(b, a) > 0);

    [Fact]
    public void NullIsLessThanEmpty()
        => AssertLessThan(null, []);

    [Fact]
    public void SingleElementArraysCompare()
    {
        AssertLessThan([1,], [2,]);
        AssertLessThan([2,], [100,]);
        AssertLessThan([int.MinValue,], [int.MaxValue,]);
    }

    [Fact]
    public void AdditionalElementsArentRelevant()
    {
        AssertLessThan([1,], [2,]);
        AssertLessThan([1, 100,], [2, 0,]);
    }

    [Fact]
    public void ArrayPrefixesAreLessThan()
    {
        AssertLessThan([1, 2, 3,], [1, 2, 3, 4,]);
        AssertLessThan([1, 1, 1,], [1, 1, 1, 4,]);
    }

    [Fact]
    public void ArraysSharingPrefixesAreComparedAfterThatPrefix()
    {
        AssertLessThan([1, 2, 3, 4,], [1, 2, 3, 5,]);
        AssertLessThan([1, 1, 1, 1000,], [1, 1, 1, 2000,]);
    }
}
