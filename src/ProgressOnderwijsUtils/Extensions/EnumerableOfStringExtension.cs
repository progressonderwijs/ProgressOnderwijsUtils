using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class EnumerableOfStringExtension
	{
		/// <summary>
		/// Concatenate a sequence of strings.
		/// </summary>
		/// <param name="strings">string sequence</param>
		/// <returns>a string</returns>
		public static string JoinStrings(this IEnumerable<string> strings) { return JoinStrings(strings, ""); }
		//don't use optional params to allow usage in expression trees

		/// <summary>
		/// Concatenate a sequence of strings with an optional separator.  A sequence of N items includes the separator N-1 times.
		/// </summary>
		/// <param name="strings">string sequence</param>
		/// <param name="separator">separator string</param>
		/// <returns>a string</returns>
		public static string JoinStrings(this IEnumerable<string> strings, string separator)
		{
			return string.Join(separator, strings);
		}
	}

	[TestFixture]
	public sealed class EnumerableOfStringExtensionTest
	{

		[Test]
		public void testJoin()
		{
			Assert.That(() => new[] { "een", "twee", "drie" }.JoinStrings(), Is.EqualTo("eentweedrie"));
		}

		[Test]
		public void EmptyJoin()
		{
			Assert.That(() => new string[] { }.JoinStrings(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void EmptyJoinStrings()
		{
			Assert.That(() => new string[] { }.JoinStrings("!"), Is.EqualTo(string.Empty));
		}
		[Test]
		public void FastJoin()
		{
			var ints = Enumerable.Range(0, 20000).Select(i => i.ToStringInvariant()).ToArray();
			var time = BenchTimer.MinimumTime(() => ints.JoinStrings());
			Assert.That(time.TotalMilliseconds, Is.LessThan(5.0));
		}

		[Test]
		public void testJoinStrings()
		{
			Assert.That(() => new[] { "een", "twee", "drie" }.JoinStrings("!"), Is.EqualTo("een!twee!drie"));
		}

		[Test]
		public void testBiggerJoinStrings()
		{
			Assert.That(() => new[] { "een", "twee", "drie" }.JoinStrings("123"), Is.EqualTo("een123twee123drie"));
		}

		[Test]
		public void testEmptyJoinStrings()
		{
			Assert.That(() => new string[] { }.JoinStrings("!"), Is.EqualTo(""));
		}
		[Test]
		public void testJoinShortStrings()
		{
			Assert.That(() => new[] { "", "0", "1", "2" }.JoinStrings(","), Is.EqualTo(",0,1,2"));
		}
	}
}
