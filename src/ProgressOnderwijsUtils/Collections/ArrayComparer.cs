using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Collections
{
    public class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        public static readonly ArrayComparer<T> Default = new ArrayComparer<T>(EqualityComparer<T>.Default);
        readonly EqualityComparer<T> underlying;
        static readonly ulong start = (ulong)typeof(T).MetadataToken + ((ulong)typeof(T).Module.MetadataToken << 32);
        public ArrayComparer(EqualityComparer<T> underlying) { this.underlying = underlying; }

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
                for (int i = 0; i < arr.Length; i++) {
                    buffer = buffer * 997 + (ulong)underlying.GetHashCode(arr[i]);
                }
            } else {
                buffer = ~start;
            }
            return (int)((buffer >> 32) ^ buffer);
        }
    }
}
