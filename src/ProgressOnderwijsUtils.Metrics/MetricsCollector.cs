using System.Diagnostics.Metrics;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Collects, per second, the request durations and the number of active requests of ASP.NET Core, the operation
/// durations of the OpenTelemetry SqlClient instrumentation, and samples of the thread pool and of the connection
/// pools in <see cref="ConnectionPoolRegistry{TDatabase,TPoolPurpose}"/>.
/// </summary>
public sealed class MetricsCollector<TEndpoint, TDatabase, TPoolPurpose> : IDisposable
    where TEndpoint : struct, Enum
    where TDatabase : struct, Enum
    where TPoolPurpose : struct, Enum
{
    readonly SecondsSinceStart seconds;
    readonly DurationHistogramRing<TDatabase> databaseDuration;
    readonly DurationHistogramRing<TEndpoint> requestDuration;
    readonly ActiveRequests activeRequests;
    readonly ThreadPoolUsage threadPoolUsage;
    readonly ConnectionPoolUsage<TDatabase, TPoolPurpose> connectionPoolUsage;
    readonly MeterListener listener;
    readonly ManualResetEventSlim stopSampling;
    Thread? sampler;

    /// <summary>
    /// Sampling more often than once per second, because during a congestion the thread pool takes about half a
    /// second to add a thread. At exactly one hertz that build-up could run in step with the sampling rhythm and stay
    /// practically invisible.
    /// </summary>
    static readonly TimeSpan sampleInterval = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// The number of seconds every ring keeps. That is more than the largest window: collecting skips the most recent
    /// seconds, so a window of the full length starts that many seconds earlier, and the counters need a starting
    /// level before that still. A power of two, so that the slot of a second can be found with a mask instead of with
    /// a division on every measurement.
    /// </summary>
    public const int RingCapacity = 256;

    /// <summary>
    /// The first second in which this collector measured; it begins exactly when the collector starts, so it is
    /// observed completely. Seconds before that do not exist for us and must not count as quiet.
    /// </summary>
    public const long FirstObservableSecond = 0;

    public const int DefaultSettleSeconds = 2;
    public const int DefaultMinimumWindow = 3;

    /// <param name="description">The dimensions the measurements are divided over, and the windows that may be requested.</param>
    /// <param name="pools">The connection pools to sample.</param>
    /// <param name="time">
    /// The rings count their seconds on its monotonic timestamp; its real time is only read when collecting. Defaults to
    /// <see cref="TimeProvider.System"/>; a test passes a clock of its own to decide the seconds.
    /// </param>
    public MetricsCollector(MetricsDescription<TEndpoint, TDatabase, TPoolPurpose> description, ConnectionPoolRegistry<TDatabase, TPoolPurpose> pools, TimeProvider? time = null)
    {
        Description = description;
        if (Description.LargestInterval + DefaultSettleSeconds > RingCapacity) {
            throw new InvalidOperationException($"A ring of {RingCapacity} seconds cannot hold a window of {Description.LargestInterval} seconds that ends {DefaultSettleSeconds} seconds ago.");
        }

        seconds = new(time ?? TimeProvider.System);
        databaseDuration = new(new(seconds, RingCapacity), Description.DatabaseDurationHistogram);
        requestDuration = new(new(seconds, RingCapacity), Description.RequestDurationHistogram);
        activeRequests = new(new(seconds, RingCapacity));
        threadPoolUsage = new(new(seconds, RingCapacity));
        connectionPoolUsage = new(new(seconds, RingCapacity), Description.ConnectionPools, pools);
        listener = new();
        stopSampling = new(false);
    }

    /// <summary>
    /// Signals the sampler thread to stop and waits until it is done, so that it does not still write into a half
    /// cleaned-up ring. The waiting time is ample compared to the sampling rhythm; if it hangs nevertheless, it is a
    /// background thread and does not hold up the process. The signal is not disposed then: the thread still waits
    /// on it later, and an ObjectDisposedException on a thread of our own brings down the whole process.
    /// </summary>
    public void Dispose()
    {
        listener.Dispose();
        stopSampling.Set();
        if (sampler?.Join(TimeSpan.FromSeconds(5)) is not false) {
            stopSampling.Dispose();
        }
    }

    public MetricsDescription<TEndpoint, TDatabase, TPoolPurpose> Description { get; }

    /// <param name="samplingFailed">
    /// Receives the exception that stopped the sampling: the windows after that have no thread pool and connection
    /// pool levels anymore.
    /// </param>
    public void Start(Action<Exception> samplingFailed)
    {
        listener.InstrumentPublished = (instrument, l) => {
            if (string.Equals(instrument.Meter.Name, "Microsoft.AspNetCore.Hosting", StringComparison.Ordinal)) {
                if (string.Equals(instrument.Name, "http.server.active_requests", StringComparison.Ordinal)) {
                    l.EnableMeasurementEvents(instrument, activeRequests);
                } else if (string.Equals(instrument.Name, "http.server.request.duration", StringComparison.Ordinal)) {
                    l.EnableMeasurementEvents(instrument, requestDuration);
                }
            } else if (string.Equals(instrument.Meter.Name, "OpenTelemetry.Instrumentation.SqlClient", StringComparison.Ordinal)) {
                if (string.Equals(instrument.Name, "db.client.operation.duration", StringComparison.Ordinal)) {
                    l.EnableMeasurementEvents(instrument, databaseDuration);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>(
            (_, measurement, _, state) => {
                if (state is ActiveRequests request) {
                    request.Measurement(measurement);
                }
            }
        );
        listener.SetMeasurementEventCallback<double>(
            (_, measurement, tags, state) => {
                if (ReferenceEquals(state, requestDuration)) {
                    requestDuration.Measurement(measurement, tags);
                } else if (ReferenceEquals(state, databaseDuration)) {
                    databaseDuration.Measurement(measurement, tags);
                }
            }
        );

        listener.MeasurementsCompleted = (_, state) => {
            if (state is ActiveRequests target) {
                // The instrumentation stops with requests still running: the matching -1 measurements never arrive
                // anymore, so the running total would stay stuck.
                target.Reset();
            }
        };

        listener.Start();
        StartSampling(samplingFailed);
    }

    /// <summary>
    /// Samples the thread pool and the connection pools from a thread of its own instead of from a timer on the
    /// thread pool. A thread pool timer is offered as a work item and so ends up in the same queue as all other work:
    /// exactly during the congestion this measurement has to demonstrate, the sampling itself would be left waiting.
    /// The peak in the queue is then already gone by the time it runs, or the window holds no sample at all and falls
    /// back to null. A thread of its own is scheduled by the operating system and so is independent of what it
    /// measures.
    /// </summary>
    void StartSampling(Action<Exception> samplingFailed)
    {
        sampler = new(
            () => MetricsSampler.SampleUntilStopped(
                stopSampling,
                sampleInterval,
                () => {
                    threadPoolUsage.Measurement();
                    connectionPoolUsage.Measurement();
                },
                samplingFailed
            )
        ) {
            IsBackground = true,
            Name = "MetricsSampler",
        };
        sampler.Start();
    }

    /// <summary>
    /// Collects the last <paramref name="interval"/> completed seconds.
    /// </summary>
    /// <param name="interval">The requested window in seconds.</param>
    /// <param name="settleSeconds">
    /// How many recent seconds are skipped. The current second is still being written and the second before it can
    /// still get a late writer from a thread that took its timestamp just before the change, so by default we look
    /// two seconds back.
    /// </param>
    /// <param name="minimumWindow">
    /// The window is stretched to this so that short intervals are less jerky. The result states in SecondsCovered
    /// how many seconds it really holds.
    /// </param>
    public MetricsCollection<TEndpoint, TDatabase, TPoolPurpose> Collect(int interval, int settleSeconds = DefaultSettleSeconds, int minimumWindow = DefaultMinimumWindow)
    {
        if (interval < Description.SmallestInterval || interval > Description.LargestInterval) {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be in range of the description.");
        }

        var lastSecond = LatestCompleteSecond(settleSeconds);
        return CollectRange(lastSecond - Math.Max(interval, minimumWindow) + 1, lastSecond);
    }

    /// <summary>
    /// Collects everything after the second the caller has already processed, up to and including the last completed
    /// second. Meant for continuous storing: by draining up to the present a slow writer catches up by itself instead
    /// of structurally lagging behind.
    /// The result states with LastSecond and SecondsCovered what was really processed, so a caller that lagged
    /// further behind than the ring can recognise the gap.
    /// </summary>
    /// <param name="previousLastSecond">The <see cref="MetricsCollection{TEndpoint,TDatabase,TPoolPurpose}.LastSecond"/> of what the caller has already processed.</param>
    /// <param name="settleSeconds">See <see cref="Collect"/>.</param>
    public MetricsCollection<TEndpoint, TDatabase, TPoolPurpose> CollectAfter(long previousLastSecond, int settleSeconds = DefaultSettleSeconds)
    {
        var lastSecond = LatestCompleteSecond(settleSeconds);
        return CollectRange(previousLastSecond + 1, lastSecond);
    }

    long LatestCompleteSecond(int settleSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(settleSeconds);
        return seconds.Second - settleSeconds;
    }

    /// <summary>
    /// Collects exactly the requested seconds, for a caller that keeps track itself of where it left off and wants
    /// to end at the same last second as an earlier <see cref="Collect"/>.
    /// The seconds are those of the timeline of the rings, like
    /// <see cref="MetricsCollection{TEndpoint,TDatabase,TPoolPurpose}.LastSecond"/>.
    /// </summary>
    public MetricsCollection<TEndpoint, TDatabase, TPoolPurpose> CollectRange(long firstSecond, long lastSecond)
    {
        // A window is at most LargestInterval seconds long. The ring keeps more (RingCapacity), so that the oldest
        // seconds of such a window are still there as well. Seconds from before our own start do not exist at all:
        // counting those as quiet would make the traffic right after starting seem structurally too low for whoever
        // divides by SecondsCovered. This limit is also the reason SecondsCovered fits in a byte.
        firstSecond = Math.Max(firstSecond, lastSecond - Description.LargestInterval + 1);
        firstSecond = Math.Max(firstSecond, FirstObservableSecond);

        var threadPool = threadPoolUsage.Collect(firstSecond, lastSecond);
        var connectionPools = connectionPoolUsage.Collect(firstSecond, lastSecond);
        return new() {
            LastSecond = lastSecond,
            WindowEndUtc = seconds.EndUtcOf(lastSecond),
            SecondsCovered = firstSecond > lastSecond ? (byte)0 : (byte)(lastSecond - firstSecond + 1),
            MaxActiveRequests = activeRequests.Collect(firstSecond, lastSecond),
            MaxThreadCount = threadPool?.MaxThreadCount,
            MinThreadCount = threadPool?.MinThreadCount,
            MaxPendingWorkItems = threadPool?.MaxPendingWorkItems,
            CompletedWorkItems = threadPool?.CompletedWorkItems,
            LockContentions = threadPool?.LockContentions,
            MaxUsedConnections = connectionPools?.MaxUsedConnections,
            MaxPendingConnections = connectionPools?.MaxPendingConnections,
            OpenedConnections = connectionPools?.OpenedConnections,
            RequestDurationHistogram = requestDuration.Collect(firstSecond, lastSecond),
            DatabaseDurationHistogram = databaseDuration.Collect(firstSecond, lastSecond),
            Description = Description,
        };
    }
}
