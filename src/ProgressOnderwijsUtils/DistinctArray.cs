#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class DistinctArray
    {
        [Pure]
        public static DistinctArray<T> ToDistinctArray<T>([NotNull] this IEnumerable<T> items)
            => DistinctArray<T>.FromPossiblyNotDistinct(items, EqualityComparer<T>.Default);

        [Pure]
        public static DistinctArray<T> ToDistinctArray<T>([NotNull] this ISet<T> items)
            => ToDistinctArrayFromDistinct_Unchecked(items.ToArray());

        [Pure]
        public static DistinctArray<T> ToDistinctArray<T, TVal>([NotNull] this Dictionary<T, TVal>.KeyCollection items)
            => ToDistinctArrayFromDistinct_Unchecked(items.ToArray());

        [Pure]
        public static DistinctArray<T> ToDistinctArray<T>([NotNull] this IEnumerable<T> items, IEqualityComparer<T> comparer)
            => items is HashSet<T> set && (object)set.Comparer == (object)comparer ? set.ToDistinctArray() : DistinctArray<T>.FromPossiblyNotDistinct(items, comparer);

        [Pure]
        public static DistinctArray<T> ToDistinctArrayFromDistinct<T>([NotNull] this IEnumerable<T> items)
            => DistinctArray<T>.FromDistinctNonMutatedArray(items.ToArray(), EqualityComparer<T>.Default);

        [Pure]
        public static DistinctArray<T> ToDistinctArrayFromDistinct<T>([NotNull] this IEnumerable<T> items, IEqualityComparer<T> comparer)
            => DistinctArray<T>.FromDistinctNonMutatedArray(items.ToArray(), comparer);

        public static DistinctArray<T> ToDistinctArrayFromDistinct_Unchecked<T>([NotNull] this T[] items)
            => DistinctArray<T>.FromDistinct_ClaimDistinctnessWithoutCheck(items);
    }

    [Serializable]
    public struct DistinctArray<T> : IReadOnlyList<T>
    {
        public static DistinctArray<T> Empty
            => new DistinctArray<T>(Array.Empty<T>());

        public static DistinctArray<T> FromDistinct_ClaimDistinctnessWithoutCheck([NotNull] T[] items)
            => new DistinctArray<T>(items);

        public static DistinctArray<T> FromDistinctNonMutatedArray([NotNull] T[] items, IEqualityComparer<T> comparer)
        {
            var set = new HashSet<T>(items, comparer);
            if (set.Count != items.Length) {
                throw new ArgumentException("items are not distinct");
            }
            return new DistinctArray<T>(items);
        }

        public static DistinctArray<T> FromPossiblyNotDistinct([NotNull] IEnumerable<T> items, IEqualityComparer<T> comparer)
            => new DistinctArray<T>(new HashSet<T>(items, comparer).ToArray());

        readonly T[] items;

        DistinctArray(T[] items)
            => this.items = items;

        [NotNull]
        public T[] UnderlyingArrayThatShouldNeverBeMutated()
            => items ?? Array.Empty<T>();

        public int Count
            => UnderlyingArrayThatShouldNeverBeMutated().Length;

        public T this[int index]
            => UnderlyingArrayThatShouldNeverBeMutated()[index];

        public Enumerator GetEnumerator()
            => new Enumerator(UnderlyingArrayThatShouldNeverBeMutated());

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Efficient iterator for foreach.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            readonly T[] items;
            int idx;

            public Enumerator(T[] items)
            {
                this.items = items;
                idx = -1;
            }

            public bool MoveNext()
            {
                if (idx + 1 < items.Length) {
                    idx = idx + 1;
                    return true;
                }
                return false;
            }

            public void Reset()
                => idx = 0;

            public T Current
                => items[idx];

            object IEnumerator.Current
                => Current;

            public void Dispose() { }
        }
    }
}
