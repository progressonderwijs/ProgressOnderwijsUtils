using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ProgressOnderwijsUtils.Collections
{

	/// <summary>
	/// Immutable, thread-safe, singly-linked list
	/// </summary>
	public struct SList<T> : IEnumerable<T>, IEquatable<SList<T>>
	{

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
		[Pure]
		public SList<T> Prepend(T head) { return new SList<T>(new Impl(head, list)); }

		[Pure]
		public SList<T> Prepend(SList<T> heads)
		{
			var retval = this;
			for (var cur = heads.Reverse(); !cur.IsEmpty; cur = cur.Tail)
				retval = retval.Prepend(cur.Head);
			return retval;
		}

		[Pure]
		public SList<T> Reverse()
		{
			var retval = Empty;
			for (var cur = this; !cur.IsEmpty; cur = cur.Tail)
				retval = retval.Prepend(cur.Head);
			return retval;
		}

		[Pure]
		public SList<TR> SelectEager<TR>(Func<T, TR> map)
		{
			return SelectReverse(map).Reverse();
		}

		[Pure]
		public SList<TR> SelectReverse<TR>(Func<T, TR> map)
		{
			var retval = SList<TR>.Empty;
			for (var cur = this; !cur.IsEmpty; cur = cur.Tail)
				retval = retval.Prepend(map(cur.Head));
			return retval;
		}

		[Pure]
		public SList<T> WhereReverse(Func<T, bool> filter)
		{
			var retval = Empty;
			for (var cur = this; !cur.IsEmpty; cur = cur.Tail)
				if (filter(cur.Head))
					retval = retval.Prepend(cur.Head);
			return retval;
		}

		[Pure]
		public SList<T> Skip(int count)
		{
			var retval = this;
			for (int i = 0; !retval.IsEmpty && i < count; i++)
				retval = retval.Tail;
			return retval;
		}

		public T Head { get { return list.Head; } }
		public SList<T> Tail { get { return new SList<T>(list.Tail); } }

		static readonly int typeHash = typeof(T).GetHashCode();
		static readonly IEqualityComparer<T> elemEquality = EqualityComparer<T>.Default;
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
		public static SList<T> Create<T>(IEnumerable<T> list)
		{
			if (list is IList<T>) return Create((IList<T>)list);//use IList interface for reverse iterability
			else return Create(list.ToArray());//can't help but iterate forwards, so at least stick to it with the fastest possible path.
		}
		public static SList<T> Create<T>(IList<T> list)
		{
			if (list is T[]) return Create((T[])list);
			var retval = SList<T>.Empty;
			for (int i = list.Count - 1; i >= 0; i--)
				retval = retval.Prepend(list[i]);
			return retval;
		}
		public static SList<T> Create<T>(T[] list)
		{
			var retval = SList<T>.Empty;
			for (int i = list.Length - 1; i >= 0; i--)
				retval = retval.Prepend(list[i]);
			return retval;
		}

		public static SList<T> SingleElement<T>(T element) { return SList<T>.Empty.Prepend(element); }
		public static SList<T> Empty<T>() { return SList<T>.Empty; }
	}
}
