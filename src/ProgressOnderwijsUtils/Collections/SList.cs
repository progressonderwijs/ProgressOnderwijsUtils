using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.Collections
{

	/// <summary>
	/// Immutable, thread-safe, singly-linked list
	/// </summary>
	public struct SList<T> : IEquatable<SList<T>>, IEnumerable<T> where T : IEquatable<T>
	{
		static readonly int typeHash = typeof(T).GetHashCode();
		static readonly IEqualityComparer<T> elemEquality = EqualityComparer<T>.Default;

		sealed class Impl
		{
			public readonly T Head;
			public readonly Impl Tail;
			public Impl(T head, Impl tail) { Head = head; Tail = tail; }
		}

		SList(Impl list) { this.list = list; }
		readonly Impl list;
		public static SList<T> Empty { get { return default(SList<T>); } }

		public bool IsEmpty { get { return list == null; } }
		public SList<T> Prepend(T head) { return new SList<T>(new Impl(head, list)); }
		public SList<T> Reverse()
		{
			var retval = Empty;
			for (var cur = this; !cur.IsEmpty; cur = cur.Tail)
				retval = retval.Prepend(cur.Head);
			return retval;
		}

		public T Head { get { return list.Head; } }
		public SList<T> Tail { get { return new SList<T>(list.Tail); } }

		public bool Equals(SList<T> other)
		{
			var alist = list;
			var blist = other.list;
			while (alist != null && blist != null)
			{
				if (ReferenceEquals(alist, blist)) return true;
				if (!elemEquality.Equals(alist.Head, blist.Head)) return false;
				alist = alist.Tail;
				blist = blist.Tail;
			}
			return alist == null && blist == null;
		}
		public override bool Equals(object obj) { return obj is SList<T> && Equals((SList<T>)obj); }
		public override int GetHashCode()
		{
			ulong hash = (ulong)(typeHash + 1);
			for (var current = list; current != null; current = current.Tail)
				hash = hash * 137ul + (ulong)elemEquality.GetHashCode(current.Head);
			return (int)hash ^ (int)(hash >> 32);
		}
		public static bool operator ==(SList<T> a, SList<T> b) { return a.Equals(b); }
		public static bool operator !=(SList<T> a, SList<T> b) { return !a.Equals(b); }

		public IEnumerable<SList<T>> NonEmpySuffixes
		{
			get
			{
				for (var current = this; !current.IsEmpty; current = current.Tail)
					yield return current;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			for (var current = list; current != null; current = current.Tail)
				yield return current.Head;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable<T>)this).GetEnumerator(); }
	}

	public static class SList
	{
		public static SList<T> Create<T>(IEnumerable<T> list) where T : IEquatable<T>
		{
			if (list is IList<T>) return Create((IList<T>)list);//use IList interface for reverse iterability
			else return Create(list.ToArray());//can't help but iterate forwards, so at least stick to it with the fastest possible path.
		}
		public static SList<T> Create<T>(IList<T> list) where T : IEquatable<T>
		{
			if (list is T[]) return Create((T[])list);
			var retval = SList<T>.Empty;
			for (int i = list.Count - 1; i >= 0; i--)
				retval = retval.Prepend(list[i]);
			return retval;
		}
		public static SList<T> Create<T>(T[] list) where T : IEquatable<T>
		{
			var retval = SList<T>.Empty;
			for (int i = list.Length - 1; i >= 0; i--)
				retval = retval.Prepend(list[i]);
			return retval;
		}
	}
}
