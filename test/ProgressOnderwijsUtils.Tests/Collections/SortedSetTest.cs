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

    [Fact]
    public void IsSubSet_small_example()
    {
        var superset = IntSet.FromValues([22462, 113372, 115148, 197410,]);
        PAssert.That(() => !IntSet.FromValues(new[] { 197665, }).IsSubsetOf(superset));
        PAssert.That(() => IntSet.FromValues(new[] { 115148, }).IsSubsetOf(superset));
        PAssert.That(() => IntSet.Empty.IsSubsetOf(superset));
    }

    [Fact]
    public void IsSubSet_large_example()
    {
        var superset = IntSet.FromValues(Enumerable.Range(0, 1000).Select(i => i * i));

        PAssert.That(() => !IntSet.FromValues(new[] { 36, 49, 50, 64, }).IsSubsetOf(superset));
        PAssert.That(() => IntSet.FromValues(new[] { 36, 49, 64, 100, 400, 256, }).IsSubsetOf(superset));
        PAssert.That(() => IntSet.Empty.IsSubsetOf(superset));
    }

    [Fact]
    public void IsSubSet_smallSets()
    {
        for (var size = 0; size < 100; size++) {
            for (var largerSize = size; largerSize < size * 10 + 10; largerSize += 1 + (largerSize >> 2)) {
                var r = new Random(42 + 13 * size + 37 * largerSize);
                var largeSetValues = Enumerable.Range(0, largerSize).Select(_ => r.Next(100_000) * 2).ToArray();
                var smallContainedSet = IntSet.FromValues(largeSetValues[..size]);
                var smallNonContainedSet = IntSet.FromValues(largeSetValues[..size].Append(r.Next(100_000) * 2 + 1));
                var largeSet = IntSet.FromValues(largeSetValues);

                PAssert.That(() => smallContainedSet.IsSubsetOf(largeSet));
                PAssert.That(() => !smallNonContainedSet.IsSubsetOf(largeSet));
                PAssert.That(() => smallContainedSet.IsSubsetOf(smallNonContainedSet));
            }
        }
    }

    [Fact]
    public void IsSubSet_largeSets()
    {
        for (var size = 0; size < 100; size++) {
            for (var largerSize = size * 10 + 10; largerSize < size * 30 + 30; largerSize += 1 + (largerSize >> 1)) {
                var r = new Random(37 + 11 * size + 31 * largerSize);
                var largeSetValues = Enumerable.Range(0, largerSize).Select(_ => r.Next(100_000) * 2).ToArray();
                var smallContainedSet = IntSet.FromValues(largeSetValues[..size]);
                var smallNonContainedSet = IntSet.FromValues(largeSetValues[..size].Append(r.Next(100_000) * 2 + 1));
                var largeSet = IntSet.FromValues(largeSetValues);

                PAssert.That(() => smallContainedSet.IsSubsetOf(largeSet));
                PAssert.That(() => !smallNonContainedSet.IsSubsetOf(largeSet));
                PAssert.That(() => smallContainedSet.IsSubsetOf(smallNonContainedSet));
            }
        }
    }
}
