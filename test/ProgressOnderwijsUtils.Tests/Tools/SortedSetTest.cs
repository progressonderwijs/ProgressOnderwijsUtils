using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            PAssert.That(() => set.Values.None());
        }

        [Test]
        public void ThreeDistinctValuesRemainDistinct()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 3 });
            PAssert.That(() => set.Values.SetEqual(new[] { 3, 2, 1 }));
        }

        [Test]
        public void IdenticalValuesAreRemoved()
        {
            var set = IntSet.FromValues(new[] { 1, 2, 3, 3, 2, 1 });
            PAssert.That(() => set.Values.SequenceEqual(new[] { 1, 2, 3 }));
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
            var set = IntSet.FromValues(Enumerable.Range(0, 1000).Select(i => i * 2).ToArray());
            PAssert.That(() => !set.Contains(299));
        }
    }
}
