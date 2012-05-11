using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class EnumerableExtensions
	{
		public static string ToStringFlattened<T>(this IEnumerable<T> self)
		{
			StringBuilder sb = new StringBuilder();
			foreach (T item in self.EmptyIfNull())
			{
				if (sb.Length > 0) sb.Append(", ");
				sb.Append(item);
			}
			return string.Format("[{0}]", sb);
		}
	}

	[TestFixture]
	public class EnumerableExtensionsTest
	{
		private IEnumerable<TestCaseData> ToStringFlattenedData()
		{
			yield return new TestCaseData(null, "[]");		
			yield return new TestCaseData(new string[0], "[]");		
			yield return new TestCaseData(new[] { "single" }, "[single]");		
			yield return new TestCaseData(new[] { "first", "second" }, "[first, second]");		
		}
		
		[Test, TestCaseSource("ToStringFlattenedData")]
		public void ToStringFlattened(IEnumerable<string> sut, string expected)
		{
			Assert.That(sut.ToStringFlattened(), Is.EqualTo(expected));	
		}
	}
}
