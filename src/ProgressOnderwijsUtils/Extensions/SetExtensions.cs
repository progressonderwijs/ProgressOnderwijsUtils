using System;
using System.Collections.Generic;
using System.Linq;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Extensions
{
	public static class SetExtensions
	{
		public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
		{
			return new ReadOnlySet<T>(set);
		}

		public static HashSet<T> CopyAndAddItems<T>(this IEnumerable<T> H, IEnumerable<T> toAdd)
		{
			HashSet<T> newHashSet = new HashSet<T>(H);
			foreach (T s in toAdd) 
				newHashSet.Add(s);
			return newHashSet;
		}

		public static HashSet<T> CopyAndRemoveItems<T>(this IEnumerable<T> H, IEnumerable<T> toRemove)
		{
			HashSet<T> newHashSet = new HashSet<T>(H);
			foreach (T s in toRemove)
				newHashSet.Remove(s);
			return newHashSet;
		}

		public static HashSet<T> CopyAndRemoveItem<T>(this IEnumerable<T> H, T toRemove)
		{
			HashSet<T> newHashSet = new HashSet<T>(H);
			newHashSet.Remove(toRemove);
			return newHashSet;
		}

		public static HashSet<T> CopyAndAddItem<T>(this IEnumerable<T> H, T toAdd)
		{
			HashSet<T> newHashSet = new HashSet<T>(H) { toAdd };
			return newHashSet;
		}

	}
}
