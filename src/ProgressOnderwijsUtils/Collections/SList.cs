using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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

        SList(Impl list)
        {
            this.list = list;
        }

        public SList(T head, SList<T> tail)
            : this(new Impl(head, tail.list)) { }

        readonly Impl list;
        public static SList<T> Empty => default(SList<T>);
        public bool IsEmpty => list == null;
        public T Head => list.Head;
        public SList<T> Tail => new SList<T>(list.Tail);
        static readonly int typeHash = typeof(T).GetHashCode();
        static readonly IEqualityComparer<T> elemEquality = EqualityComparer<T>.Default;

        [Pure]
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

        [Pure]
        public override bool Equals(object obj) => obj is SList<T> && Equals((SList<T>)obj);

        [Pure]
        public override int GetHashCode()
        {
            var hash = (ulong)(typeHash + 1);
            for (var current = list; current != null; current = current.Tail) {
                hash = hash * 137ul + (ulong)elemEquality.GetHashCode(current.Head);
            }
            return (int)hash ^ (int)(hash >> 32);
        }

        [Pure]
        public static bool operator ==(SList<T> a, SList<T> b)
        {
            return a.Equals(b);
        }

        [Pure]
        public static bool operator !=(SList<T> a, SList<T> b)
        {
            return !a.Equals(b);
        }

        public IEnumerable<SList<T>> NonEmptySuffixes
        {
            get {
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    }

    public static class SList
    {
        [Pure]
        public static SList<T> Prepend<T>(this SList<T> self, T head)
        {
            return new SList<T>(head, self);
        }

        [Pure]
        [UsefulToKeep("library method")]
        public static SList<T> Prepend<T>(this SList<T> self, SList<T> heads)
        {
            var retval = self;
            for (var cur = heads.Reverse(); !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(cur.Head);
            }
            return retval;
        }

        [Pure]
        public static SList<T> PrependReversed<T>(this SList<T> self, [NotNull] IEnumerable<T> items)
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
            var retval = default(SList<T>);
            for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(cur.Head);
            }
            return retval;
        }

        [Pure]
        public static SList<TR> SelectEager<T, TR>(this SList<T> self, Func<T, TR> map)
        {
            return self.SelectReverse(map).Reverse();
        }

        [Pure]
        public static SList<TR> SelectReverse<T, TR>(this SList<T> self, Func<T, TR> map)
        {
            var retval = default(SList<TR>);
            for (var cur = self; !cur.IsEmpty; cur = cur.Tail) {
                retval = retval.Prepend(map(cur.Head));
            }
            return retval;
        }

        [Pure]
        [UsefulToKeep("library method")]
        public static SList<T> WhereReverse<T>(this SList<T> self, Func<T, bool> filter)
        {
            var retval = default(SList<T>);
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

        [Pure]
        public static SList<T> Create<T>([NotNull] IEnumerable<T> list)
        {
            if (list is IList<T>) {
                return Create((IList<T>)list); //use IList interface for reverse iterability
            } else {
                return Create(list.ToArray()); //can't help but iterate forwards, so at least stick to it with the fastest possible path.
            }
        }

        [Pure]
        public static SList<T> Create<T>([NotNull] IList<T> list)
        {
            if (list is T[]) {
                return Create((T[])list);
            }
            var retval = default(SList<T>);
            for (var i = list.Count - 1; i >= 0; i--) {
                retval = retval.Prepend(list[i]);
            }
            return retval;
        }

        [Pure]
        public static SList<T> Create<T>([NotNull] T[] list)
        {
            var retval = default(SList<T>);
            for (var i = list.Length - 1; i >= 0; i--) {
                retval = retval.Prepend(list[i]);
            }
            return retval;
        }

        [Pure]
        public static SList<T> SingleElement<T>(T element) => new SList<T>(element, default(SList<T>));

        [UsefulToKeep("library method")]
        public static SList<T> Empty<T>() => default(SList<T>);
    }
}
