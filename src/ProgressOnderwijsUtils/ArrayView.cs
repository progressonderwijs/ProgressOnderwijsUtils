using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public sealed class CollectionView_Mapped<T, TOut> : IReadOnlyCollection<TOut>
    {
        readonly ICollection<T> source;
        readonly Func<T, TOut> map;

        public CollectionView_Mapped(ICollection<T> source, Func<T, TOut> map)
        {
            this.source = source;
            this.map = map;
        }

        public int Count => source.Count;

        public IEnumerator<TOut> GetEnumerator()
        {
            foreach (var item in source) {
                yield return map(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class ArrayView_MappedByElement<T, TOut> : IReadOnlyList<TOut>
    {
        readonly IReadOnlyList<T> source;
        readonly Func<T, TOut> map;

        public ArrayView_MappedByElement(IReadOnlyList<T> source, Func<T, TOut> map)
        {
            this.source = source;
            this.map = map;
        }

        public IEnumerator<TOut> GetEnumerator()
        {
            foreach (var item in source) {
                yield return map(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public TOut this[int index] => map(source[index]);
        public int Count => source.Count;
    }

    public sealed class ArrayView_MappedWithIndex<T, TOut> : IReadOnlyList<TOut>
    {
        readonly IReadOnlyList<T> source;
        readonly Func<T, int, TOut> map;

        public ArrayView_MappedWithIndex(IReadOnlyList<T> source, Func<T, int, TOut> map)
        {
            this.source = source;
            this.map = map;
        }

        public IEnumerator<TOut> GetEnumerator()
        {
            var i = 0;
            foreach (var item in source) {
                yield return map(item, i++);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public TOut this[int index] => map(source[index], index);
        public int Count => source.Count;
    }

    public static class CollectionViewExtensions
    {
        [NotNull]
        public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, TOut> map)
        {
            return new ArrayView_MappedByElement<T, TOut>(vals, map);
        }

        [NotNull]
        public static IReadOnlyList<TOut> SelectIndexable<T, TOut>(this IReadOnlyList<T> vals, Func<T, int, TOut> map)
        {
            return new ArrayView_MappedWithIndex<T, TOut>(vals, map);
        }

        [NotNull]
        public static IReadOnlyCollection<TOut> SelectCountable<T, TOut>(this ICollection<T> vals, Func<T, TOut> map)
        {
            return new CollectionView_Mapped<T, TOut>(vals, map);
        }
    }
}
