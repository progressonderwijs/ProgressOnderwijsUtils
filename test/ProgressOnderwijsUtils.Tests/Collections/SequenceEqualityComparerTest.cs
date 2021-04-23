using System;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    public sealed class SequenceEqualityComparerTest
    {
        static void AssertEquality(int[]? a, int[]? b, bool shouldBeEqual)
        {
            var eq = SequenceEqualityComparer<int>.Default;
            var aSeq = a?.AsEnumerable();
            var bSeq = b?.AsEnumerable();
            PAssert.That(() => eq.Equals(a, b) == shouldBeEqual);
            PAssert.That(() => eq.Equals(aSeq, bSeq) == shouldBeEqual);
            PAssert.That(() => (eq.GetHashCode(a) == eq.GetHashCode(b)) == shouldBeEqual);
            PAssert.That(() => (eq.GetHashCode(aSeq) == eq.GetHashCode(bSeq)) == shouldBeEqual);
        }

        [Fact]
        public void NullIsNotEmpty()
        {
            AssertEquality(null, new int[0], false);
            AssertEquality(Array.Empty<int>(), new int[0], true);
        }

        [Fact]
        public void SingleElementArraysCompare()
        {
            AssertEquality(new[] { 1 }, new[] { 2 }, false);
            AssertEquality(new[] { 2 }, new[] { 2 }, true);
            var sharedRef = new[] { int.MinValue };
            AssertEquality(sharedRef, sharedRef, true);
        }

        [Fact]
        public void AdditionalElementsArentRelevant()
        {
            AssertEquality(new[] { 1 }, new[] { 1 }, true);
            AssertEquality(new[] { 1, 100 }, new[] { 1, 0 }, false);
            AssertEquality(new[] { 1, 100 }, new[] { 1, 100 }, true);
            AssertEquality(new[] { 1, 100 }, new[] { 0, 100 }, false);
        }

        [Fact]
        public void LengthDifferencesMatter()
        {
            AssertEquality(new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4 }, false);
            AssertEquality(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3 }, false);
        }
    }
}
