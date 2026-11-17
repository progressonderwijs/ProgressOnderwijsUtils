namespace ProgressOnderwijsUtils.Metrics;

public sealed record MetricsCollection<TEndpoint, TDatabase, TPool>
    where TEndpoint : struct, Enum
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    public required MetricsDescription<TEndpoint, TDatabase, TPool> Description { get; init; }

    /// <summary>
    /// The last second included in this collection, on the collector's own timeline; never the second that
    /// is currently being written. Pass this value back as cursor to collect the seconds that follow it.
    /// That timeline is cheap to read and never runs backwards, but drifts away from real time, so this
    /// is not a time to show or store; <see cref="WindowEndUtc"/> is.
    /// </summary>
    public required int LastSecond { get; init; }

    /// <summary>
    /// The real time at which <see cref="LastSecond"/> ended, and so the window with it. The window started
    /// <see cref="SecondsCovered"/> seconds earlier. Two collections that follow each other exactly by
    /// <see cref="LastSecond"/> therefore also follow each other here, up to the drift between the clocks over the time
    /// between the two collections, or a jump of the wall clock.
    /// </summary>
    public required DateTime WindowEndUtc { get; init; }

    /// <summary>
    /// The number of seconds actually aggregated, which can exceed the requested interval because of
    /// smoothing, and can be lower than requested when the ring buffer could not reach that far back.
    /// The histograms are raw sums over these seconds, so divide by this to get a rate per second.
    /// </summary>
    public required int SecondsCovered { get; init; }

    /// <summary>
    /// The peak of the number of concurrent requests.
    /// </summary>
    public required long MaxActiveRequests { get; init; }

    /// <summary>
    /// The thread pool usage over this window, or null when no sample was taken in these seconds.
    /// The five belong together: they come from the same sample, so all five are filled or all five are null.
    /// <see cref="MaxThreadCount"/>, <see cref="MinThreadCount"/> and <see cref="MaxPendingWorkItems"/> are levels;
    /// the other two are the increase within the window, so divide those by <see cref="SecondsCovered"/> for a rate
    /// per second.
    /// </summary>
    public required int? MaxThreadCount { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required int? MinThreadCount { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required long? MaxPendingWorkItems { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required long? CompletedWorkItems { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required long? LockContentions { get; init; }

    /// <summary>
    /// The usage of the connection pools over this window, per database or per pool (the slots of
    /// <see cref="MetricsDescription{TEndpoint,TDatabase,TPool}.ConnectionPools"/>), or null when no sample was
    /// taken in these seconds. The three come from the same sample as the thread pool, so all three are filled or all
    /// three are null. Use <see cref="ConnectionCount"/> to sum them per database, per pool or all together.
    /// <see cref="MaxUsedConnections"/> and <see cref="MaxPendingConnections"/> are the highest levels;
    /// <see cref="OpenedConnections"/> is the increase within the window, so divide that by
    /// <see cref="SecondsCovered"/> for a rate per second.
    /// </summary>
    public required long[]? MaxUsedConnections { get; init; }

    /// <inheritdoc cref="MaxUsedConnections"/>
    public required long[]? MaxPendingConnections { get; init; }

    /// <inheritdoc cref="MaxUsedConnections"/>
    public required long[]? OpenedConnections { get; init; }

    /// <summary>
    /// A connection level of a database and pool, or summed over all databases and/or all pools; null when no
    /// sample was taken in this window. For the peaks a sum is an upper bound, because not every pool peaks at the
    /// same moment.
    /// </summary>
    /// <param name="perPoolSlot">One of <see cref="MaxUsedConnections"/>, <see cref="MaxPendingConnections"/> or <see cref="OpenedConnections"/>.</param>
    /// <param name="databaseSlot">The database slot, or null for all databases.</param>
    /// <param name="pool">The pool, or null for all pools.</param>
    public long? ConnectionCount(long[]? perPoolSlot, int? databaseSlot, TPool? pool)
    {
        if (perPoolSlot is null) {
            return null;
        }

        var pools = Description.ConnectionPools;
        var total = 0L;
        for (var slot = 0; slot < perPoolSlot.Length; slot++) {
            if ((databaseSlot is null || pools.DatabaseSlotOf(slot) == databaseSlot)
                && (pool is not { } requested || EqualityComparer<TPool>.Default.Equals(pools.PoolOf(slot), requested))) {
                total += perPoolSlot[slot];
            }
        }

        return total;
    }

    public required int[] DatabaseDurationHistogram { get; init; }

    /// <summary>
    /// The number of measurements in a bucket of a database slot, for succeeded or for failed operations.
    /// </summary>
    public int DatabaseDurationCount(int databaseSlot, int bucketIndex, bool isError)
        => Description.DatabaseDurationHistogram.Count(DatabaseDurationHistogram, databaseSlot, bucketIndex, isError);

    /// <summary>
    /// The number of measurements in a bucket across all databases.
    /// </summary>
    public int DatabaseDurationCount(int bucketIndex, bool isError)
        => Description.DatabaseDurationHistogram.Count(DatabaseDurationHistogram, bucketIndex, isError);

    /// <summary>
    /// The histogram of a database slot: per bucket the number of succeeded or of failed operations.
    /// </summary>
    public int[] DatabaseDurationCounts(int databaseSlot, bool isError)
        => Description.DatabaseDurationHistogram.Counts(DatabaseDurationHistogram, databaseSlot, isError);

    /// <summary>
    /// The histogram across all databases: per bucket the number of succeeded or of failed operations.
    /// </summary>
    public int[] DatabaseDurationCounts(bool isError)
        => Description.DatabaseDurationHistogram.Counts(DatabaseDurationHistogram, isError);

    /// <summary>
    /// The succeeded and the failed measurements sit in a single array; see
    /// <see cref="MetricsDimension{T}.CellIndex"/> for the layout.
    /// </summary>
    public required int[] RequestDurationHistogram { get; init; }

    /// <summary>
    /// The number of measurements in a bucket of an endpoint slot, for succeeded or for failed requests.
    /// </summary>
    public int RequestDurationCount(int endpointSlot, int bucketIndex, bool isError)
        => Description.RequestDurationHistogram.Count(RequestDurationHistogram, endpointSlot, bucketIndex, isError);

    /// <summary>
    /// The number of measurements in a bucket across all endpoints.
    /// </summary>
    public int RequestDurationCount(int bucketIndex, bool isError)
        => Description.RequestDurationHistogram.Count(RequestDurationHistogram, bucketIndex, isError);

    /// <summary>
    /// The histogram of an endpoint slot: per bucket the number of succeeded or of failed requests.
    /// </summary>
    public int[] RequestDurationCounts(int endpointSlot, bool isError)
        => Description.RequestDurationHistogram.Counts(RequestDurationHistogram, endpointSlot, isError);

    /// <summary>
    /// The histogram across all endpoints: per bucket the number of succeeded or of failed requests.
    /// </summary>
    public int[] RequestDurationCounts(bool isError)
        => Description.RequestDurationHistogram.Counts(RequestDurationHistogram, isError);
}
