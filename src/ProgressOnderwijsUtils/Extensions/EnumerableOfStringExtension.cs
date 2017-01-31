using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils
{
    public static class EnumerableOfStringExtension
    {
        /// <summary>
        /// Concatenate a sequence of strings.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string> strings) => JoinStrings(strings, "");

        //don't use optional params to allow usage in expression trees
        /// <summary>
        /// Concatenate a sequence of strings with an optional separator.  A sequence of N items includes the separator N-1 times.
        /// </summary>
        /// <param name="strings">string sequence</param>
        /// <param name="separator">separator string</param>
        /// <returns>a string</returns>
        public static string JoinStrings(this IEnumerable<string> strings, string separator)
            => string.Join(separator, strings);

        public static string JoinStringsLimitLength(this IReadOnlyCollection<string> strings, string separator,
            int maxCount)
        {
            return string.Join(separator, strings.Take(maxCount)) + (strings.Count > maxCount ? separator + "..." : "");
        }
    }

    public sealed class EnumerableOfStringExtensionTest
    {
        [Fact]
        public void testJoin()
        {
            PAssert.That(() => new[] {"een", "twee", "drie"}.JoinStrings() == "eentweedrie");
        }

        [Fact]
        public void EmptyJoin()
        {
            PAssert.That(() => new string[] {}.JoinStrings() == "");
        }

        [Fact]
        public void EmptyJoinStrings()
        {
            PAssert.That(() => new string[] {}.JoinStrings("!") == "");
        }

        [Fact]
        public void FastJoin()
        {
            var ints = Enumerable.Range(0, 20000).Select(i => i.ToStringInvariant()).ToArray();
            var time = BenchTimer.BestTime(() => ints.JoinStrings(), 5);
            PAssert.That(() => time.TotalMilliseconds < 5.0);
        }

        [Fact]
        public void testJoinStrings()
        {
            PAssert.That(() => new[] {"een", "twee", "drie"}.JoinStrings("!") == "een!twee!drie");
        }

        [Fact]
        public void testBiggerJoinStrings()
        {
            PAssert.That(() => new[] {"een", "twee", "drie"}.JoinStrings("123") == "een123twee123drie");
        }

        [Fact]
        public void testEmptyJoinStrings()
        {
            PAssert.That(() => new string[] {}.JoinStrings("!") == "");
        }

        [Fact]
        public void testJoinShortStrings()
        {
            PAssert.That(() => new[] {"", "0", "1", "2"}.JoinStrings(",") == ",0,1,2");
        }

        [Fact]
        public void JoinStringsLimitLength_works_like_normal_join_when_count_is_smaller_than_or_equal_to_max_count()
        {
            PAssert.That(() => new[] {"1", "2"}.JoinStringsLimitLength(" ", 2) == "1 2");
        }

        [Fact]
        public void JoinStringsLimitLength_limits_length_when_count_is_greater_than_max_count()
        {
            PAssert.That(() => new[] {"1", "2", "3"}.JoinStringsLimitLength(" ", 2) == "1 2 ...");
        }
    }
}