using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public sealed class ArrayComparer<T> : IEqualityComparer<T[]?>
    {
        public static readonly ArrayComparer<T> Default = new(EqualityComparer<T>.Default);
        public readonly IEqualityComparer<T> UnderlyingElementComparer;
        const int NullHashCode = 0x1d45_7af3;

        public ArrayComparer(IEqualityComparer<T> underlying)
            => UnderlyingElementComparer = underlying;

        [Pure]
        public bool Equals(T[]? x, T[]? y)
        {
            if (x == null) {
                return y == null;
            } else if (y == null) {
                return false;
            } else if (x.Length != y.Length) {
                return false;
            }

            for (var i = 0; i < x.Length; i++) {
                if (!UnderlyingElementComparer.Equals(x[i], y[i])) {
                    return false;
                }
            }
            return true;
        }

        [Pure]
        public int GetHashCode(T[]? arr)
        {
            if (arr == null) {
                return NullHashCode;
            }
            var buffer = new HashCode();
            foreach (var obj in arr) {
                buffer.Add(obj, UnderlyingElementComparer);
            }
            return buffer.ToHashCode();
        }
    }
}
