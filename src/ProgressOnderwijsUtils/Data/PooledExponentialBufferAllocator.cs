using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils
{
    public static class PooledExponentialBufferAllocator<T>
    {
        static readonly int IndexCount = 15;
        static readonly int MaxIndex = IndexCount - 1;
        static readonly int MaxArrayLength = 1 << MaxIndex;
        static readonly ConcurrentQueue<T[]>[] bagsByIndex = InitBags();

        static ConcurrentQueue<T[]>[] InitBags()
        {
            var allBags = new ConcurrentQueue<T[]>[IndexCount];
            for (int i = 0; i < IndexCount; i++) {
                allBags[i] = new ConcurrentQueue<T[]>();
            }
            return allBags;
        }

        /// <summary>
        /// Provides a new or reused array of the given length (rounded up to the nearest power of 2)
        /// </summary>
        public static T[] GetByLength(uint length)
        {
            if (length > MaxArrayLength) {
                return new T[length];
            }
            var i = Utils.LogBase2RoundedUp(length);
            var bag = bagsByIndex[i];
            T[] result;
            if (bag.TryDequeue(out result)) {
                return result;
            }
            return new T[1 << i];
        }

        /// <summary>
        /// Releases an array array back into the pool.  It is an error for a caller to use the array after this call.
        /// Arrays that are not an exact power of two or are too large (more than 2^14) are never pooled; this operation is a no-op for such arrays.
        /// 
        /// It is inefficient to return "old" arrays into the pool; the most efficient usage is when the consumer only need needs few arrays simultaneously.
        /// Note that arrays are *NOT* cleared when they are returned to the pool; for reference types it may be advisable to clear the array to avoid unnecessary gc load.
        /// </summary>
        public static void ReturnToPool(T[] arr)
        {
            if (arr.Length > MaxArrayLength) {
                return;
            }
            var i = Utils.LogBase2RoundedUp((uint)arr.Length);
            if (1 << i == arr.Length) {
                var bag = bagsByIndex[i];
                bag.Enqueue(arr);
            }
        }
    }
}
