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
			//if (strings.Count<string>() < 1) { return ""; } //dit is veel trager - dit is een hele extra enumeration!
			return strings.Aggregate(new StringBuilder(), (builder, str) => builder.Append(str)).ToString();

			//Let wel, het is maar de vraag of dit 't meest leesbare alternatief is, andere mogelijkheden zijn (ruwweg even snel)
			
			//StringBuilder sb = new StringBuilder();
			//foreach (var str in strings) sb.Append(str);
			//return sb.ToString();

			//return string.Join("", strings.ToArray());
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
			var ints = Enumerable.Range(0, 20000).Select(i => i.ToString()).ToArray();
			//dan ook alleen de join timen natuurlijk (bigjoin inst is 10ms)
			//- om dit te doen moet je  .ToArray moeten gebruiken om em te instantieren.
			var time = BenchTimer.MinimumTime(() => ints.Join());

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
