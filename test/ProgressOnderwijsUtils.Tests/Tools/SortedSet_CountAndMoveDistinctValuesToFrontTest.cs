using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using NUnit.Framework;
using IntSet = Progress.Business.Tools.SortedSet<int, Progress.Business.Tests.Tools.IntOrdering>;

namespace Progress.Business.Tests.Tools
{
    public sealed class SortedSet_CountAndMoveDistinctValuesToFrontTest
    {
        [Test]
        public void EmptyArrayDoesNothing()
        {
            var arr = Array.Empty<int>();

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => distinctCount == 0);
        }

        [Test]
        public void SingleElementIsRetained()
        {
            var arr = new[] { 42 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 42 }));
        }

        [Test]
        public void TwoDistinctElementsAreBothRetained()
        {
            var arr = new[] { -42, 42 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { -42, 42 }));
        }

        [Test]
        public void ThreeUniqueElementsAreRetained()
        {
            var arr = new[] { 10, 100, 1000 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 10, 100, 1000 }));
        }

        [Test]
        public void TwoIdenticalElementsAreDeduplicated()
        {
            var arr = new[] { 13, 13 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 13 }));
        }

        [Test]
        public void CanRemoveSequencesOfThreeTwice()
        {
            var arr = new[] { 1, 2, 2, 2, 3, 3, 3 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, arr.Length);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 1, 2, 3 }));
        }

        [Test]
        public void IgnoresItemsPastLength()
        {
            var arr = new[] { 1, 2, 2, 2, 3, 3, 3 };

            var distinctCount = IntSet.Algorithms.CountAndMoveDistinctValuesToFront(arr, 3);
            PAssert.That(() => arr.Take(distinctCount).SequenceEqual(new[] { 1, 2, }));
        }
    }
}
