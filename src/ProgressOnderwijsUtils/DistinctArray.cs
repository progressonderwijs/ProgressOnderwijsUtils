using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class DistinctArray
    {
        public static DistinctArray<T> FromDistinct<T>(IEnumerable<T> items)
            => DistinctArray<T>.FromDistinct(items);
    }

    [Serializable]
    public struct DistinctArray<T> : IReadOnlyList<T>
    {
        public static DistinctArray<T> Empty
            => FromDistinct(Array.Empty<T>());

        public static DistinctArray<T> FromDistinct([NotNull] IEnumerable<T> items)
            => FromDistinct(items, EqualityComparer<T>.Default);

        public static DistinctArray<T> FromDistinct([NotNull] IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items.ContainsDuplicates(comparer)) {
                throw new ArgumentException("items are not distinct");
            }

            return new DistinctArray<T> {
                items = items.ToArray()
            };
        }

        public static DistinctArray<T> FromPossiblyNotDistinct([NotNull] IEnumerable<T> items)
            => FromPossiblyNotDistinct(items, EqualityComparer<T>.Default);

        public static DistinctArray<T> FromPossiblyNotDistinct([NotNull] IEnumerable<T> items, IEqualityComparer<T> comparer)
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
