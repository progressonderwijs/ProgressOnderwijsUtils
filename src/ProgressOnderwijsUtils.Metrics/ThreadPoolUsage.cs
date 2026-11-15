namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// The usage of the thread pool, measured separately from the traffic. The thread pool can get stuck while no
/// requests start or finish at all; hitching a ride on the request measurements would therefore measure the least
/// exactly during a congestion.
/// The two gauges (<see cref="ThreadPool.ThreadCount"/> and <see cref="ThreadPool.PendingWorkItemCount"/>) are kept
/// as peak, because of those the highest level within a second is the interesting number.
/// The minimum number of threads (<see cref="ThreadPool.GetMinThreads"/>) goes along as third gauge: above it the
/// thread pool only adds about one thread per half second, so it is the lower bound against which a congestion can be
/// read. It can go up while running; because it only rises, the peak is also the last level.
/// The two counters (<see cref="ThreadPool.CompletedWorkItemCount"/> and <see cref="Monitor.LockContentionCount"/>)
/// only ever increase since the start of the process; a peak of those is meaningless and the level itself is kept,
/// so that a window can later calculate the difference over that window.
/// Together they say more than apart: a long queue that does drain is something else than a long queue that stands
/// still, and rising lock contention is often the cause of that standing still.
/// </summary>
sealed class ThreadPoolUsage
{
    /// <summary>
    /// The thread pool usage over a window; the counters are the increase within that window here.
    /// The number of threads and the minimum fit in a short, because the thread pool limits itself to
    /// <see cref="short.MaxValue"/> threads by default and the minimum cannot exceed that. The queue does not: during
    /// a congestion there are easily tens of thousands of work items, so that one is an int; a queue of over two
    /// billion work items does not survive the memory. The increases cover at most the largest interval of the
    /// description in seconds and so fit in an int as well.
    /// </summary>
    public readonly record struct Window(short MaxThreadCount, short MinThreadCount, int MaxPendingWorkItems, int CompletedWorkItems, int LockContentions);

    const int threadCountGauge = 0;
    const int pendingWorkItemsGauge = 1;
    const int minThreadCountGauge = 2;

    // per second: one slot, with three gauges (threads, queue, minimum) and two counters (completed, lock contention)
    readonly SampledRing ring;

    public ThreadPoolUsage(SecondsRingBuffer secondsRing)
        => ring = new(secondsRing, 1, 3, 2);

    /// <summary>
    /// Reads the state of the thread pool and records it at the current second. First read and only then claim the
    /// slot: that way the measurement belongs as closely as possible to the moment it was taken.
    /// </summary>
    internal void Measurement()
    {
        ThreadPool.GetMinThreads(out var minWorkerThreads, out _);
        Span<long> gauges = stackalloc long[3];
        gauges[threadCountGauge] = ThreadPool.ThreadCount;
        gauges[pendingWorkItemsGauge] = ThreadPool.PendingWorkItemCount;
        gauges[minThreadCountGauge] = minWorkerThreads;
        Span<long> counters = [ThreadPool.CompletedWorkItemCount, Monitor.LockContentionCount,];
        ring.Record(gauges, counters);
    }

    /// <summary>
    /// The thread pool usage over the requested seconds, or null when no sample was taken in that window; see
    /// <see cref="SampledRing.Collect"/>.
    /// </summary>
    public Window? Collect(long firstSecond, long lastSecond)
        => ring.Collect(firstSecond, lastSecond) is { } window
            ? new(
                checked((short)window.GaugePeaks[threadCountGauge]),
                checked((short)window.GaugePeaks[minThreadCountGauge]),
                checked((int)window.GaugePeaks[pendingWorkItemsGauge]),
                // The increase over a window fits in an int: the window lasts at most the largest interval, and no
                // process reaches that many work items per second.
                checked((int)window.CounterIncreases[0]),
                checked((int)window.CounterIncreases[1])
            )
            : null;
}
