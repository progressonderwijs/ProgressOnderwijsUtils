using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public sealed class ArrayView<T> : IReadOnlyList<T>
	{
		readonly T[] vals;
		public ArrayView(T[] vals) { this.vals = vals; }
		public IEnumerator<T> GetEnumerator() { foreach (var item in vals) yield return item; }
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

	public sealed class ReadOnlyCollectionView_Mapped<T, TOut> : IReadOnlyCollection<TOut>
	{
		readonly IReadOnlyCollection<T> source;
		readonly Func<T, TOut> map;
		public ReadOnlyCollectionView_Mapped(IReadOnlyCollection<T> source, Func<T, TOut> map) { this.source = source; this.map = map; }
		public int Count { get { return source.Count; } }
		public IEnumerator<TOut> GetEnumerator() { foreach (var item in source) yield return map(item); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public sealed class CollectionView_Mapped<T, TOut> : IReadOnlyCollection<TOut>
	{
		readonly ICollection<T> source;
		readonly Func<T, TOut> map;
		public CollectionView_Mapped(ICollection<T> source, Func<T, TOut> map) { this.source = source; this.map = map; }
		public int Count { get { return source.Count; } }
		public IEnumerator<TOut> GetEnumerator() { foreach (var item in source) yield return map(item); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}

	public sealed class ArrayView_MappedByElement<T, TOut> : IReadOnlyList<TOut>
	{
		readonly IReadOnlyList<T> source;
		readonly Func<T, TOut> map;
		public ArrayView_MappedByElement(IReadOnlyList<T> source, Func<T, TOut> map) { this.source = source; this.map = map; }
		public IEnumerator<TOut> GetEnumerator() { foreach (var item in source) yield return map(item); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(source[index]); } }
		public int Count { get { return source.Count; } }
	}

	public sealed class ArrayView_MappedWithIndex<T, TOut> : IReadOnlyList<TOut>
	{
		readonly IReadOnlyList<T> source;
		readonly Func<T, int, TOut> map;
		public ArrayView_MappedWithIndex(IReadOnlyList<T> source, Func<T, int, TOut> map) { this.source = source; this.map = map; }
		public IEnumerator<TOut> GetEnumerator() { int i = 0; foreach (var item in source) yield return map(item, i++); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public TOut this[int index] { get { return map(source[index], index); } }
		public int Count { get { return source.Count; } }
	}

	public static class CollectionViewExtensions
	{
		public static IReadOnlyList<T> AsReadView<T>(this T[] vals) { return new ArrayView<T>(vals); }
		public static IReadOnlyList<T> AsReadOnlyView<T>(this IList<T> list) { return list as IReadOnlyList<T> ?? new ListView<T>(list); }

		public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, TOut> map) { return new ArrayView_MappedByElement<T, TOut>(vals, map); }
		public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, int, TOut> map) { return new ArrayView_MappedWithIndex<T, TOut>(vals, map); }
		public static IReadOnlyCollection<TOut> SelectROCountable<T, TOut>(this IReadOnlyCollection<T> vals, Func<T, TOut> map) { return new ReadOnlyCollectionView_Mapped<T, TOut>(vals, map); }
		public static IReadOnlyCollection<TOut> SelectCountable<T, TOut>(this ICollection<T> vals, Func<T, TOut> map) { return new CollectionView_Mapped<T, TOut>(vals, map); }
	}
}