using System;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ArrayExtensionsTest
    {
        [Fact]
        public void AppendingNullToNullReturnsEmpty()
            => PAssert.That(() => default(int[]).ConcatArray(default) == Array.Empty<int>());

        [Fact]
        public void AppendingSomethingToNullOrViceVersaReturnsSomething()
        {
            var arr = new[] { "foo", "baar" };
            PAssert.That(() => default(string[]).ConcatArray(arr) == arr);
            PAssert.That(() => arr.ConcatArray(default(string[])) == arr);
        }

        [Fact]
        public void AppendingingArraysIsEquivalentToConcat()
        {
            var arrA = new[] { "paper", "scissors", "stone" };
            var arrB = new[] { "lizard", "spock" };
            PAssert.That(() => arrA.ConcatArray(arrB).SequenceEqual(new[] { "paper", "scissors", "stone", "lizard", "spock" }));
            PAssert.That(() => arrB.ConcatArray(arrA).SequenceEqual(new[] { "lizard", "spock", "paper", "scissors", "stone" }));
        }

        [Fact]
        public void AppendingingEmptyWorks()
        {
            var arr = new[] { "foo", "baar" };
            PAssert.That(() => new string[0].ConcatArray(arr).SequenceEqual(arr));
            PAssert.That(() => arr.ConcatArray(new string[0]).SequenceEqual(arr));
        }
    }
}
