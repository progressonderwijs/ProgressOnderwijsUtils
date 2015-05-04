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

            public Impl(T head, Impl tail)
            {
                Head = head;
                Tail = tail;
            }
        }

        SList(Impl list) { this.list = list; }

        public SList(T head, SList<T> tail)
            : this(new Impl(head, tail.list)) { }

        readonly Impl list;
        public static SList<T> Empty { get { return default(SList<T>); } }
        public bool IsEmpty { get { return list == null; } }
        public T Head { get { return list.Head; } }
        public SList<T> Tail { get { return new SList<T>(list.Tail); } }
        static readonly int typeHash = typeof(T).GetHashCode();
        static readonly IEqualityComparer<T> elemEquality = EqualityComparer<T>.Default;

        public bool Equals(SList<T> other)
        {
            var alist = list;
            var blist = other.list;
            while (alist != null && blist != null) {
                if (ReferenceEquals(alist, blist)) {
                    return true;
                }
                if (!elemEquality.Equals(alist.Head, blist.Head)) {
                    return false;
                }
                alist = alist.Tail;
                blist = blist.Tail;
            }
            return alist == null && blist == null;
        }

        public override bool Equals(object obj) { return obj is SList<T> && Equals((SList<T>)obj); }

        public override int GetHashCode()
        {
            var hash = (ulong)(typeHash + 1);
            for (var current = list; current != null; current = current.Tail) {
                hash = hash * 137ul + (ulong)elemEquality.GetHashCode(current.Head);
            }
            return (int)hash ^ (int)(hash >> 32);
        }

        public static bool operator ==(SList<T> a, SList<T> b) { return a.Equals(b); }
        public static bool operator !=(SList<T> a, SList<T> b) { return !a.Equals(b); }

        public IEnumerable<SList<T>> NonEmpySuffixes
        {
            get
            {
                for (var current = this; !current.IsEmpty; current = current.Tail) {
                    yield return current;
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (var current = list; current != null; current = current.Tail) {
                yield return current.Head;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable<T>)this).GetEnumerator(); }
    }

    public static class SList
    {
        [Pure]
        public static SList<T> Prepend<T>(this SList<T> self, T head) { return new SList<T>(head, self); }

        [Pure]
        public static SList<T> Prepend<T>(this SList<T> self, SList<T> heads)
        {
            var retval = self;
            for (var cur = heads.Reverse(); !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(cur.Head);
            }
            return retval;
        }

        public static SList<T> PrependReversed<T>(this SList<T> self, IEnumerable<T> items)
        {
            var retval = self;
            foreach (var item in items) {
                retval = retval.Prepend(item);
            }
            return retval;
        }

        [Pure]
        public static SList<T> Reverse<T>(this SList<T> self)
        {
            var retval = SList<T>.Empty;
            for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(cur.Head);
            }
            return retval;
        }

        [Pure]
        public static SList<TR> SelectEager<T, TR>(this SList<T> self, Func<T, TR> map) { return self.SelectReverse(map).Reverse(); }

        [Pure]
        public static SList<TR> SelectReverse<T, TR>(this SList<T> self, Func<T, TR> map)
        {
            var retval = SList<TR>.Empty;
            for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(map(cur.Head));
            }
            return retval;
        }

        [Pure]
        public static SList<T> WhereReverse<T>(this SList<T> self, Func<T, bool> filter)
        {
            var retval = SList<T>.Empty;
            for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
                if (filter(cur.Head)) {
                    retval = retval.Prepend(cur.Head);
                }
            }
            return retval;
        }

        [Pure]
        public static SList<T> Skip<T>(this SList<T> self, int count)
        {
            var retval = self;
            for (var i = 0; !retval.IsEmpty && i < count; i++) {
                retval = retval.Tail;
            }
            return retval;
        }

        public static SList<T> Create<T>(IEnumerable<T> list)
        {
            if (list is IList<T>) {
                return Create((IList<T>)list); //use IList interface for reverse iterability
            } else {
                return Create(list.ToArray()); //can't help but iterate forwards, so at least stick to it with the fastest possible path.
            }
        }

        public static SList<T> Create<T>(IList<T> list)
        {
            if (list is T[]) {
                return Create((T[])list);
            }
            var retval = SList<T>.Empty;
            for (var i = list.Count - 1; i >= 0; i--) {
                retval = retval.Prepend(list[i]);
            }
            return retval;
        }

        public static SList<T> Create<T>(T[] list)
        {
            var retval = SList<T>.Empty;
            for (var i = list.Length - 1; i >= 0; i--) {
                retval = retval.Prepend(list[i]);
            }
            return retval;
        }

        public static SList<T> SingleElement<T>(T element) { return SList<T>.Empty.Prepend(element); }
        public static SList<T> Empty<T>() { return SList<T>.Empty; }
    }
}
