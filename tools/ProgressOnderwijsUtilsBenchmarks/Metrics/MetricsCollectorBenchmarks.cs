using ProgressOnderwijsUtils.Metrics;

namespace ProgressOnderwijsUtils.Benchmarks.Metrics;

[MemoryDiagnoser]
//[Microsoft.VSDiagnostics.CPUUsageDiagnoser] // uncomment if you want to measure this
// ReSharper disable once ClassCanBeSealed.Global because a benchmark class may not be sealed as it is edited by the JIT
public class MetricsCollectorBenchmarks
{
    const int TotalOperations = 1 << 20;

    // Length of an up-and-down wave in number of measurements. Within a second only the first wave sets new peaks
    // (CAS writes); the following waves do not rise above that peak and only read, just like in production. Only a
    // change of second resets the peak, so how often a write happens depends on the clock. The cost of both paths
    // separately is in the ActiveRequests_Measurement_RisingPeak and _BelowPeak benchmarks.
    const int RampLength = 1 << 9;

    [Params(1, 4, 16)]
    public int ThreadCount { get; set; }

    // These fields are filled by BenchmarkDotNet in GlobalSetup, before any benchmark runs; null! keeps the compiler
    // from demanding a constructor assignment that does not fit here.
    // ReSharper disable NullableWarningSuppressionIsUsed
    ActiveRequests steadyActiveRequests = null!;
    DurationHistogramRing<BenchmarkEndpoint> steadyRequestDuration = null!;
    SecondsRingBuffer steadyRing = null!;
    ActiveRequests rollingActiveRequests = null!;
    DurationHistogramRing<BenchmarkEndpoint> rollingRequestDuration = null!;
    ActiveRequests systemActiveRequests = null!;
    DurationHistogramRing<BenchmarkEndpoint> systemRequestDuration = null!;
    KeyValuePair<string, object?>[] okTags = null!;
    KeyValuePair<string, object?>[] errorTags = null!;
    double[] durations = null!;
    Thread[] workers = null!;
    Barrier startBarrier = null!;
    Barrier endBarrier = null!;
    // ReSharper restore NullableWarningSuppressionIsUsed as we do not have a constructor here and do not want to place .AssertNotNull() everywhere

    Action<int, int>? work;
    volatile bool stopping;

    // ReSharper disable once NotAccessedField.Local as the JIT does use it
    volatile int sink;

    [GlobalSetup]
    public void Setup()
    {
        var description = BenchmarkMetrics.Description();
        var ringLength = BenchmarkMetrics.RingCapacity;

        var frozen = new FrozenClock();
        var steady = new SecondsSinceStart(frozen);
        steadyRing = new(steady, ringLength);
        steadyActiveRequests = new(new(steady, ringLength));
        steadyRequestDuration = new(new(steady, ringLength), description.RequestDurationHistogram);
        // Accelerated but real time, so that the reset path is hit regularly without the clock itself touching
        // shared state and clouding the measurement.
        var scaled = new ScaledClock(1000);
        var rolling = new SecondsSinceStart(scaled);
        rollingActiveRequests = new(new(rolling, ringLength));
        rollingRequestDuration = new(new(rolling, ringLength), description.RequestDurationHistogram);
        // The clock of production: rotation once per second and including the cost of reading the timestamp, so
        // that this benchmark shows the real overhead per request.
        var system = new SecondsSinceStart(TimeProvider.System);
        systemActiveRequests = new(new(system, ringLength));
        systemRequestDuration = new(new(system, ringLength), description.RequestDurationHistogram);
        // A tracked route, so that the measurement also includes looking up the number of the route.
        var trackedRoute = description.RequestDurationHistogram.Values[1] is { } trackedEndpoint
            ? BenchmarkEndpointLookup.EndpointOf(trackedEndpoint)?.Route
            : null;
        okTags = [new("http.request.method", "GET"), new("http.route", trackedRoute), new("http.response.status_code", 200),];
        errorTags = [new("http.request.method", "GET"), new("http.route", trackedRoute), new("http.response.status_code", 500),];
        durations = new double[16];
        for (var i = 0; i < durations.Length; i++) {
            durations[i] = (1 << i) / 1000d;
        }

        stopping = false;
        startBarrier = new(ThreadCount + 1);
        endBarrier = new(ThreadCount + 1);
        workers = new Thread[ThreadCount];
        for (var i = 0; i < ThreadCount; i++) {
            var threadIndex = i;
            workers[i] = new(() => WorkerLoop(threadIndex)) {
                IsBackground = true,
                Name = $"metrics-bench-{threadIndex}",
            };
            workers[i].Start();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        stopping = true;
        startBarrier.SignalAndWait();
        foreach (var worker in workers) {
            worker.Join();
        }

        startBarrier.Dispose();
        endBarrier.Dispose();
    }

    void WorkerLoop(int threadIndex)
    {
        while (true) {
            startBarrier.SignalAndWait();
            if (stopping) {
                return;
            }

            if (work is { } body) {
                body(threadIndex, TotalOperations / ThreadCount);
            }

            endBarrier.SignalAndWait();
        }
    }

    void Run(Action<int, int> body)
    {
        work = body;
        startBarrier.SignalAndWait();
        endBarrier.SignalAndWait();
    }

    // With the frozen clock the peak is never reset, so only a total that keeps rising sets a new peak on every
    // measurement.
    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void ActiveRequests_Measurement_RisingPeak()
        => Run(
            (_, ops) => {
                var target = steadyActiveRequests;
                for (var i = 0; i < ops; i++) {
                    target.Measurement(1);
                }
            }
        );

    // A start and an end in turn: after the first measurements the total no longer rises above the peak, so only the
    // read path. In production that is by far the most common case.
    [Benchmark(OperationsPerInvoke = TotalOperations, Baseline = true)]
    public void ActiveRequests_Measurement_BelowPeak()
        => Run(
            (_, ops) => {
                var target = steadyActiveRequests;
                for (var i = 0; i < ops; i++) {
                    target.Measurement((i & 1) == 0 ? 1 : -1);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void ActiveRequests_Measurement_WithSecondRollover()
        => Run(
            (_, ops) => {
                var target = rollingActiveRequests;
                for (var i = 0; i < ops; i++) {
                    target.Measurement((i & RampLength) == 0 ? 1 : -1);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void RequestDuration_Measurement()
        => Run(
            (_, ops) => {
                var target = steadyRequestDuration;
                var values = durations;
                var ok = okTags;
                var error = errorTags;
                for (var i = 0; i < ops; i++) {
                    target.Measurement(values[i & 15], (i & 7) == 0 ? error : ok);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void RequestDuration_Measurement_WithSecondRollover()
        => Run(
            (_, ops) => {
                var target = rollingRequestDuration;
                var values = durations;
                var ok = okTags;
                var error = errorTags;
                for (var i = 0; i < ops; i++) {
                    target.Measurement(values[i & 15], (i & 7) == 0 ? error : ok);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void ActiveRequests_Measurement_SystemClock()
        => Run(
            (_, ops) => {
                var target = systemActiveRequests;
                for (var i = 0; i < ops; i++) {
                    target.Measurement((i & RampLength) == 0 ? 1 : -1);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void RequestDuration_Measurement_SystemClock()
        => Run(
            (_, ops) => {
                var target = systemRequestDuration;
                var values = durations;
                var ok = okTags;
                var error = errorTags;
                for (var i = 0; i < ops; i++) {
                    target.Measurement(values[i & 15], (i & 7) == 0 ? error : ok);
                }
            }
        );

    [Benchmark(OperationsPerInvoke = TotalOperations)]
    public void SecondsRingBuffer_TryEnterSlot()
        => Run(
            (_, ops) => {
                var target = steadyRing;
                var local = 0;
                for (var i = 0; i < ops; i++) {
                    if (target.TryEnterSlot(null, out var index)) {
                        local += index;
                    }
                }

                sink = local;
            }
        );

    /// <summary>
    /// Stands still, so that the benchmark only shows the cost of the collector itself and not that of the clock.
    /// Not representative of production; the SystemClock benchmarks are for that.
    /// </summary>
    sealed class FrozenClock : TimeProvider
    {
        public override long GetTimestamp()
            => 0;
    }

    /// <summary>
    /// Lets virtual time run <paramref name="secondsPerRealSecond"/> times as fast as real time, so that second
    /// rotations happen often. Only reads the timestamp counter and so shares no state between threads.
    /// </summary>
    sealed class ScaledClock(long secondsPerRealSecond) : TimeProvider
    {
        public override long GetTimestamp()
            => Stopwatch.GetTimestamp() * secondsPerRealSecond;
    }
}
