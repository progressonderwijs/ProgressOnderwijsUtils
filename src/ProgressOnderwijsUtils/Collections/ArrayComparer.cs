using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public sealed class SequenceEqualityComparer<T> : IEqualityComparer<T[]?>, IEqualityComparer<IEnumerable<T>?>
    {
        public static readonly SequenceEqualityComparer<T> Default = new(EqualityComparer<T>.Default);
        public readonly IEqualityComparer<T> UnderlyingElementComparer;
        const int NullHashCode = 0x1d45_7af3;

        public SequenceEqualityComparer(IEqualityComparer<T> underlying)
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

        [Pure]
        public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
        {
            if (x == null) {
                return y == null;
            } else if (y == null) {
                return false;
            }

            using var xs = x.GetEnumerator();
            using var ys = y.GetEnumerator();

            while (true) {
                var hasX = xs.MoveNext();
                var hasY = ys.MoveNext();
                if (hasX && hasY) {
                    if (!UnderlyingElementComparer.Equals(xs.Current, ys.Current)) {
                        return false;
                    }
                } else {
                    return !hasX && !hasY;
                }
            }
        }

        [Pure]
        public int GetHashCode(IEnumerable<T>? seq)
        {
            if (seq == null) {
                return NullHashCode;
            }
            var buffer = new HashCode();
            foreach (var obj in seq) {
                buffer.Add(obj, UnderlyingElementComparer);
            }
            return buffer.ToHashCode();
        }
    }
}
