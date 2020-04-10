using System;
using System.Collections.Concurrent;

namespace ProgressOnderwijsUtils
{
    public static class PooledSmallBufferAllocator<T>
    {
        const int MaxArrayLength = 128;
        const int IndexCount = MaxArrayLength + 1;

        //Unfortunately, ConcurrentStacks and ConcurrentBags perform allocations when used in this fashion, and are thus unsuitable
        //conceptually, a ConcurrentBag that doesn't allocation on .Add(...) is what we're looking for here, and a queue is close enough.
        static readonly ConcurrentQueue<T[]>[] bagsByIndex = InitBags();

        static ConcurrentQueue<T[]>[] InitBags()
        {
            var allBags = new ConcurrentQueue<T[]>[IndexCount];
            for (var i = 0; i < IndexCount; i++) {
                allBags[i] = new ConcurrentQueue<T[]>();
            }
            return allBags;
        }

        /// <summary>
        /// Provides a new or reused array of the given length.
        /// </summary>
        public static T[] GetByLength(int length)
        {
            if (length > MaxArrayLength) {
                return new T[length];
            }
            var bag = bagsByIndex[length];
            if (bag.TryDequeue(out var result)) {
                return result;
            }
            return new T[length];
        }

        /// <summary>
        /// Releases an array array back into the pool.  It is an error for a caller to use the array after this call.
        /// Large arrays (currently longer than 128 elements) are never pooled; this operation is a no-op for such arrays.
        /// </summary>
        public static void ReturnToPool(T[] arr)
        {
            if (arr.Length > MaxArrayLength) {
                return;
            }
            var bag = bagsByIndex[arr.Length];
            Array.Clear(arr, 0, arr.Length);
            bag.Enqueue(arr);
        }
    }
}
