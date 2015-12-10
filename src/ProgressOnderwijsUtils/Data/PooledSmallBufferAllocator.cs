using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    static class PooledSmallBufferAllocator<T>
    {
        static readonly int IndexCount = 129;
        static readonly int MaxArrayLength = IndexCount-1;

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