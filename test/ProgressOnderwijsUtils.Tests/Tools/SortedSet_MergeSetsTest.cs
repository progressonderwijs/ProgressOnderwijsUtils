using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Tools;
using IntSet = Progress.Business.Tools.SortedSet<int, Progress.Business.Tests.Tools.IntOrdering>;

namespace Progress.Business.Tests.Tools
{
    public sealed class SortedSet_MergeSetsTest
    {
        [Test]
        public void MergeOfTwoEmptySetsIsEmpty()
        {
            var mergeResult = new[] { IntSet.Empty, IntSet.Empty }.MergeSets();
            PAssert.That(() => mergeResult.ValuesInOrder.Length == 0);
        }

        [Test]
        public void TheEmptyMergeIsEmpty()
        {
            var mergeResult = new IntSet[0].MergeSets();
            PAssert.That(() => mergeResult.ValuesInOrder.Length == 0);
        }

        [Test]
        public void MergeOfASingleSetIsThatSet()
        {
            var set = IntSet.FromValues(new[] { 3, 5, 2 });
            var mergeResult = new[] { set }.MergeSets();
            PAssert.That(() => set.Equals(mergeResult));
            PAssert.That(() => Equals(set.ValuesInOrder, mergeResult.ValuesInOrder), "Failed optimization: if the set is identical; reuse array");
        }

        [Test]
        public void MergeOfEmptyWithASetIsThatSet()
        {
            var set = IntSet.FromValues(new[] { 3, 5, 2 });
            var mergeResult = new[] { IntSet.Empty, set, IntSet.Empty }.MergeSets();
            PAssert.That(() => set.Equals(mergeResult));
            PAssert.That(() => Equals(set.ValuesInOrder, mergeResult.ValuesInOrder), "Failed optimization: if the set is identical; reuse array");
        }

        [Test]
        public void MergeOfASetWithItselfIsThatSet()
        {
            var set = IntSet.FromValues(new[] { 3, 5, 2, 9, 9 });
            var mergeResult = new[] { set, set, set, set }.MergeSets();
            PAssert.That(() => set.Equals(mergeResult));
        }

        [Test]
        public void MergeRemovesDuplicatedItems()
        {
            var setA = IntSet.FromValues(new[] { 3, 5, 2, 9, 9 });
            var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2 });
            var setC = IntSet.FromValues(new[] { 1, 3, 4, });
            var setExpected = IntSet.FromValues(Enumerable.Range(1, 9).ToArray());
            var mergeResult = new[] { setA, setB, setC, }.MergeSets();
            PAssert.That(() => setExpected.Equals(mergeResult));
        }

        [Test]
        public void MergeWorksShortSetFirst()
        {
            var setA = IntSet.FromValues(new[] { 3, 5, });
            var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2, 2, 9, 9, 10, 4, 1 });
            var setExpected = IntSet.FromValues(Enumerable.Range(1, 10).ToArray());
            var mergeResult = new[] { setA, setB, }.MergeSets();
            PAssert.That(() => setExpected.Equals(mergeResult));
        }

        [Test]
        public void MergeWorksLongSetFirst()
        {
            var setA = IntSet.FromValues(new[] { 3, 5, });
            var setB = IntSet.FromValues(new[] { 2, 8, 7, 6, 2, 2, 9, 9, 10, 4, 1 });
            var setExpected = IntSet.FromValues(Enumerable.Range(1, 10).ToArray());
            var mergeResult = new[] { setB, setA }.MergeSets();
            PAssert.That(() => setExpected.Equals(mergeResult));
        }

        [Test]
        public void ALargeMergeWorks()
        {
            var sets = Enumerable.Range(1, 100)
                .Select(n => IntSet.FromValues(Enumerable.Range(1, 1000).Select(i => i * n % 2345).ToArray()));
            var setExpected = IntSet.FromValues(Enumerable.Range(0, 2345).ToArray());
            var mergeResult = sets.MergeSets();
            PAssert.That(() => setExpected.Equals(mergeResult));
        }
    }
}
