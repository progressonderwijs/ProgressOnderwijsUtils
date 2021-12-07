using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;
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
        var mergeResult = System.Array.Empty<IntSet>().MergeSets();
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
}