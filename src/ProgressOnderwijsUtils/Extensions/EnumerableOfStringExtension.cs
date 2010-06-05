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
		/* >prevous code<
		public static string Join(this IEnumerable<string> strings)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string s in strings)
				sb.Append(s);
			return sb.ToString();
		}
		
		public static string JoinStrings(this IEnumerable<string> strings, string separator)
		{
			//string.Join(separator, strings.ToArray())
			StringBuilder sb = new StringBuilder();
			bool addsep = false;
			foreach (string s in strings)
			{
				if (addsep)
					sb.Append(separator);
				else 
					addsep = true;
				sb.Append(s);
			}
			return sb.ToString();
		}*/
		/// <summary>
		/// Joins a collection into a string (w/o joiner string)
		/// </summary>
		/// <param name="strings">string collection</param>
		/// <param name="joiner">joiner string</param>
		/// <returns>a string</returns>
		/// <remarks>rewritten using linq RK 2010/05/25
		/// <para/>See IenumberableExtensions (Join). This could be ditched.
		/// </remarks>
		public static string Join(this IEnumerable<string> strings)
		{
			if (strings.Count<string>() < 1) { return ""; }
			StringBuilder s = new StringBuilder();
			return strings.Aggregate(s,(a, b) => s.Append(b)).ToString(); //a & b zijn al strings: ToString onwenselijk.
		}

		/// <summary>
		/// Join a collection of strings using a joiner string
		/// </summary>
		/// <param name="strings">string collection</param>
		/// <param name="joiner">joiner string</param>
		/// <returns>a string</returns>
		/// <remarks>rewritten using linq RK 2010/05/25</remarks>
		public static string JoinStrings(this IEnumerable<string> strings, string joiner)
		{
			if (strings.Count<string>() < 1) { return ""; }
			StringBuilder s = new StringBuilder();
			return strings.Aggregate(s, (a, b) => s.Append(b.Insert(0,joiner))).ToString().Substring(1);
		}
	}

	[TestFixture]
	public class IEnumberableOFStringExtensionTest
	{

		[Test]
		public void testJoin()
		{
			Assert.That(() => new[] { "een", "twee", "drie" }.Join(), Is.EqualTo("eentweedrie"));
		}

		[Test]
		public void EmptyJoin()
		{
			Assert.That(() => new string[] { }.Join(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void EmptyJoinStrings()
		{
			Assert.That(() => new string[] { }.JoinStrings("!"), Is.EqualTo(string.Empty));
		}
		[Test]
		public void FastJoin()
		{
			var bigJoin = Enumerable.Range(0, 20000).Select(i => i.ToString());

			//dan ook alleen de join timen natuurlijk (bigjoin inst is 10ms)
			Stopwatch timer = Stopwatch.StartNew();
			string bigjoined = bigJoin.Join(); //15ms
			timer.Stop();

			Assert.That(timer.ElapsedMilliseconds, Is.LessThan(50.0));
		}

		[Test]
		public void testJoinStrings()
		{
			Assert.That(() => new[] { "een", "twee", "drie" }.JoinStrings("!"), Is.EqualTo("een!twee!drie"));
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
