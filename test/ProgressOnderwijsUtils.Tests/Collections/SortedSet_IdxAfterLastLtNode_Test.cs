using System;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    public sealed class SortedSet_IdxAfterLastLtNode_Test
    {
        static int IdxAfterLastLtNode(int[] sortedArray, int needle) //Just to make this test read a little nicer.
            => SortedSet<int, IntOrdering>.BinarySearchAlgorithm.IdxAfterLastLtNode(sortedArray, needle);

        [Fact]
        public void IndexIs0ForEmptyArray()
            => PAssert.That(() => IdxAfterLastLtNode(Array.Empty<int>(), 37) == 0);

        [Fact]
        public void IndexIs0IfNeedleIsSmallerThanSingleElement()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 42, }, 37) == 0);

        [Fact]
        public void IndexIsOneIfNeedleIsLargerThanSingleElement()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 37, }, 42) == 1);

        [Fact]
        public void IndexIs0IfNeedleIsEqualToAllValues()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 3, 3, 3, 3, }, 3) == 0);

        [Fact]
        public void IndexIs10IfNeedleIsGreaterThanAll10Values()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, }, 100) == 10);

        [Fact]
        public void IndexIs5IfNeedleEqualToNode5Value()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, }, 5) == 5);

        [Fact]
        public void IndexIs2IfNeedleBetweenValueOfNode1AndNode2()
            => PAssert.That(() => IdxAfterLastLtNode(new[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, }, 3) == 2);
    }
}
