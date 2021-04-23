using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public sealed class ArrayComparer<T> : IEqualityComparer<T[]?>
    {
        public static readonly ArrayComparer<T> Default = new(EqualityComparer<T>.Default);
        public readonly IEqualityComparer<T> UnderlyingElementComparer;
        static readonly ulong start = (ulong)typeof(T).MetadataToken + ((ulong)typeof(T).Module.MetadataToken << 32);

        public ArrayComparer(IEqualityComparer<T> underlying)
            => UnderlyingElementComparer = underlying;

        [Pure]
        public bool Equals(T[]? x, T[]? y)
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
            ulong buffer;
            if (arr != null) {
                buffer = start;
                foreach (var obj in arr) {
                    buffer = buffer * 997 + (obj is null ? 0 : (ulong)UnderlyingElementComparer.GetHashCode(obj));
                }
            } else {
                buffer = ~start;
            }
            return (int)(buffer >> 32 ^ buffer);
        }
    }
}
