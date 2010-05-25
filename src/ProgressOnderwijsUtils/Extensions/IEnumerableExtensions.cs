using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class IEnumerableExtensions
	{
		/*
		public static int IndexOf<T>(this IEnumerable<T> list, T elem)
		{
			if (list == null) throw new ArgumentNullException("list");
			if (elem == null) throw new ArgumentNullException("elem");
			int retval=0;
			foreach (T item in list)
			{
				if (elem.Equals(item))
					return retval;
				retval++;
			}
			return -1;
		}*/

		/// <summary>
		/// returns the (first) index of an element in a collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <name="elem">the element searched</param>
		/// <returns>an int</returns>
		/// <codefrom value="Renzo Kooi" date="2010/05/25" dateLast="2010/05/25"/>
		/// <remarks>If you just want to test existance the native "Contains" would be sufficient</remarks>
		public static int IndexOf<T>(this IEnumerable<T> list, T elem)
		{
			int i = -1;
			return list.Aggregate(i, (a, b) => { a = b.Equals(elem) && a < 1 ? i+1 : a; i++; return a; });
		}

		/// <summary>
		/// Joins elements in a collection, using a joiner string
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src">any collection</param>
		/// <param name="joiner">string between elems</param>
		/// <returns>a string</returns>
		/// <codefrom value="Renzo Kooi" date="2010/05/25" dateLast="2010/05/25"/>
		public static string Join<T>(this IEnumerable<T> src, string joiner)
		{
			return src.Aggregate("",(a, b) => a += a.Length < 1 ? b.ToString() : joiner + b.ToString());
		}
	}

	[TestFixture]
	public class IEnumberableExtensionTest
	{
		[Test]
		public void testIndexOf()
		{
			List<string> lst = new List<string> { "een", "twee", "drie" };
			int[] ints = { 1, 2, 3, 4, 5 };
			Assert.That(lst.IndexOf("twee"), Is.EqualTo(1));
			Assert.That(lst.IndexOf("tweeeneenhalf"), Is.EqualTo(-1));
		}

		[Test]
		public void testJoin()
		{
			List<int> lst = new List<int>{1,2,3,4,5};
			List<string> strlst = new List<string> { "een", "twee", "drie" };
			Assert.That(lst.Join("!"), Is.EqualTo("1!2!3!4!5"));
			Assert.That(strlst.Join(" plus "), Is.EqualTo("een plus twee plus drie"));
		}
	}
}
