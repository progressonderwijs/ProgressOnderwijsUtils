namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The usage of the connection pools per database and pool, measured in the same way as the thread pool and by
/// the same sampler thread. The gauges (connections in use, and callers waiting for a connection) are kept as peak;
/// the number of succeeded opens only ever increases and is kept as level, so that a window can calculate the
/// increase.
/// A pool fills up on its own, so the pools of a database are not summed but kept apart, on the slots of
/// <see cref="ConnectionPoolSlots{TDatabase,TPool}"/>.
/// </summary>
sealed class ConnectionPoolUsage<TDatabase, TPool>
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    /// <summary>
    /// The usage over a window, per slot of <see cref="ConnectionPoolSlots{TDatabase,TPool}"/>; the opens are the
    /// increase within the window.
    /// </summary>
    public sealed record Window(long[] MaxUsedConnections, long[] MaxPendingConnections, long[] OpenedConnections);

    const int usedGauge = 0;
    const int pendingGauge = 1;
    const int gaugeCount = 2;

    readonly ConnectionPoolSlots<TDatabase, TPool> slots;
    readonly ConnectionPoolRegistry<TDatabase, TPool> pools;
    readonly SampledRing ring;

    public ConnectionPoolUsage(SecondsSinceStart clock, int length, ConnectionPoolSlots<TDatabase, TPool> slots, ConnectionPoolRegistry<TDatabase, TPool> pools)
    {
        this.slots = slots;
        this.pools = pools;
        ring = new(clock, length, slots.Count, gaugeCount, 1);
    }

    internal void Measurement()
        => Measurement(pools.All);

    /// <summary>
    /// Sums the levels of the pools per database and records them at the current second.
    /// The sum lives on the stack, bounded by <see cref="ConnectionPoolSlots{TDatabase,TPool}.MaxCount"/>, so sampling
    /// allocates nothing.
    /// </summary>
    internal void Measurement(ReadOnlySpan<ConnectionPoolCounters<TDatabase, TPool>> sampledPools)
    {
        var slotCount = ring.SlotCount;
        Span<long> gauges = stackalloc long[slotCount * gaugeCount];
        Span<long> opened = stackalloc long[slotCount];
        gauges.Clear();
        opened.Clear();

        foreach (var pool in sampledPools) {
            var slot = slots.SlotOf(pool.Database, pool.Pool);
            gauges[slot * gaugeCount + usedGauge] += pool.InUse;
            gauges[slot * gaugeCount + pendingGauge] += pool.Pending;
            opened[slot] += pool.Opened;
        }

        ring.Record(gauges, opened);
    }

    /// <summary>
    /// The usage over the requested seconds, or null when no sample was taken in that window; see
    /// <see cref="SampledRing.TryCollect"/>. Only the result is allocated: the ring writes into spans on the stack,
    /// bounded by <see cref="ConnectionPoolSlots{TDatabase,TPool}.MaxCount"/>.
    /// </summary>
    public Window? Collect(int firstSecond, int lastSecond)
    {
        Span<long> gaugePeaks = stackalloc long[ring.GaugesPerSecond];
        Span<long> openedIncreases = stackalloc long[ring.CountersPerSecond];
        if (!ring.TryCollect(firstSecond, lastSecond, gaugePeaks, openedIncreases)) {
            return null;
        }

        var slotCount = ring.SlotCount;
        var maxUsed = new long[slotCount];
        var maxPending = new long[slotCount];
        for (var slot = 0; slot < slotCount; slot++) {
            maxUsed[slot] = gaugePeaks[slot * gaugeCount + usedGauge];
            maxPending[slot] = gaugePeaks[slot * gaugeCount + pendingGauge];
        }

        return new(maxUsed, maxPending, openedIncreases.ToArray());
    }
}
