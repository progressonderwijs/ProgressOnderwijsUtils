using System.Numerics;

namespace ProgressOnderwijsUtils.Metrics;

public sealed record MetricsCollection<TEndpoint, TDatabase, TPoolPurpose>
    where TEndpoint : struct, Enum
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum
{
    public required MetricsDescription<TEndpoint, TDatabase, TPoolPurpose> Description { get; init; }

    /// <summary>
    /// The last second included in this collection, on the collector's own timeline; never the second that
    /// is currently being written. Pass this value back as cursor to collect the seconds that follow it.
    /// That timeline is cheap to read and never runs backwards, but drifts away from real time, so this
    /// is not a time to show or store; <see cref="WindowEndUtc"/> is.
    /// </summary>
    public required long LastSecond { get; init; }

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
    public required byte SecondsCovered { get; init; }

    /// <summary>
    /// The peak of the number of concurrent requests.
    /// </summary>
    public required short MaxActiveRequests { get; init; }

    /// <summary>
    /// The thread pool usage over this window, or null when no sample was taken in these seconds.
    /// The five belong together: they come from the same sample, so all five are filled or all five are null.
    /// <see cref="MaxThreadCount"/>, <see cref="MinThreadCount"/> and <see cref="MaxPendingWorkItems"/> are levels;
    /// the other two are the increase within the window, so divide those by <see cref="SecondsCovered"/> for a rate
    /// per second.
    /// </summary>
    public required short? MaxThreadCount { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required short? MinThreadCount { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required int? MaxPendingWorkItems { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required int? CompletedWorkItems { get; init; }

    /// <inheritdoc cref="MaxThreadCount"/>
    public required int? LockContentions { get; init; }

    /// <summary>
    /// The usage of the connection pools over this window, per database and purpose (the slots of
    /// <see cref="MetricsDescription{TEndpoint,TDatabase,TPoolPurpose}.ConnectionPools"/>), or null when no sample was
    /// taken in these seconds. The three come from the same sample as the thread pool, so all three are filled or all
    /// three are null. Use <see cref="ConnectionCount{T}"/> to sum them per database, per purpose or all together.
    /// <see cref="MaxUsedConnections"/> and <see cref="MaxPendingConnections"/> are the highest levels;
    /// <see cref="OpenedConnections"/> is the increase within the window, so divide that by
    /// <see cref="SecondsCovered"/> for a rate per second.
    /// </summary>
    public required short[]? MaxUsedConnections { get; init; }

    /// <inheritdoc cref="MaxUsedConnections"/>
    public required short[]? MaxPendingConnections { get; init; }

    /// <inheritdoc cref="MaxUsedConnections"/>
    public required int[]? OpenedConnections { get; init; }

    /// <summary>
    /// A connection level of a database and purpose, or summed over all databases and/or all purposes; null when no
    /// sample was taken in this window. For the peaks a sum is an upper bound, because not every pool peaks at the
    /// same moment.
    /// </summary>
    /// <param name="perPoolSlot">One of <see cref="MaxUsedConnections"/>, <see cref="MaxPendingConnections"/> or <see cref="OpenedConnections"/>.</param>
    /// <param name="databaseSlot">The database slot, or null for all databases.</param>
    /// <param name="purpose">The purpose, or null for all purposes.</param>
    public int? ConnectionCount<T>(T[]? perPoolSlot, int? databaseSlot, TPoolPurpose? purpose)
        where T : struct, INumber<T>
    {
        if (perPoolSlot is null) {
            return null;
        }

        var pools = Description.ConnectionPools;
        var total = 0;
        for (var slot = 0; slot < perPoolSlot.Length; slot++) {
            if ((databaseSlot is null || pools.DatabaseSlotOf(slot) == databaseSlot)
                && (purpose is not { } requested || EqualityComparer<TPoolPurpose>.Default.Equals(pools.PurposeOf(slot), requested))) {
                total += int.CreateChecked(perPoolSlot[slot]);
            }
        }

        return total;
    }

    public required int[] DatabaseDurationHistogram { get; init; }

    /// <summary>
    /// The number of measurements in a bucket of a database slot, for succeeded or for failed operations.
    /// </summary>
    public int DatabaseDurationCount(int databaseSlot, int bucketIndex, bool isError)
        => DatabaseDurationHistogram[Description.DatabaseDurationHistogram.CellIndex(databaseSlot, isError, bucketIndex)];

    /// <summary>
    /// The number of measurements in a bucket across all databases.
    /// </summary>
    public int DatabaseDurationCount(int bucketIndex, bool isError)
    {
        var total = 0;
        for (var databaseSlot = 0; databaseSlot < Description.DatabaseDurationHistogram.Values.Length; databaseSlot++) {
            total += DatabaseDurationCount(databaseSlot, bucketIndex, isError);
        }

        return total;
    }

    /// <summary>
    /// The histogram of a database slot: per bucket the number of succeeded or of failed operations.
    /// </summary>
    public int[] DatabaseDurationCounts(int databaseSlot, bool isError)
        => [.. Enumerable.Range(0, Description.DatabaseDurationHistogram.Histogram.BucketCount).Select(bucketIndex => DatabaseDurationCount(databaseSlot, bucketIndex, isError)),];

    /// <summary>
    /// The histogram across all databases: per bucket the number of succeeded or of failed operations.
    /// </summary>
    public int[] DatabaseDurationCounts(bool isError)
        => [.. Enumerable.Range(0, Description.DatabaseDurationHistogram.Histogram.BucketCount).Select(bucketIndex => DatabaseDurationCount(bucketIndex, isError)),];

    /// <summary>
    /// The succeeded and the failed measurements sit in a single array; see
    /// <see cref="MetricsDimension{T}.CellIndex"/> for the layout.
    /// </summary>
    public required int[] RequestDurationHistogram { get; init; }

    /// <summary>
    /// The number of measurements in a bucket of an endpoint slot, for succeeded or for failed requests.
    /// </summary>
    public int RequestDurationCount(int endpointSlot, int bucketIndex, bool isError)
        => RequestDurationHistogram[Description.RequestDurationHistogram.CellIndex(endpointSlot, isError, bucketIndex)];

    /// <summary>
    /// The number of measurements in a bucket across all endpoints.
    /// </summary>
    public int RequestDurationCount(int bucketIndex, bool isError)
    {
        var total = 0;
        for (var endpointSlot = 0; endpointSlot < Description.RequestDurationHistogram.Values.Length; endpointSlot++) {
            total += RequestDurationCount(endpointSlot, bucketIndex, isError);
        }

        return total;
    }

    /// <summary>
    /// The histogram of an endpoint slot: per bucket the number of succeeded or of failed requests.
    /// </summary>
    public int[] RequestDurationCounts(int endpointSlot, bool isError)
        => [.. Enumerable.Range(0, Description.RequestDurationHistogram.Histogram.BucketCount).Select(bucketIndex => RequestDurationCount(endpointSlot, bucketIndex, isError)),];

    /// <summary>
    /// The histogram across all endpoints: per bucket the number of succeeded or of failed requests.
    /// </summary>
    public int[] RequestDurationCounts(bool isError)
        => [.. Enumerable.Range(0, Description.RequestDurationHistogram.Histogram.BucketCount).Select(bucketIndex => RequestDurationCount(bucketIndex, isError)),];
}
