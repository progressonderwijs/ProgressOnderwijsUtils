using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public struct DistinctArray<T> : IReadOnlyList<T>
    {
        public static DistinctArray<T> Empty
            => new DistinctArray<T>(Array.Empty<T>());

        readonly T[] items;

        public DistinctArray(IEnumerable<T> items)
            : this(items, EqualityComparer<T>.Default) { }

        public DistinctArray(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            this.items = items.Distinct(comparer).ToArray();
        }

        public int Count => items.Length;
        public T this[int index] => items[index];
        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();
    }
}
