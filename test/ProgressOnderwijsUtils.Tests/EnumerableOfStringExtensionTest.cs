using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class EnumerableOfStringExtensionTest
    {
        [Fact]
        public void testJoin()
            => PAssert.That(() => new[] { "een", "twee", "drie" }.JoinStrings() == "eentweedrie");

        [Fact]
        public void EmptyJoin()
            => PAssert.That(() => new string[] { }.JoinStrings() == "");

        [Fact]
        public void EmptyJoinStrings()
            => PAssert.That(() => new string[] { }.JoinStrings("!") == "");

        [Fact]
        public void FastJoin()
        {
            var ints = Enumerable.Range(0, 20000).Select(i => i.ToStringInvariant()).ToArray();
            var time = BenchTimer.BestTime(() => ints.JoinStrings(), 5);
            PAssert.That(() => time.TotalMilliseconds < 5.0);
        }

        [Fact]
        public void testJoinStrings()
            => PAssert.That(() => new[] { "een", "twee", "drie" }.JoinStrings("!") == "een!twee!drie");

        [Fact]
        public void testBiggerJoinStrings()
            => PAssert.That(() => new[] { "een", "twee", "drie" }.JoinStrings("123") == "een123twee123drie");

        [Fact]
        public void testEmptyJoinStrings()
            => PAssert.That(() => new string[] { }.JoinStrings("!") == "");

        [Fact]
        public void testJoinShortStrings()
            => PAssert.That(() => new[] { "", "0", "1", "2" }.JoinStrings(",") == ",0,1,2");

        [Fact]
        public void JoinStringsLimitLength_works_like_normal_join_when_count_is_smaller_than_or_equal_to_max_count()
            => PAssert.That(() => new[] { "1", "2" }.JoinStringsLimitLength(" ", 2) == "1 2");

        [Fact]
        public void JoinStringsLimitLength_limits_length_when_count_is_greater_than_max_count()
            => PAssert.That(() => new[] { "1", "2", "3" }.JoinStringsLimitLength(" ", 2) == "1 2 ...");
    }
}
