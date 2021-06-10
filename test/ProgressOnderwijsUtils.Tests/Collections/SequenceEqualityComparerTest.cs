using System;
using System.Linq;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests.Collections
{
    public sealed class SequenceEqualityComparerTest
    {
        static readonly SequenceEqualityComparer<int> defaultEq = SequenceEqualityComparer<int>.Default;
        static readonly SequenceEqualityComparer<int> nullIsEmptyEq = defaultEq with { NullCountsAsEmpty = true };

        static void AssertEquality(SequenceEqualityComparer<int> eq, int[]? a, int[]? b, bool shouldBeEqual)
        {
            var aSeq = a?.AsEnumerable();
            var bSeq = b?.AsEnumerable();
            PAssert.That(() => eq.Equals(a, b) == shouldBeEqual);
            PAssert.That(() => eq.Equals(aSeq, bSeq) == shouldBeEqual);
            PAssert.That(() => eq.GetHashCode(a) == eq.GetHashCode(b) == shouldBeEqual);
            PAssert.That(() => eq.GetHashCode(aSeq) == eq.GetHashCode(bSeq) == shouldBeEqual);
        }

        [Fact]
        public void Null_vs_Empty()
        {
            AssertEquality(defaultEq, null, new int[0], false);
            AssertEquality(defaultEq, new int[0], null, false);
            AssertEquality(defaultEq, null, null, true);
            AssertEquality(defaultEq, Array.Empty<int>(), new int[0], true);

            AssertEquality(nullIsEmptyEq, null, new int[0], true);
            AssertEquality(nullIsEmptyEq, new int[0], null, true);
            AssertEquality(nullIsEmptyEq, null, null, true);
            AssertEquality(nullIsEmptyEq, Array.Empty<int>(), new int[0], true);
        }

        [Fact]
        public void SingleElementArraysCompare()
        {
            AssertEquality(defaultEq, new[] { 1 }, new[] { 2 }, false);
            AssertEquality(defaultEq, new[] { 2 }, new[] { 2 }, true);
            var sharedRef = new[] { int.MinValue };
            AssertEquality(defaultEq, sharedRef, sharedRef, true);
        }

        [Fact]
        public void AdditionalElementsArentRelevant()
        {
            AssertEquality(defaultEq, new[] { 1 }, new[] { 1 }, true);
            AssertEquality(defaultEq, new[] { 1, 100 }, new[] { 1, 0 }, false);
            AssertEquality(defaultEq, new[] { 1, 100 }, new[] { 1, 100 }, true);
            AssertEquality(defaultEq, new[] { 1, 100 }, new[] { 0, 100 }, false);
        }

        [Fact]
        public void LengthDifferencesMatter()
        {
            AssertEquality(defaultEq, new[] { 1, 2, 3 }, new[] { 1, 2, 3, 4 }, false);
            AssertEquality(defaultEq, new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3 }, false);
        }
    }
}
