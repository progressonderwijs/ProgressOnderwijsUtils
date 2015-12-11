using System;
using System.Runtime.CompilerServices;

namespace ProgressOnderwijsUtils.Collections
{
    public interface IArrayBuilder<T>
    {
        void Add(T item);
        T[] ToArray();
    }

    public struct FastArrayBuilder<T> : IArrayBuilder<T>
    {
        const int InitSize2Pow = 4;
        const int InitSize = (1 << InitSize2Pow) - 1;
        int idx, sI;
        T[] current;
        T[][] segments;
        public static FastArrayBuilder<T> Create() => new FastArrayBuilder<T> { current = new T[InitSize] };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (idx < current.Length) {
                current[idx++] = item;
            } else {
                if (segments == null) {
                    segments = new T[31 - InitSize2Pow][];
                }
                segments[sI++] = current;
                current = new T[(current.Length << 1) & ~current.Length];
                current[0] = item;
                idx = 1;
            }
        }

        public T[] ToArray()
        {
            if (segments == null) {
                var retval = current;
                Array.Resize(ref retval, idx);
                return retval;
            } else {
                int sumlength = (1 << (sI + InitSize2Pow - 1)) + idx - 1;
                var retval = new T[sumlength];
                int j = 0;
                for (int sJ = 0; sJ < sI; sJ++) {
                    var subarr = segments[sJ];
                    subarr.CopyTo(retval, j);
                    j += subarr.Length;
                }
                Array.Copy(current, 0, retval, j, idx);
                return retval;
            }
        }
    }
}
