using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace ProgressOnderwijsUtils
{
	public static class EnumerableOfStringExtension
	{
		/// <summary>
		/// Joins a collection of strings into a single string
		/// </summary>
		/// <param name="strings">string collection</param>
		/// <param name="joiner">joiner string</param>
		/// <returns>a string</returns>
		public static string JoinStrings(this IEnumerable<string> strings)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string s in strings)
				sb.Append(s);
			return sb.ToString();
		}
		
		/// <summary>
		/// Join a collection of strings using a joiner string
		/// </summary>
		/// <param name="strings">string collection</param>
		/// <param name="separator">separator string</param>
		/// <returns>a string</returns>
		public static string JoinStrings(this IEnumerable<string> strings, string separator)
		{
			return string.Join(separator, strings.ToArray());
		}
	}

	[TestFixture]
	public class IEnumerableOfStringExtensionTest
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
			var ints = Enumerable.Range(0, 20000).Select(i => i.ToString()).ToArray();
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
			Assert.That(() => new [] {"","0","1","2" }.JoinStrings(","), Is.EqualTo(",0,1,2"));
		}
	}
}
