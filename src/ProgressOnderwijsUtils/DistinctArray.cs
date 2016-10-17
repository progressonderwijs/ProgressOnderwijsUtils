using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public struct DistinctArray<T> : IReadOnlyList<T>
    {
        public static DistinctArray<T> Empty
            => FromDistinct(Array.Empty<T>());

        public static DistinctArray<T> FromDistinct(IEnumerable<T> items)
            => FromDistinct(items, EqualityComparer<T>.Default);

        public static DistinctArray<T> FromDistinct(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items.Distinct(comparer).Count() != items.Count()) {
                throw new ArgumentException("items are not distinct");
            }

            return new DistinctArray<T> {
                items = items.ToArray()
            };
        }

        public static DistinctArray<T> FromPossiblyNotDistinct(IEnumerable<T> items)
            => FromPossiblyNotDistinct(items, EqualityComparer<T>.Default);

        public static DistinctArray<T> FromPossiblyNotDistinct(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            return new DistinctArray<T> {
                items = items.Distinct(comparer).ToArray()
            };
        }

        T[] items;
        public int Count => items.Length;
        public T this[int index] => items[index];
        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();
    }
}
