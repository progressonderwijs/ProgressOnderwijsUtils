﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// returns the (first) index of an element in a collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">the enumerable to search in</param>
		/// <param name="elem">the element searched</param>
		/// <returns>an int</returns>
		/// <remarks>If you just want to test existance the native "Contains" would be sufficient</remarks>
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
		public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> list) { return new ReadOnlyCollection<T>(list.ToArray()); }
		public static ReadOnlyCollection<T> AsReadOnlyView<T>(this IList<T> list) { return list as ReadOnlyCollection<T> ?? new ReadOnlyCollection<T>(list); }
		public static HashSet<T> ToSet<T>(this IEnumerable<T> list) { return new HashSet<T>(list); }
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list) { return list ?? Enumerable.Empty<T>(); }

		public static int GetSequenceHashCode<T>(this IEnumerable<T> list, IEqualityComparer<T> elementComparer = null)
		{
			var elemEquality = elementComparer ?? EqualityComparer<T>.Default;
			ulong hash = 3;
			foreach (var item in list)
				hash = hash * 137ul + (ulong)elemEquality.GetHashCode(item);
			return (int)hash ^ (int)(hash >> 32);
		}

		public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
		{
			var set = new HashSet<T>();
			return !list.All(set.Add);
		}

		public static string ToStringFlattened<T>(this IEnumerable<T> self)
		{
			return "[" +
				self
					.EmptyIfNull()
					.Select(item => item == null ? "" : item.ToString())
					.JoinStrings(", ")
				+ "]";
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
