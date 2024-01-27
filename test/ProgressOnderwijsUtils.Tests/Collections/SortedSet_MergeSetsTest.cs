using System.Linq;
using IntSet = ProgressOnderwijsUtils.Collections.SortedSet<int, ProgressOnderwijsUtils.Tests.Collections.IntOrdering>;

namespace ProgressOnderwijsUtils.Tests.Collections;

public sealed class SortedSet_MergeSetsTest
{
    [Fact]
    public void MergeOfTwoEmptySetsIsEmpty()
    {
        var mergeResult = new[] { IntSet.Empty, IntSet.Empty, }.MergeSets();
        PAssert.That(() => mergeResult.ValuesInOrder.Length == 0);
    }

    [Fact]
    public void TheEmptyMergeIsEmpty()
    {
        var mergeResult = Array.Empty<IntSet>().MergeSets();
        PAssert.That(() => mergeResult.ValuesInOrder.Length == 0);
    }

    [Fact]
    public void MergeOfASetWithItselfIsThatSet()
    {
        var set = IntSet.FromValues(new[] { 3, 5, 2, 9, 9, });
        var mergeResult = new[] { set, set, set, set, }.MergeSets();
        PAssert.That(() => set.Equals(mergeResult));
    }

    [Fact]
    public void MergeRemovesDuplicatedItems()
    {
        var setA = IntSet.FromValues(new[] { 3, 5, 2, 9, 9, });
        var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2, });
        var setC = IntSet.FromValues(new[] { 1, 3, 4, });
        var setExpected = IntSet.FromValues(Enumerable.Range(1, 9));
        var mergeResult = new[] { setA, setB, setC, }.MergeSets();
        PAssert.That(() => setExpected.Equals(mergeResult));
    }

    [Fact]
    public void MergeWorksShortSetFirst()
    {
        var setA = IntSet.FromValues(new[] { 3, 5, });
        var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2, 2, 9, 9, 10, 4, 1, });
        var setExpected = IntSet.FromValues(Enumerable.Range(1, 10));
        var mergeResult = new[] { setA, setB, }.MergeSets();
        PAssert.That(() => setExpected.Equals(mergeResult));
    }

    [Fact]
    public void MergeWorksLongSetFirst()
    {
        var setA = IntSet.FromValues(new[] { 3, 5, });
        var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2, 2, 9, 9, 10, 4, 1, });
        var setExpected = IntSet.FromValues(Enumerable.Range(1, 10));
        var mergeResult = new[] { setB, setA, }.MergeSets();
        PAssert.That(() => setExpected.Equals(mergeResult));
    }

    [Fact]
    public void ALargeMergeWorks()
    {
        var sets = Enumerable.Range(1, 100)
            .Select(n => IntSet.FromValues(Enumerable.Range(1, 1000).Select(i => i * n % 2345)));
        var setExpected = IntSet.FromValues(Enumerable.Range(0, 2345));
        var mergeResult = sets.MergeSets();
        PAssert.That(() => setExpected.Equals(mergeResult));
    }

    [Fact]
    public void TwoWayMergeRemovesDuplicatedItems()
    {
        var setA = IntSet.FromValues([3, 5, 2, 9, 9,]);
        var setB = IntSet.FromValues([2, 8, 7, 6, 2,]);
        var output = IntSet.Algorithms.Merge_RemovingDuplicates(setA.ValuesInOrder, setB.ValuesInOrder);
        PAssert.That(() => output.SequenceEqual(new[] { 2, 3, 5, 6, 7, 8, 9, }));
    }

    static int[] ToOrderedSetOfBits(int value)
    {
        var popcount = 0;
        for (var i = 0; i < 32; i++) {
            popcount += value >> i & 1;
        }
        var retval = new int[popcount];
        var idx = 0;
        for (var i = 0; i < 32; i++) {
            if ((value >> i & 1) != 0) {
                retval[idx++] = i;
            }
        }
        return retval;
    }

    [Fact]
    public void Exhaustive5bitTwoWayMergeCheck()
    {
        PAssert.That(() => ToOrderedSetOfBits((1<<1) + (1<<2) + (1<<4) + (1<<7)).SequenceEqual(new[] { 1, 2, 4, 7, }));
        var sets = Enumerable.Range(0, 32).Select(num => new { num, setOfBits = ToOrderedSetOfBits(num), }).ToArray();

        foreach (var a in sets) {
            foreach (var b in sets) {
                var expected = sets[a.num | b.num].setOfBits;
                var setA = a.setOfBits; 
                var setB = b.setOfBits; 
                PAssert.That(() => IntSet.Algorithms.Merge_RemovingDuplicates(setA, setB).SequenceEqual(expected));
            }
        }
    }
}
