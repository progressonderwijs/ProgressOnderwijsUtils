using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Data.SqlClient.Diagnostics;

namespace ProgressOnderwijsUtils.Metrics;

/// <summary>
/// Collects, per second, the request durations and the number of active requests of ASP.NET Core, the operation
/// durations of the OpenTelemetry SqlClient instrumentation, and samples of the thread pool and of the connection
/// pools in <see cref="ConnectionPoolRegistry{TDatabase,TPool}"/>, counted from the open events of SqlClient.
/// </summary>
public sealed class MetricsCollector<TEndpoint, TDatabase, TPool> : IDisposable
    where TEndpoint : struct, Enum
    where TDatabase : struct, Enum
    where TPool : struct, Enum
{
    /// <summary>
    /// The number of seconds every ring keeps. That is more than the largest window: collecting skips the most recent
    /// seconds, so a window of the full length starts that many seconds earlier, and the counters need a starting
    /// level before that still. A power of two, so that the slot of a second can be found with a mask instead of with
    /// a division on every measurement.
    /// </summary>
    public const int RingCapacity = 256;

    public const int DefaultSettleSeconds = 2;
    public const int DefaultMinimumWindow = 3;

    /// <summary>
    /// The first second in which this collector measured; it begins exactly when the collector starts, so it is
    /// observed completely. Seconds before that do not exist for us and must not count as quiet.
    /// </summary>
    public const int FirstObservableSecond = 0;

    public MetricsDescription<TEndpoint, TDatabase, TPool> Description { get; }
    readonly SecondsSinceStart seconds;
    readonly DurationHistogramRing<TDatabase> databaseDuration;
    readonly DurationHistogramRing<TEndpoint> requestDuration;
    readonly ActiveRequests activeRequests;
    readonly ThreadPoolUsage threadPoolUsage;
    readonly ConnectionPoolUsage<TDatabase, TPool> connectionPoolUsage;
    readonly ConnectionPoolRegistry<TDatabase, TPool> pools;
    readonly MeterListener listener;
    readonly ManualResetEventSlim stopSampling;
    readonly List<IDisposable> sqlClientSubscriptions;
    readonly Lock lifetime;
    bool started;
    bool disposed;
    Thread? sampler;
    IDisposable? allDiagnosticListenersSubscription;

    /// <param name="description">The dimensions the measurements are divided over, and the windows that may be requested.</param>
    /// <param name="pools">The connection pools to sample.</param>
    /// <param name="time">
    /// The rings count their seconds on its monotonic timestamp; its real time is only read when collecting. Defaults to
    /// <see cref="TimeProvider.System"/>; a test passes a clock of its own to decide the seconds.
    /// </param>
    public MetricsCollector(MetricsDescription<TEndpoint, TDatabase, TPool> description, ConnectionPoolRegistry<TDatabase, TPool> pools, TimeProvider? time = null)
    {
        if (description.SmallestInterval < 1 || description.SmallestInterval > description.LargestInterval) {
            throw new InvalidOperationException(
                $"The smallest interval of {description.SmallestInterval} seconds must be at least 1 second and at most the largest interval of {description.LargestInterval} seconds."
            );
        }

        if (description.LargestInterval + DefaultSettleSeconds >= RingCapacity) {
            throw new InvalidOperationException($"A ring of {RingCapacity} seconds cannot hold a window of {description.LargestInterval} seconds that ends {DefaultSettleSeconds} seconds ago.");
        }

        this.pools = pools;
        Description = description;
        seconds = new(time ?? TimeProvider.System);
        databaseDuration = new(new(seconds, RingCapacity), Description.DatabaseDurationHistogram);
        requestDuration = new(new(seconds, RingCapacity), Description.RequestDurationHistogram);
        activeRequests = new(new(seconds, RingCapacity));
        threadPoolUsage = new(new(seconds, RingCapacity));
        connectionPoolUsage = new(new(seconds, RingCapacity), Description.ConnectionPools, pools);
        listener = new();
        lifetime = new();
        sqlClientSubscriptions = [];
        stopSampling = new(false);
    }

    /// <summary>
    /// Signals the sampler thread to stop and waits until it is done, so that it does not still write into a half
    /// cleaned-up ring. The waiting time is ample compared to the sampling rhythm; if it hangs nevertheless, it is a
    /// background thread and does not hold up the process. The signal is not disposed then: the thread still waits
    /// on it later, and an ObjectDisposedException on a thread of our own brings down the whole process.
    /// Only the first call cleans up: a collector stopped early is disposed again when the host stops.
    /// The subscription on all diagnostic listeners is released outside the lock, for the same reason it is made
    /// outside the lock in <see cref="Start"/>.
    /// </summary>
    public void Dispose()
    {
        if (disposed) {
            return;
        }

        IDisposable? allListeners;
        lock (lifetime) {
            if (disposed || !started) {
                return;
            }

            allListeners = allDiagnosticListenersSubscription;
            allDiagnosticListenersSubscription = null;
            foreach (var subscription in sqlClientSubscriptions) {
                subscription.Dispose();
            }
            sqlClientSubscriptions.Clear();

            stopSampling.Set();
            if (sampler.AssertNotNull().Join(TimeSpan.FromSeconds(5))) {
                stopSampling.Dispose();
            }

            listener.Dispose();

            disposed = true;
        }
        allListeners?.Dispose();
    }

    /// <summary>
    /// Starts listening and sampling. A collector that is already disposed does not start anymore: its stop signal is
    /// gone, and listeners hooked up after the cleanup would never be released. A second call is ignored. A start that is
    /// rejected because another host already published its ASP.NET Core instruments leaves the collector disposed, so
    /// starting it again does nothing.
    /// The subscription on all diagnostic listeners is made outside the lock: DiagnosticListener calls our observer
    /// while holding its own global lock, and the observer takes ours; the other way around could deadlock.
    /// </summary>
    /// <param name="hostMeters">
    /// The meter factory of the host whose requests are measured. Every host creates its own ASP.NET Core instruments
    /// through its own factory, and the listener sees those of all hosts in the process. Measuring more than one host is
    /// not supported: the ASP.NET Core instruments of any other factory are rejected with a
    /// <see cref="NotSupportedException"/>, which ends up in the code that creates them, typically the startup of that
    /// other host. The SqlClient sources are static and therefore always process-wide.
    /// </param>
    /// <param name="samplingFailed">
    /// Receives the exception that stopped the sampling: the windows after that have no thread pool and connection
    /// pool levels anymore.
    /// </param>
    public void Start(IMeterFactory hostMeters, Action<Exception> samplingFailed)
    {
        if (started) {
            return;
        }

        lock (lifetime) {
            if (started || disposed) {
                return;
            }

            try {
                StartListening();
            } catch {
                listener.Dispose();
                disposed = true;
                throw;
            }
            StartSampling();
            started = true;
        }

        var subscription = StartDiagnosing();

        lock (lifetime) {
            if (disposed) {
                subscription.Dispose();
            } else {
                allDiagnosticListenersSubscription = subscription;
            }
        }

        void StartListening()
        {
            listener.InstrumentPublished = (instrument, l) => {
                if (string.Equals(instrument.Meter.Name, "Microsoft.AspNetCore.Hosting", StringComparison.Ordinal)) {
                    if (instrument.Meter.Scope != hostMeters) {
                        throw new NotSupportedException(
                            """
                            The metrics collector cannot measure the requests of more than one host: a second host published its ASP.NET Core
                            instruments while the collector measures the host it was registered on.
                            Register the metrics collector on one host only, and run no other web hosts in the same process while it runs.
                            """
                        );
                    } else if (string.Equals(instrument.Name, "http.server.active_requests", StringComparison.Ordinal)) {
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
        }

        // Samples the thread pool and the connection pools from a thread of its own instead of from a timer on the
        // thread pool. A thread pool timer is offered as a work item and so ends up in the same queue as all other work:
        // exactly during the congestion this measurement has to demonstrate, the sampling itself would be left waiting.
        // The peak in the queue is then already gone by the time it runs, or the window holds no sample at all and falls
        // back to null. A thread of its own is scheduled by the operating system and so is independent of what it
        // measures.
        void StartSampling()
        {
            sampler = new(
                () => MetricsSampler.SampleUntilStopped(
                    stopSampling,
                    TimeSpan.FromMilliseconds(250),
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

        // Calls back right away for the SqlClient listener when it already exists, otherwise as soon as SqlClient creates it.
        IDisposable StartDiagnosing()
            => DiagnosticListener.AllListeners.Subscribe(
                new NextObserver<DiagnosticListener>(
                    diagnostics => {
                        lock (lifetime) {
                            if (!disposed && string.Equals(diagnostics.Name, "SqlClientDiagnosticListener", StringComparison.Ordinal)) {
                                var sqlClientSubscription = diagnostics.Subscribe(
                                    new NextObserver<KeyValuePair<string, object?>>(SqlClientEvent),
                                    name => name is SqlClientConnectionOpenBefore.Name or SqlClientConnectionOpenError.Name
                                );
                                sqlClientSubscriptions.Add(sqlClientSubscription);
                            }
                        }
                    }
                )
            );
    }

    /// <summary>
    /// Only the start and the failure of an open are needed: from the start on the pool follows the connection itself
    /// through its StateChange event. Thus the pool is only looked up while the connection string still holds the
    /// password, and an open never has to be matched to its end through a lookup on its OperationId.
    /// Without a subscriber SqlClient skips these events altogether, so a process whose collector does not run pays
    /// nothing. Connections that are already open when the collector starts are not counted:
    /// <see cref="ConnectionPoolCounters{TDatabase,TPool}.InUse"/> is too low until they are closed.
    /// </summary>
    void SqlClientEvent(KeyValuePair<string, object?> sqlClientEvent)
    {
        switch (sqlClientEvent.Value) {
            case SqlClientConnectionOpenBefore { Connection: { } connection, }:
                pools.OpenStarted(connection);
                break;
            // An empty OperationId means nobody listened when this open started, so it was never pending. When another
            // subscriber did listen, a failing open that started just before the collector started still makes pending
            // one too low; the collector starts before the host does, so such an open practically does not exist.
            case SqlClientConnectionOpenError { Connection: { } connection, } openError when openError.OperationId != Guid.Empty:
                pools.OpenFailed(connection);
                break;
        }
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
    public MetricsCollection<TEndpoint, TDatabase, TPool> Collect(int interval, int settleSeconds = DefaultSettleSeconds, int minimumWindow = DefaultMinimumWindow)
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
    /// <param name="previousLastSecond">The <see cref="MetricsCollection{TEndpoint,TDatabase,TPool}.LastSecond"/> of what the caller has already processed.</param>
    /// <param name="settleSeconds">See <see cref="Collect"/>.</param>
    public MetricsCollection<TEndpoint, TDatabase, TPool> CollectAfter(int previousLastSecond, int settleSeconds = DefaultSettleSeconds)
    {
        var lastSecond = LatestCompleteSecond(settleSeconds);
        return CollectRange(previousLastSecond + 1, Math.Max(previousLastSecond, lastSecond));
    }

    int LatestCompleteSecond(int settleSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(settleSeconds, 1);
        return seconds.Second - settleSeconds;
    }

    /// <summary>
    /// Collects exactly the requested seconds, for a caller that keeps track itself of where it left off and wants
    /// to end at the same last second as an earlier <see cref="Collect"/>.
    /// The seconds are those of the timeline of the rings, like
    /// <see cref="MetricsCollection{TEndpoint,TDatabase,TPool}.LastSecond"/>.
    /// </summary>
    public MetricsCollection<TEndpoint, TDatabase, TPool> CollectRange(int firstSecond, int lastSecond)
    {
        // A window is at most LargestInterval seconds long. The ring keeps more (RingCapacity), so that the oldest
        // seconds of such a window are still there as well. Seconds from before our own start do not exist at all:
        // counting those as quiet would make the traffic right after starting seem structurally too low for whoever
        // divides by SecondsCovered.
        if (lastSecond >= seconds.Second) {
            throw new ArgumentOutOfRangeException(nameof(lastSecond), lastSecond, "Expected a completed second before the current second.");
        }

        firstSecond = Math.Max(firstSecond, lastSecond - Description.LargestInterval + 1);
        firstSecond = Math.Max(firstSecond, Math.Max(FirstObservableSecond, seconds.Second - RingCapacity + 1));

        var threadPool = threadPoolUsage.Collect(firstSecond, lastSecond);
        var connectionPools = connectionPoolUsage.Collect(firstSecond, lastSecond);
        return new() {
            LastSecond = lastSecond,
            WindowEndUtc = seconds.EndUtcOf(lastSecond),
            SecondsCovered = Math.Max(0, lastSecond - firstSecond + 1),
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
