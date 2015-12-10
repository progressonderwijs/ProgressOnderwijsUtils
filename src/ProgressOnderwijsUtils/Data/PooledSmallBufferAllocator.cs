using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public static class PooledSmallBufferAllocator<T>
    {
        static readonly int IndexCount = 129;
        static readonly int MaxArrayLength = IndexCount - 1;

        static readonly ConcurrentQueue<T[]>[] bagsByIndex = new ConcurrentQueue<T[]>[IndexCount];

        static ConcurrentQueue<T[]> GetBag(int index)
        {
            var bag = bagsByIndex[index];
            if (bag != null)
                return bag;
            var otherBag = Interlocked.CompareExchange(ref bagsByIndex[index], bag = new ConcurrentQueue<T[]>(), null);
            //CompareExchange returns previous value
            //if that wasn't null, this was a no-op and that value shold be returned
            //if that *was* null, we set that location, and return our bag
            return otherBag ?? bag;
        }


        /// <summary>
        /// Provides a new or reused array of the given length.
        /// </summary>
        public static T[] GetByLength(int length)
        {
            if (length > MaxArrayLength)
                return new T[length];
            var bag = GetBag(length);
            T[] result;
            if (bag.TryDequeue(out result))
                return result;
            return new T[length];
        }

        /// <summary>
        /// Releases an array array back into the pool.  It is an error for a caller to use the array after this call.
        /// Large arrays (currently longer than 128 elements) are never pooled; this operation is a no-op for such arrays.
        /// </summary>
        public static void ReturnToPool(T[] arr)
        {
            if (arr.Length > MaxArrayLength)
                return;
            var bag = GetBag(arr.Length);
            Array.Clear(arr, 0, arr.Length);
            bag.Enqueue(arr);
        }
    }
}