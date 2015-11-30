using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    [CodeDieAlleenWordtGebruiktInTests]
    public class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        [CodeDieAlleenWordtGebruiktInTests]
        public static readonly ArrayComparer<T> Default = new ArrayComparer<T>(EqualityComparer<T>.Default);

        readonly IEqualityComparer<T> underlying;
        static readonly ulong start = (ulong)typeof(T).MetadataToken + ((ulong)typeof(T).Module.MetadataToken << 32);

        public ArrayComparer(IEqualityComparer<T> underlying)
        {
            this.underlying = underlying;
        }

        [Pure]
        public bool Equals(T[] x, T[] y)
        {
            if (x == null && y == null) {
                return true;
            }
            if (x == null || y == null) {
                return false;
            }
            if (x.Length != y.Length) {
                return false;
            }
            for (int i = 0; i < x.Length; i++) {
                if (!underlying.Equals(x[i], y[i])) {
                    return false;
                }
            }
            return true;
        }

        [Pure]
        public int GetHashCode(T[] arr)
        {
            ulong buffer;
            if (arr != null) {
                buffer = start;
                foreach (var obj in arr) {
                    buffer = buffer * 997 + (ulong)underlying.GetHashCode(obj);
                }
            } else {
                buffer = ~start;
            }
            return (int)((buffer >> 32) ^ buffer);
        }
    }

    public struct ComparableArray<T> : IEquatable<ComparableArray<T>>
    {
        readonly T[] array;

        public ComparableArray(T[] array)
        {
            this.array = array;
        }

        public bool Equals(ComparableArray<T> other) => ArrayComparer<T>.Default.Equals(array, other.array);
        public override int GetHashCode() => ArrayComparer<T>.Default.GetHashCode(array);
        public override bool Equals(object obj) => obj is ComparableArray<T> && Equals((ComparableArray<T>)obj);
    }
}
