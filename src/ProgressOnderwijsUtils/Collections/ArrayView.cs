using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtils.Collections
{
	//represents a read-only view of an array or array-like  datastructure (known size, random access)

	public interface IArrayView<out T> : IEnumerable<T>
	{
		T this[int index] { get; }
		int Count { get; }
	}



	public sealed class ArrayView<T> : IArrayView<T>
	{
		readonly T[] vals;
		public ArrayView(T[] vals) { this.vals = vals; }
		public IEnumerator<T> GetEnumerator() { return vals.AsEnumerable().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return vals.GetEnumerator(); }
		public T this[int index] { get { return vals[index]; } }

		public int Count { get { return vals.Length; } }
	}

	public sealed class ListView<T> : IArrayView<T>
	{
		readonly IList<T> vals;
		public ListView(IList<T> vals) { this.vals = vals; }
		public IEnumerator<T> GetEnumerator() { return vals.AsEnumerable().GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return vals.GetEnumerator(); }
		public T this[int index] { get { return vals[index]; } }

		public int Count { get { return vals.Count; } }
	}

	public sealed class ArrayView_MappedByElement<T, TOut> : IArrayView<TOut>
	{
		readonly IArrayView<T> vals;
		readonly Func<T, TOut> map;
		public ArrayView_MappedByElement(IArrayView<T> vals, Func<T, TOut> map) { this.vals = vals; this.map = map; }

		public IEnumerator<TOut> GetEnumerator() { return vals.AsEnumerable().Select(map).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(vals[index]); } }

		public int Count { get { return vals.Count; } }
	}

	public sealed class ArrayView_MappedWithIndex<T, TOut> : IArrayView<TOut>
	{
		readonly IArrayView<T> vals;
		readonly Func<T, int, TOut> map;
		public ArrayView_MappedWithIndex(IArrayView<T> vals, Func<T, int, TOut> map) { this.vals = vals; this.map = map; }

		public IEnumerator<TOut> GetEnumerator() { return vals.AsEnumerable().Select(map).GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(vals[index], index); } }

		public int Count { get { return vals.Count; } }
	}

	public static class ArrayViewExtensions
	{
		public static IArrayView<T> AsReadView<T>(this T[] vals) { return new ArrayView<T>(vals); }
		public static IArrayView<TOut> Select<T, TOut>(this IArrayView<T> vals, Func<T, TOut> map) { return new ArrayView_MappedByElement<T, TOut>(vals, map); }
		public static IArrayView<TOut> Select<T, TOut>(this IArrayView<T> vals, Func<T, int, TOut> map) { return new ArrayView_MappedWithIndex<T, TOut>(vals, map); }
	}
}