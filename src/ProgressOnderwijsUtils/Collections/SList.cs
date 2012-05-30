
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Collections
{

	/// <summary>
	/// Immutable, thread-safe, singly-linked list
	/// </summary>
	public struct SList<T> : IEquatable<SList<T>>, IEnumerable<T>
	{
		sealed class Impl
		{
			public readonly T Head;
			public readonly Impl Tail;
			public Impl(T head, Impl tail) { this.Head = head; this.Tail = tail; }
		}

		SList(Impl list) { this.list = list; }
		readonly Impl list;
		public static SList<T> Empty { get { return default(SList<T>); } }

		public bool IsEmpty { get { return list == null; } }
		public SList<T> Prepend(T head) { return new SList<T>(new Impl(head, list)); }
		public SList<T> Reverse() { return this.Aggregate(Empty, (current, item) => current.Prepend(item)); }

		public T Head { get { return list.Head; } }
		public SList<T> Tail { get { return new SList<T>(list.Tail); } }

		public bool Equals(SList<T> other) { return this.SequenceEqual(other); }

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			for (var current = list; current != null; current = current.Tail)
				yield return current.Head;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable<T>)this).GetEnumerator(); }
	}

	public static class SList
	{
		public static SList<T> Create<T>(IEnumerable<T> mutable) { return mutable.Aggregate(SList<T>.Empty, (current, item) => current.Prepend(item)).Reverse(); }
	}
}
