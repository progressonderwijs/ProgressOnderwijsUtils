using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public static class IEnumerableExtensions
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

		public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
		{
			var set = new HashSet<T>();
			return !list.All(set.Add);
		}
	}
}
