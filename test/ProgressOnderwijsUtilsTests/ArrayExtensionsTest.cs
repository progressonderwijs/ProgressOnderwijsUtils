using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    
    public class ArrayExtensionsTest
    {
        [Test]
        public void AppendingNullToNullReturnsNull()
        {
            PAssert.That(() => default(int[]).AppendArrays(default(int[])) == null);
        }

        [Test]
        public void AppendingSomethingToNullOrViceVersaReturnsSomething()
        {
            var arr = new[] { "foo", "baar" };
            PAssert.That(() => default(string[]).AppendArrays(arr) == arr);
            PAssert.That(() => arr.AppendArrays(default(string[])) == arr);
        }

        [Test]
        public void AppendingingArraysIsEquivalentToConcat()
        {
            var arrA = new[] { "paper", "scissors", "stone" };
            var arrB = new[] { "lizard", "spock" };
            PAssert.That(() => arrA.AppendArrays(arrB).SequenceEqual(new[] { "paper", "scissors", "stone", "lizard", "spock" }));
            PAssert.That(() => arrB.AppendArrays(arrA).SequenceEqual(new[] { "lizard", "spock", "paper", "scissors", "stone" }));
        }

        [Test]
        public void AppendingingEmptyWorks()
        {
            var arr = new[] { "foo", "baar" };
            PAssert.That(() => new string[0].AppendArrays(arr).SequenceEqual(arr));
            PAssert.That(() => arr.AppendArrays(new string[0]).SequenceEqual(arr));
        }
    }
}
