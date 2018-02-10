using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using IntSet = ProgressOnderwijsUtils.Collections.SortedSet<int, ProgressOnderwijsUtils.Tests.Collections.IntOrdering>;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    public sealed class SortedSet_CountAndMoveDistinctValuesToFrontTest
    {
        [Fact]
        public void EmptyArrayDoesNothing()
        {
            var arr = Array.Empty<int>();

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => distinctCount == 0);
        }

        [Fact]
        public void SingleElementIsRetained()
        {
            var arr = new[] { 42 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 42 }));
        }

        [Fact]
        public void TwoDistinctElementsAreBothRetained()
        {
            var arr = new[] { -42, 42 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { -42, 42 }));
        }

        [Fact]
        public void ThreeUniqueElementsAreRetained()
        {
            var arr = new[] { 10, 100, 1000 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 10, 100, 1000 }));
        }

        [Fact]
        public void TwoIdenticalElementsAreDeduplicated()
        {
            var arr = new[] { 13, 13 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 13 }));
        }

        [Fact]
        public void CanRemoveSequencesOfThreeTwice()
        {
            var arr = new[] { 1, 2, 2, 2, 3, 3, 3 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 1, 2, 3 }));
        }

        [Fact]
        public void IgnoresItemsPastLength()
        {
            var arr = new[] { 1, 2, 2, 2, 3, 3, 3 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, 3);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 1, 2, }));
        }
    }
}
