using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using IntSet = Progress.Business.Tools.SortedSet<int, Progress.Business.Tests.Tools.IntOrdering>;

namespace Progress.Business.Tests.Tools
{
    public sealed class SortedSetTest
    {
        [Test]
        public void TheEmptySetIsEmpty()
        {
            var set = IntSet.FromValues(new int[] { });
            PAssert.That(() => set.ValuesInOrder.None());
        }

        [Test]
        public void ThreeDistinctValuesRemainDistinct()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 3 });
            PAssert.That(() => set.ValuesInOrder.SetEqual(new[] { 3, 2, 1 }));
        }

        [Test]
        public void IdenticalValuesAreRemoved()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 3, 3, 2, 1 });
            PAssert.That(() => set.ValuesInOrder.SequenceEqual(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ContainsReturnsTrueForANumberInSet()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 4, 8, 16, 32 });
            PAssert.That(() => set.Contains(16));
        }

        [Test]
        public void ContainsReturnsTrueForANegativeNumberInSet()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 4, -8, 16, 32 });
            PAssert.That(() => set.Contains(-8));
        }

        [Test]
        public void ContainsReturnsFalseForANumberNotSet()
        {
            var set = IntSet.FromValues(Enumerable.Range(0, 1000).Select(i => i * 2).Reverse());
            PAssert.That(() => !set.Contains(299));
        }
    }
}
