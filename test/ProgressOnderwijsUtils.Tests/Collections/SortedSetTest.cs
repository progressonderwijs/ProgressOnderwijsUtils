using IntSet = ProgressOnderwijsUtils.Collections.SortedSet<int, ProgressOnderwijsUtils.Tests.Collections.IntOrdering>;

namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class SortedSetTest
{
    [Fact]
    public void TheEmptySetIsEmpty()
    {
        var set = IntSet.FromValues(Array.Empty<int>());
        PAssert.That(() => set.ValuesInOrder.None());
    }

    [Fact]
    public void ThreeDistinctValuesRemainDistinct()
    {
        var set = IntSet.FromValues(new[] { 1, 2, 3, });
        PAssert.That(() => set.ValuesInOrder.SetEqual(new[] { 3, 2, 1, }));
    }

    [Fact]
    public void IdenticalValuesAreRemoved()
    {
        var set = IntSet.FromValues(new[] { 1, 2, 3, 3, 2, 1, });
        PAssert.That(() => set.ValuesInOrder.SequenceEqual(new[] { 1, 2, 3, }));
    }

    [Fact]
    public void ContainsReturnsTrueForANumberInSet()
    {
        var set = IntSet.FromValues(new[] { 1, 2, 4, 8, 16, 32, });
        PAssert.That(() => set.Contains(16));
    }

    [Fact]
    public void ContainsReturnsTrueForANegativeNumberInSet()
    {
        var set = IntSet.FromValues(new[] { 1, 2, 4, -8, 16, 32, });
        PAssert.That(() => set.Contains(-8));
    }

    [Fact]
    public void ContainsReturnsFalseForANumberNotSet()
    {
        var set = IntSet.FromValues(Enumerable.Range(0, 1000).Select(i => i * 2).Reverse());
        PAssert.That(() => !set.Contains(299));
    }
}
