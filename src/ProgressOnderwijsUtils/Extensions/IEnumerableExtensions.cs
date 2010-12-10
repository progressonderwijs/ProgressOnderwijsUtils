using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.ObjectModel;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils
{
	// <summary>
	/// returns the (first) index of an element in a collection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <name="elem">the element searched</param>
	/// <returns>an int</returns>
	/// <remarks>If you just want to test existance the native "Contains" would be sufficient</remarks>
	public static class IEnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> list, T elem)
		{
			if (list == null) throw new ArgumentNullException("list");
			int retval = 0;
			foreach (T item in list)
			{
				if (Equals(elem, item))
					return retval;
				retval++;
			}
			return -1;
		}
		public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> matcher)
		{
			if (list == null) throw new ArgumentNullException("list");
			if (matcher == null) throw new ArgumentNullException("matcher");
			int retval = 0;
			foreach (T item in list)
			{
				if (matcher(item))
					return retval;
				retval++;
			}
			return -1;
		}
		public static ReadOnlyCollection<T> AsReadOnlyCopy<T>(this IEnumerable<T> list) { return new ReadOnlyCollection<T>(list.ToArray()); }
		public static ReadOnlyCollection<T> AsReadOnlyView<T>(this IList<T> list) { return list as ReadOnlyCollection<T> ?? new ReadOnlyCollection<T>(list); }
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list) { return list ?? Enumerable.Empty<T>(); }
	}

	[TestFixture]
	public class IEnumerableExtensionTest
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
		public void testFirstIndexOfDups()
		{
			Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(0), Is.EqualTo(0));
			Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(1), Is.EqualTo(2));
			Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(2), Is.EqualTo(4));
		}

		[Test]
		public void EmptyIfNullOk()
		{
			PAssert.That(() => new[] { 0, 1, 2, }.EmptyIfNull().SequenceEqual(new[] { 0, 1, 2, }));
			PAssert.That(() => default(int[]).EmptyIfNull().SequenceEqual(new int[] { }));
			PAssert.That(() => default(int[]) == null);
			PAssert.That(() => default(int[]) != default(int[]).EmptyIfNull());
			var arr = new[] { 0, 1, 2, };
			PAssert.That(() => arr.EmptyIfNull() == arr);
		}
	}
}
