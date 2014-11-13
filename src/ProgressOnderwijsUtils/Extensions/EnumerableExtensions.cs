using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

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
			if(list == null) throw new ArgumentNullException("list");
			int retval = 0;
			foreach(T item in list)
			{
				if(Equals(elem, item))
					return retval;
				retval++;
			}
			return -1;
		}

		public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> matcher)
		{
			if(list == null) throw new ArgumentNullException("list");
			if(matcher == null) throw new ArgumentNullException("matcher");
			int retval = 0;
			foreach(var item in list)
			{
				if(matcher(item))
					return retval;
				retval++;
			}
			return -1;
		}

		public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
		{
			return condition ? source.Where(predicate) : source;
		}

		[Pure]
		public static IReadOnlyList<T> ToReadOnly<T>(this IEnumerable<T> list) { return list.ToArray(); }
		[Pure]
		public static HashSet<T> ToSet<T>(this IEnumerable<T> list) { return new HashSet<T>(list); }
		[Pure]
		public static HashSet<T> ToSet<T>(this IEnumerable<T> list, IEqualityComparer<T> comparer) { return new HashSet<T>(list, comparer); }

		[Pure]
		public static bool SetEqual<T>(this IEnumerable<T> list, IEnumerable<T> other) { return list.ToSet().SetEquals(other); }
		[Pure]
		public static bool SetEqual<T>(this IEnumerable<T> list, IEnumerable<T> other, IEqualityComparer<T> comparer) { return list.ToSet(comparer).SetEquals(other); }


		[Pure]
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> list) { return list ?? Enumerable.Empty<T>(); }

		public static int GetSequenceHashCode<T>(IEnumerable<T> list, IEqualityComparer<T> elementComparer = null)
		{
			var elemEquality = elementComparer ?? EqualityComparer<T>.Default;
			ulong hash = 3;
			foreach(var item in list)
				hash = hash * 137ul + (ulong)elemEquality.GetHashCode(item);
			return (int)hash ^ (int)(hash >> 32);
		}

		public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
		{
			var set = new HashSet<T>();
			return !list.All(set.Add);
		}


		public static SortedList<TKey, TVal> ToSortedList<T, TKey, TVal>(this IEnumerable<T> list, Func<T, TKey> keySelector, Func<T, TVal> valSelector)
		{
			return list.ToSortedList(keySelector, valSelector, Comparer<TKey>.Default);
		}

		public static SortedList<TKey, TVal> ToSortedList<T, TKey, TVal>(this IEnumerable<T> list, Func<T, TKey> keySelector, Func<T, TVal> valSelector, IComparer<TKey> keyComparer)
		{
			var retval = new SortedList<TKey, TVal>(keyComparer);
			foreach(var item in list.OrderBy(keySelector, keyComparer))
				retval.Add(keySelector(item), valSelector(item));
			return retval;
		}

		public static Dictionary<TKey, TValue> ToGroupedDictionary<TElem, TKey, TValue>(this IEnumerable<TElem> list, Func<TElem, TKey> keyLookup,
			Func<TKey, IEnumerable<TElem>, TValue> groupMap
			)
		{
			var groups = new Dictionary<TKey, List<TElem>>();
			foreach(var elem in list)
			{
				var key = keyLookup(elem);
				List<TElem> group;
				if(!groups.TryGetValue(key, out group))
					groups.Add(key, group = new List<TElem>());
				group.Add(elem);
			}
			var retval = new Dictionary<TKey, TValue>(groups.Count);
			foreach(var group in groups)
				retval.Add(group.Key, groupMap(group.Key, group.Value));
			return retval;
		}

		public static string ToCsv<T>(this IEnumerable<T> items, bool useHeader = true, string delimiter = "\t", bool useQuotesForStrings = false)
            where T : class
        {
            var csvBuilder = new StringBuilder();
            var properties = typeof(T).GetProperties();

            if (useHeader)
	            csvBuilder.AppendLine(string.Join(delimiter, properties.Select(p => p.Name.ToCsvValue(delimiter, useQuotesForStrings)).ToArray()));
            foreach (var item in items)
            {
                var line = string.Join(delimiter, properties.Select(p => p.GetValue(item, null).ToCsvValue(delimiter, useQuotesForStrings)).ToArray());
                csvBuilder.AppendLine(line);
            }
            return csvBuilder.ToString();
        }
 
        private static string ToCsvValue<T>(this T item, string delimiter, bool useQuotesForStrings)
        {
			if (string.Format("{0}", item).Contains(delimiter) && !useQuotesForStrings) throw new ArgumentException("item contains illegal characters, use useQuotesForStrings=true");

			if (!useQuotesForStrings)
				return string.Format("{0}", item);

            if(item == null) return "\"\"";
 
            if (item is string)
            {
                return string.Format("\"{0}\"", item.ToString().Replace("\"", "\\\""));
            }
			else
			{ 
				return string.Format("{0}", item);
			}
		}
	}
}
