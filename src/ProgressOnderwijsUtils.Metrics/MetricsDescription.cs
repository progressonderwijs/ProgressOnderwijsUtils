namespace ProgressOnderwijsUtils.Metrics;

public sealed class MetricsDescription<TDatabase, TEndpoint, TPoolPurpose>
    where TDatabase : struct, Enum
    where TEndpoint : struct, Enum
    where TPoolPurpose : struct, Enum
{
    /// <summary>
    /// Number of seconds.
    /// </summary>
    public required byte SmallestInterval { get; init; }

    /// <summary>
    /// Number of seconds; the largest window that may be requested. The ring keeps more seconds than that, see
    /// <see cref="MetricsCollector{TDatabase,TEndpoint,TPoolPurpose}.RingCapacity"/>.
    /// </summary>
    public required byte LargestInterval { get; init; }

    public required MetricsDimension<TDatabase> DatabaseDurationHistogram { get; init; }

    /// <summary>
    /// The slots of the connection usage: per database slot of <see cref="DatabaseDurationHistogram"/> and per purpose.
    /// </summary>
    public ConnectionPoolSlots<TDatabase, TPoolPurpose> ConnectionPools
        => field ??= new(DatabaseDurationHistogram);

    public required MetricsDimension<TEndpoint> RequestDurationHistogram { get; init; }
}
