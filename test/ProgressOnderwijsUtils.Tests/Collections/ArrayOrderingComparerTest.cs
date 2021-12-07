using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class ArrayOrderingComparerTest
{
    static int Compare(int[]? a, int[]? b)
        => ArrayOrderingComparer<int>.Default.Compare(a, b);

    static void AssertLessThan(int[]? a, int[]? b)
        => PAssert.That(() => Compare(a, b) < 0 && Compare(b, a) > 0);

    [Fact]
    public void NullIsLessThanEmpty()
        => AssertLessThan(null, new int[0]);

    [Fact]
    public void SingleElementArraysCompare()
    {
        AssertLessThan(new[] { 1 }, new[] { 2 });
        AssertLessThan(new[] { 2 }, new[] { 100 });
        AssertLessThan(new[] { int.MinValue }, new[] { int.MaxValue });
    }

    [Fact]
    public void AdditionalElementsArentRelevant()
    {
        AssertLessThan(new[] { 1 }, new[] { 2 });
        AssertLessThan(new[] { 1, 100 }, new[] { 2, 0 });
    }

    [Fact]
    public void ArrayPrefixesAreLessThan()
    {
        AssertLessThan(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4 });
        AssertLessThan(new[] { 1, 1, 1 }, new[] { 1, 1, 1, 4 });
    }

    [Fact]
    public void ArraysSharingPrefixesAreComparedAfterThatPrefix()
    {
        AssertLessThan(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 5 });
        AssertLessThan(new[] { 1, 1, 1, 1000 }, new[] { 1, 1, 1, 2000 });
    }
}