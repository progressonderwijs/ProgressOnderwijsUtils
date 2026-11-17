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
    /// The thread pool usage over a window, each in the type the runtime reports it in; the counters are the increase
    /// within that window here.
    /// </summary>
    public readonly record struct Window(int MaxThreadCount, int MinThreadCount, long MaxPendingWorkItems, long CompletedWorkItems, long LockContentions);

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
    /// <see cref="SampledRing.TryCollect"/>.
    /// </summary>
    public Window? Collect(int firstSecond, int lastSecond)
    {
        Span<long> gaugePeaks = stackalloc long[ring.GaugesPerSecond];
        Span<long> counterIncreases = stackalloc long[ring.CountersPerSecond];
        if (!ring.TryCollect(firstSecond, lastSecond, gaugePeaks, counterIncreases)) {
            return null;
        }

        return new(
            // The ring keeps every gauge as a long, but these two only ever received ints, and the peak of ints is one
            // of them; the conversion restores their type and fails only on a bug.
            checked((int)gaugePeaks[threadCountGauge]),
            checked((int)gaugePeaks[minThreadCountGauge]),
            gaugePeaks[pendingWorkItemsGauge],
            counterIncreases[0],
            counterIncreases[1]
        );
    }
}
