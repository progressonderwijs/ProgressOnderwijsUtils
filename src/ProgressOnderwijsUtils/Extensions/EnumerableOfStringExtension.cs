using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

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
			return strings.Aggregate((a, b) => a + b.ToString());
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
			return strings.Aggregate((a, b) => a += a.Length < 1 ? b.ToString() : joiner + b.ToString());
		}

	}

	[TestFixture]
	public class IEnumberableOFStringExtensionTest
	{
		
		[Test]
		public void testJoin()
		{
			List<string> lst = new List<string> { "een", "twee", "drie" };
			Assert.That(lst.Join(), Is.EqualTo("eentweedrie"));
		}

		[Test]
		public void testJoinStrings() {
			List<string> lst = new List<string> { "een", "twee", "drie" };
			Assert.That(lst.JoinStrings("!"), Is.EqualTo("een!twee!drie"));
		}
	}
}
