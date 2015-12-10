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