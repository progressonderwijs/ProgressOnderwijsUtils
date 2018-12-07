using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public struct TimestampedNonce : IEquatable<TimestampedNonce>
    {
        public TimestampedNonce(DateTime timestamp, long nonceValue)
        {
            Timestamp = timestamp.ToUniversalTime();
            Nonce = nonceValue;
        }

        public DateTime Timestamp { get; }
        public long Nonce { get; }
        public bool Equals(TimestampedNonce other) => other.Timestamp == Timestamp && other.Nonce == Nonce;
        public override bool Equals(object obj) => (obj as TimestampedNonce?)?.Equals(this) ?? false;
        public override int GetHashCode() => Timestamp.GetHashCode() * 397 ^ Nonce.GetHashCode();
    }

    public class NonceStore
    {
        readonly TimeSpan window = TimeSpan.FromMinutes(10);
        long nextNonce;
        readonly ConcurrentDictionary<TimestampedNonce, byte> seenNonces;
        readonly object cleanupSync = new object();
        readonly Queue<TimestampedNonce> noncesInCleanupOrder;

        //seenNonces is the primary store.
        //whenever a nonce is added, it *eventually* is also added to noncesInCleanupOrder
        public NonceStore()
        {
            noncesInCleanupOrder = new Queue<TimestampedNonce>();
            nextNonce = 0;
            seenNonces = new ConcurrentDictionary<TimestampedNonce, byte>();
        }

        public long Generate() => Interlocked.Increment(ref nextNonce);
        public bool IsFreshAndPreviouslyUnusedNonce(TimestampedNonce item, DateTime utcNow) => IsFresh(item, utcNow) && PreviouslyUnused(item, utcNow);
        bool IsFresh(TimestampedNonce item, DateTime utcNow) => (utcNow - item.Timestamp).Duration() <= window;

        bool PreviouslyUnused(TimestampedNonce freshItem, DateTime utcNow)
        {
            var wasAdded = seenNonces.TryAdd(freshItem, 0);
            if (wasAdded) {
                RegisterFutureCleanupThenCleanup(freshItem, utcNow);
            }
            return wasAdded;
        }

        void RegisterFutureCleanupThenCleanup(TimestampedNonce newNonce, DateTime utcNow)
        {
            lock (cleanupSync) {
                noncesInCleanupOrder.Enqueue(newNonce);
                while (true) {
                    if (noncesInCleanupOrder.Count == 0) {
                        //clearly no need for further cleanup
                        return;
                    }
                    var nextNonceToCleanup = noncesInCleanupOrder.Peek();
                    if (IsFresh(nextNonceToCleanup, utcNow)) {
                        //head of the queue is fresh: assume most others are fresh too.
                        return;
                    }
                    //stale nonce found!
                    noncesInCleanupOrder.Dequeue();
                    seenNonces.TryRemove(nextNonceToCleanup, out _);
                }
            }
        }
    }
}
