using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Collections
{
	public sealed class ArrayView<T> : IReadOnlyList<T>
	{
		readonly T[] vals;
		public ArrayView(T[] vals) { this.vals = vals; }
		public IEnumerator<T> GetEnumerator() { return vals.AsEnumerable().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return vals.GetEnumerator(); }
		public T this[int index] { get { return vals[index]; } }

		public int Count { get { return vals.Length; } }
	}

	public sealed class ListView<T> : IReadOnlyList<T>
	{
		readonly IList<T> vals;
		public ListView(IList<T> vals) { this.vals = vals; }
		public IEnumerator<T> GetEnumerator() { return vals.AsEnumerable().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return vals.GetEnumerator(); }
		public T this[int index] { get { return vals[index]; } }

		public int Count { get { return vals.Count; } }
	}

	public sealed class ArrayView_MappedByElement<T, TOut> : IReadOnlyList<TOut>
	{
		readonly IReadOnlyList<T> vals;
		readonly Func<T, TOut> map;
		public ArrayView_MappedByElement(IReadOnlyList<T> vals, Func<T, TOut> map) { this.vals = vals; this.map = map; }

		public IEnumerator<TOut> GetEnumerator() { return vals.AsEnumerable().Select(map).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(vals[index]); } }

		public int Count { get { return vals.Count; } }
	}

	public sealed class ArrayView_MappedWithIndex<T, TOut> : IReadOnlyList<TOut>
	{
		readonly IReadOnlyList<T> vals;
		readonly Func<T, int, TOut> map;
		public ArrayView_MappedWithIndex(IReadOnlyList<T> vals, Func<T, int, TOut> map) { this.vals = vals; this.map = map; }

		public IEnumerator<TOut> GetEnumerator() { return vals.AsEnumerable().Select(map).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(vals[index], index); } }

		public int Count { get { return vals.Count; } }
	}

	public static class ArrayViewExtensions
	{
		public static IReadOnlyList<T> AsReadView<T>(this T[] vals) { return new ArrayView<T>(vals); }
		public static IReadOnlyList<TOut> Select<T, TOut>(this IReadOnlyList<T> vals, Func<T, TOut> map) { return new ArrayView_MappedByElement<T, TOut>(vals, map); }
		public static IReadOnlyList<TOut> Select<T, TOut>(this IReadOnlyList<T> vals, Func<T, int, TOut> map) { return new ArrayView_MappedWithIndex<T, TOut>(vals, map); }
		public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, TOut> map) { return new ArrayView_MappedByElement<T, TOut>(vals, map); }
		public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, int, TOut> map) { return new ArrayView_MappedWithIndex<T, TOut>(vals, map); }
	}
}