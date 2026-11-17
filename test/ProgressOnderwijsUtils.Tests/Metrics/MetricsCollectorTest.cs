using System.Collections.Immutable;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProgressOnderwijsUtils.Metrics;
using ProgressOnderwijsUtils.Tests.Data;
using MetricsCollection = ProgressOnderwijsUtils.Metrics.MetricsCollection<ProgressOnderwijsUtils.Tests.Metrics.TestEndpoint, ProgressOnderwijsUtils.Tests.Metrics.TestDatabase, ProgressOnderwijsUtils.Tests.Metrics.TestPool>;
using MetricsCollector = ProgressOnderwijsUtils.Metrics.MetricsCollector<ProgressOnderwijsUtils.Tests.Metrics.TestEndpoint, ProgressOnderwijsUtils.Tests.Metrics.TestDatabase, ProgressOnderwijsUtils.Tests.Metrics.TestPool>;

namespace ProgressOnderwijsUtils.Tests.Metrics;

/// <summary>
/// Sums over the histogram cells that only the tests need; the production code always reads
/// a single cell or a single bucket.
/// </summary>
static class MetricsCollectionTestExtensions
{
    extension(MetricsCollection collection)
    {
        /// <summary>
        /// The total number of measurements of an endpoint slot, for either successful or failed requests.
        /// </summary>
        public int TotalRequestDurationCount(int endpointSlot, bool isError)
        {
            var total = 0;
            for (var bucketIndex = 0; bucketIndex < collection.Description.RequestDurationHistogram.Histogram.BucketCount; bucketIndex++) {
                total += collection.RequestDurationCount(endpointSlot, bucketIndex, isError);
            }

            return total;
        }

        /// <summary>
        /// The total number of measurements, for either successful or failed requests.
        /// </summary>
        public int TotalRequestDurationCount(bool isError)
            => collection.RequestDurationHistogram
                .Where((_, cellIndex) => collection.Description.RequestDurationHistogram.IsFailed(cellIndex) == isError)
                .Sum();
    }
}

[Collection(nameof(MetricsCollectorCollection))]
public sealed class MetricsCollectorTest
{
    /// <summary>
    /// The collector derives its division into seconds from the timestamp of the clock, so with a clock that we move
    /// by whole seconds ourselves, all measurements predictably land in the slot we intend. The timestamp starts at 0
    /// and runs in step with real time, unless a test lets it run ahead or lag with <see cref="TimestampAhead"/>.
    /// </summary>
    sealed class MutableClock : TimeProvider
    {
        static readonly DateTimeOffset start = DateTimeOffset.UnixEpoch.AddDays(20_000);
        DateTimeOffset now = start;

        public override DateTimeOffset GetUtcNow()
            => now;

        public override long GetTimestamp()
            => (now - start + TimestampAhead).Ticks;

        public override long TimestampFrequency
            => TimeSpan.TicksPerSecond;

        /// <summary>
        /// How far the timestamp runs ahead of real time; negative when it lags, as a drifting counter does over time.
        /// </summary>
        public TimeSpan TimestampAhead { get; set; }

        /// <summary>
        /// The current second of a timeline that was started before this clock was first advanced, as long as the
        /// timestamp runs in step.
        /// </summary>
        public int Second
            => checked((int)((now - start).Ticks / TimeSpan.TicksPerSecond));

        /// <summary>
        /// The real time at which that second of the timeline ended, as long as the timestamp runs in step.
        /// </summary>
        public static DateTime EndUtcOf(int second)
            => start.UtcDateTime.AddSeconds(second + 1);

        public void Advance(int seconds)
            => now = now.AddSeconds(seconds);
    }

    /// <summary>
    /// A meter of our own under the name the collector listens to, created through the meter factory of a host, so
    /// that the test comes in along the same path as the real ASP.NET instrumentation.
    /// </summary>
    sealed class Harness : IDisposable
    {
        readonly ServiceProvider host = TestMetrics.HostServices();
        readonly UpDownCounter<long> activeRequests;
        readonly Histogram<double> requestDuration;

        public Harness()
        {
            var hostMeters = host.GetRequiredService<IMeterFactory>();
            var meter = hostMeters.Create("Microsoft.AspNetCore.Hosting");
            activeRequests = meter.CreateUpDownCounter<long>("http.server.active_requests");
            requestDuration = meter.CreateHistogram<double>("http.server.request.duration");
            Collector = TestMetrics.Collector(Clock);
            Collector.Start(hostMeters, samplingFailures.Enqueue);
        }

        readonly ConcurrentQueue<Exception> samplingFailures = new();

        public MutableClock Clock { get; } = new();
        public MetricsCollector Collector { get; }

        /// <summary>
        /// The endpoints that have a slot of their own; slot 0 is the other slot and is not one of them.
        /// </summary>
        public ImmutableArray<TestEndpoint> TrackedEndpoints
            => [
                .. Enumerable
                    .Range(1, Collector.Description.RequestDurationHistogram.Values.Length - 1)
                    .Select(slot => Collector.Description.RequestDurationHistogram.Values[slot].AssertNotNull()),
            ];

        /// <summary>
        /// An endpoint that is tracked, so that the test does not break as soon as the selection in
        /// <see cref="TestRequestDuration"/> changes.
        /// </summary>
        public (string Method, string Route) FirstTrackedEndpoint()
            => TestRequestDuration.EndpointOf(TrackedEndpoints[0]);

        public void RequestStarted()
            => activeRequests.Add(1);

        public void RequestFinished()
            => activeRequests.Add(-1);

        public void Request(double seconds, int statusCode = 200)
            => requestDuration.Record(seconds, new KeyValuePair<string, object?>("http.response.status_code", statusCode));

        /// <summary>
        /// A measurement with the route tags that ASP.NET supplies once the router has found an endpoint.
        /// </summary>
        public void RouteRequest(double seconds, string method, string route, int statusCode = 200)
            => requestDuration.Record(
                seconds,
                new("http.response.status_code", statusCode),
                new("http.request.method", method),
                new("http.route", route)
            );

        /// <summary>
        /// Stops the instrumentation while the collector stays alive, as when the application shuts
        /// down with requests still running. A meter of a factory is only disposed together with that factory.
        /// </summary>
        public void StopInstrumentation()
            => host.Dispose();

        public void Dispose()
        {
            Collector.Dispose();
            host.Dispose();
            PAssert.That(() => samplingFailures.IsEmpty);
        }
    }

    /// <summary>
    /// A collector stopped early is disposed again when the host stops; the harness disposes it a third time.
    /// </summary>
    [Fact]
    public void A_started_collector_can_be_disposed_more_than_once()
    {
        using var harness = new Harness();

        harness.Collector.Dispose();
        harness.Collector.Dispose();
    }

    /// <summary>
    /// A collector that has already been disposed must not start anymore: its stop signal is gone, so a
    /// sampler would immediately report a failure, and the listeners would stay hooked up forever.
    /// </summary>
    [Fact]
    public void A_disposed_collector_does_not_start_again()
    {
        var failures = new ConcurrentQueue<Exception>();
        var collector = TestMetrics.Collector(new MutableClock());
        collector.Dispose();

        collector.Start(TestMetrics.HostWithoutRequests, failures.Enqueue);
        collector.Start(TestMetrics.HostWithoutRequests, failures.Enqueue);
        // Without the guard the sampler fails right away on the disposed signal.
        Thread.Sleep(TimeSpan.FromMilliseconds(200));
        collector.Dispose();

        PAssert.That(() => failures.IsEmpty);
    }

    /// <summary>
    /// When another host has already published its ASP.NET Core instruments, the start is rejected. The collector must
    /// then release its listener instead of keeping it hooked up for the rest of the process, and stays disposed.
    /// </summary>
    [Fact]
    public void A_start_rejected_because_of_another_host_releases_the_listener_and_leaves_the_collector_disposed()
    {
        using var otherHost = TestMetrics.HostServices();
        var otherMeter = otherHost.GetRequiredService<IMeterFactory>().Create("Microsoft.AspNetCore.Hosting");
        _ = otherMeter.CreateHistogram<double>("http.server.request.duration");
        using var ownHost = TestMetrics.HostServices();
        var ownMeters = ownHost.GetRequiredService<IMeterFactory>();
        var failures = new ConcurrentQueue<Exception>();
        var collector = TestMetrics.Collector(new MutableClock());

        _ = Assert.Throws<NotSupportedException>(() => collector.Start(ownMeters, failures.Enqueue));

        // A listener that was still hooked up would reject this instrument of the other host as well.
        _ = otherMeter.CreateUpDownCounter<long>("http.server.active_requests");

        collector.Start(ownMeters, failures.Enqueue);
        // A sampler started on the second attempt would fail right away on the disposed signal.
        Thread.Sleep(TimeSpan.FromMilliseconds(200));
        collector.Dispose();

        PAssert.That(() => failures.IsEmpty);
    }

    /// <summary>
    /// The whole path an application uses: the OpenTelemetry SqlClient instrumentation that
    /// <see cref="MetricsServiceCollectionExtensions.AddProgressOnderwijsMetrics{TEndpoint,TDatabase,TPool}"/> enables,
    /// against a real database. A different meter or instrument name, unit or tag would otherwise silently leave
    /// the database histograms empty, or count the measurements under the wrong database or outcome.
    /// </summary>
    [Fact]
    public async Task Sql_commands_are_counted_per_database_in_the_bucket_of_their_duration_and_outcome()
    {
        var clock = new MutableClock();
        var databaseDuration = new TestDatabaseDuration(catalog => catalog == "tempdb" ? TestDatabase.Alpha : null);
        var collector = new MetricsCollector(TestMetrics.Description(databaseDuration), TestMetrics.Registry(), clock);
        var samplingFailures = new ConcurrentQueue<Exception>();
        var builder = Host.CreateEmptyApplicationBuilder(new());
        _ = builder.Services.AddProgressOnderwijsMetrics(collector, samplingFailures.Enqueue);
        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        var connectionString = new SqlConnectionStringBuilder(TransactedLocalConnection.ConnectionString) { InitialCatalog = "tempdb", }.ConnectionString;
        await using (var connection = new SqlConnection(connectionString)) {
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            foreach (var sql in new[] { "select 1", "select 1", "waitfor delay '00:00:00.300'", }) {
                await using var command = new SqlCommand(sql, connection);
                _ = await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
            }

            await using var failing = new SqlCommand("select 1/0", connection);
            _ = await Assert.ThrowsAsync<SqlException>(() => failing.ExecuteNonQueryAsync(TestContext.Current.CancellationToken));
        }

        clock.Advance(2);
        var collection = collector.Collect(1, minimumWindow: 1);
        await host.StopAsync(TestContext.Current.CancellationToken);

        var histogram = collection.Description.DatabaseDurationHistogram;
        var alpha = histogram.SlotOf(TestDatabase.Alpha);
        var succeeded = collection.DatabaseDurationCounts(alpha, false);
        var failed = collection.DatabaseDurationCounts(alpha, true);
        var other = collection.DatabaseDurationCounts(MetricsDimension<TestDatabase>.OtherSlot, false).Sum() + collection.DatabaseDurationCounts(MetricsDimension<TestDatabase>.OtherSlot, true).Sum();

        // db.namespace names the database, error.type marks the failure.
        PAssert.That(() => succeeded.Sum() == 3 && failed.Sum() == 1 && other == 0);

        // The duration is in seconds: the two quick selects stay below 0.3 s, the delay lands between 0.3 s and 2 s.
        var delayBucket = histogram.Histogram.GetBucketIndex(0.3);
        var quick = succeeded[..delayBucket].Sum();
        var delayed = succeeded[delayBucket..(histogram.Histogram.GetBucketIndex(2.0) + 1)].Sum();
        PAssert.That(() => quick == 2 && delayed == 1);
        PAssert.That(() => samplingFailures.IsEmpty);
    }

    [Fact]
    public void Requests_are_counted_in_the_bucket_that_matches_their_duration()
    {
        using var harness = new Harness();

        harness.Request(0.001);
        harness.Request(0.005);
        harness.Request(0.005);
        harness.Request(20.0);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);
        var overflowBucket = collection.Description.RequestDurationHistogram.Histogram.Labels.Length - 1;

        // [0 - 4) ms, [4 - 8) ms and the overflow bucket >= 16.384 s.
        var requestDurationCount = collection.RequestDurationCount(0, false);
        PAssert.That(() => requestDurationCount == 1);
        PAssert.That(() => collection.RequestDurationCount(1, false) == 2);
        PAssert.That(() => collection.RequestDurationCount(overflowBucket, false) == 1);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == 4);
    }

    [Fact]
    public void Requests_with_a_failing_status_code_are_counted_separately()
    {
        using var harness = new Harness();

        harness.Request(0.001);
        harness.Request(0.001, 404);
        harness.Request(0.001, 500);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.RequestDurationCount(0, false) == 1);
        PAssert.That(() => collection.RequestDurationCount(0, true) == 2);
    }

    [Fact]
    public void A_tracked_endpoint_gets_its_own_histogram()
    {
        using var harness = new Harness();
        var tracked = harness.FirstTrackedEndpoint();

        harness.RouteRequest(0.001, tracked.Method, tracked.Route);
        harness.RouteRequest(0.001, tracked.Method, tracked.Route, 500);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);
        var endpointSlot = collection.Description.RequestDurationHistogram.SlotOf(harness.TrackedEndpoints[0]);

        PAssert.That(() => collection.RequestDurationCount(endpointSlot, 0, false) == 1);
        PAssert.That(() => collection.RequestDurationCount(endpointSlot, 0, true) == 1);
        PAssert.That(() => collection.TotalRequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, false) == 0);
    }

    /// <summary>
    /// Every host creates its own ASP.NET Core instruments, and the listener sees those of all hosts in the process.
    /// Measuring more than one host is not supported, so the instruments of a second host are rejected where they are
    /// created, and the requests of the measured host are still counted.
    /// </summary>
    [Fact]
    public void A_second_host_in_the_same_process_is_rejected()
    {
        using var harness = new Harness();
        using var otherHost = TestMetrics.HostServices();
        var otherMeter = otherHost.GetRequiredService<IMeterFactory>().Create("Microsoft.AspNetCore.Hosting");

        _ = Assert.Throws<NotSupportedException>(() => otherMeter.CreateUpDownCounter<long>("http.server.active_requests"));
        _ = Assert.Throws<NotSupportedException>(() => otherMeter.CreateHistogram<double>("http.server.request.duration"));

        harness.RequestStarted();
        harness.Request(0.001);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.TotalRequestDurationCount(false) == 1);
        PAssert.That(() => collection.MaxActiveRequests == 1);
    }

    [Fact]
    public void An_untracked_endpoint_is_counted_with_the_other_endpoints()
    {
        using var harness = new Harness();

        harness.RouteRequest(0.001, "GET", "whoami");
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.TotalRequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, false) == 1);
    }

    [Fact]
    public void A_request_without_a_route_is_counted_with_the_other_endpoints()
    {
        using var harness = new Harness();

        // Without a route tag the router has not found an endpoint; that measurement must not be lost
        // and therefore ends up in the other slot.
        harness.Request(0.001);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.TotalRequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, false) == 1);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == 1);
    }

    [Fact]
    public void An_unnumbered_route_is_counted_with_the_other_endpoints()
    {
        using var harness = new Harness();

        // A route that is not in the numbering has no slot of its own, so it is counted in the
        // other slot.
        harness.RouteRequest(0.001, "GET", "deze/route/bestaat/niet");
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.TotalRequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, false) == 1);
    }

    [Fact]
    public void Measurements_of_different_endpoints_are_kept_apart_but_add_up_to_the_total()
    {
        using var harness = new Harness();
        var tracked = harness.FirstTrackedEndpoint();

        harness.RouteRequest(0.001, tracked.Method, tracked.Route);
        harness.RouteRequest(0.001, tracked.Method, tracked.Route);
        harness.RouteRequest(0.001, "GET", "whoami");
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);
        var endpointSlot = collection.Description.RequestDurationHistogram.SlotOf(harness.TrackedEndpoints[0]);

        PAssert.That(() => collection.TotalRequestDurationCount(endpointSlot, false) == 2);
        PAssert.That(() => collection.TotalRequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, false) == 1);
        PAssert.That(() => collection.RequestDurationCount(0, false) == 3);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == 3);
    }

    [Fact]
    public void A_bucket_of_one_endpoint_holds_only_that_endpoint_while_the_buckets_add_up_to_the_total()
    {
        // The chart shows per bucket either a single endpoint slot or the total; that total must be
        // exactly the sum over the slots, otherwise a slot is counted twice or not at all.
        using var harness = new Harness();
        var tracked = harness.FirstTrackedEndpoint();

        harness.RouteRequest(0.001, tracked.Method, tracked.Route);
        harness.RouteRequest(0.1, tracked.Method, tracked.Route);
        harness.RouteRequest(0.001, "GET", "whoami");
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);
        var endpointSlot = collection.Description.RequestDurationHistogram.SlotOf(harness.TrackedEndpoints[0]);

        PAssert.That(() => collection.RequestDurationCount(endpointSlot, 0, false) == 1);
        PAssert.That(() => collection.RequestDurationCount(MetricsDimension<TestEndpoint>.OtherSlot, 0, false) == 1);

        var everyBucketAddsUpToTheTotal = Enumerable
            .Range(0, collection.Description.RequestDurationHistogram.Histogram.Labels.Length)
            .All(
                bucketIndex => Enumerable
                        .Range(0, collection.Description.RequestDurationHistogram.Values.Length)
                        .Sum(slot => collection.RequestDurationCount(slot, bucketIndex, false))
                    == collection.RequestDurationCount(bucketIndex, false)
            );

        PAssert.That(() => everyBucketAddsUpToTheTotal);

        var totalCount = collection.TotalRequestDurationCount(false);
        PAssert.That(() => totalCount == 3);
    }

    [Fact]
    public void The_bucket_counts_of_a_collection_match_its_single_buckets()
    {
        using var harness = new Harness();
        var tracked = harness.FirstTrackedEndpoint();

        harness.RouteRequest(0.001, tracked.Method, tracked.Route);
        harness.RouteRequest(0.1, tracked.Method, tracked.Route, 500);
        harness.RouteRequest(0.01, "GET", "whoami");
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);
        var endpointSlot = collection.Description.RequestDurationHistogram.SlotOf(harness.TrackedEndpoints[0]);
        var bucketIndexes = Enumerable.Range(0, collection.Description.RequestDurationHistogram.Histogram.BucketCount).ToArray();

        foreach (var isError in new[] { false, true, }) {
            var ofEndpoint = collection.RequestDurationCounts(endpointSlot, isError);
            var ofAll = collection.RequestDurationCounts(isError);
            PAssert.That(() => ofEndpoint.SequenceEqual(bucketIndexes.Select(bucketIndex => collection.RequestDurationCount(endpointSlot, bucketIndex, isError))));
            PAssert.That(() => ofAll.SequenceEqual(bucketIndexes.Select(bucketIndex => collection.RequestDurationCount(bucketIndex, isError))));
        }

        PAssert.That(() => collection.RequestDurationCounts(false).Sum() == 2);
        PAssert.That(() => collection.RequestDurationCounts(true).Sum() == 1);
    }

    [Fact]
    public void Collecting_a_range_returns_exactly_the_requested_seconds()
    {
        using var harness = new Harness();
        var firstSecond = harness.Clock.Second;

        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Clock.Advance(3);

        var collection = harness.Collector.CollectRange(firstSecond + 1, firstSecond + 1);

        PAssert.That(() => collection.WindowEndUtc == MutableClock.EndUtcOf(firstSecond + 1));
        PAssert.That(() => collection.SecondsCovered == 1);
        PAssert.That(() => collection.RequestDurationCounts(false).Sum() == 2);
    }

    [Fact]
    public void Measurements_are_attributed_to_the_second_in_which_they_were_recorded()
    {
        using var harness = new Harness();

        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Request(0.001);
        harness.Clock.Advance(2);

        // The last completed second is the second with two measurements.
        var lastSecond = harness.Collector.Collect(1, minimumWindow: 1);
        PAssert.That(() => lastSecond.TotalRequestDurationCount(false) == 2);

        // Over both seconds together they all count, so nothing ended up in the wrong slot.
        var both = harness.Collector.Collect(2, minimumWindow: 1);
        PAssert.That(() => both.SecondsCovered == 2);
        PAssert.That(() => both.TotalRequestDurationCount(false) == 3);
    }

    [Fact]
    public void The_current_and_settling_seconds_are_never_collected()
    {
        using var harness = new Harness();

        harness.Request(0.001);

        // Without the clock advancing, the measurement lies in the current second, which is not yet complete.
        var immediately = harness.Collector.Collect(5);
        PAssert.That(() => immediately.TotalRequestDurationCount(false) == 0);

        // A second later it still falls within the settling window of two seconds.
        harness.Clock.Advance(1);
        var afterOneSecond = harness.Collector.Collect(5);
        PAssert.That(() => afterOneSecond.TotalRequestDurationCount(false) == 0);

        harness.Clock.Advance(1);
        var afterTwoSeconds = harness.Collector.Collect(5);
        PAssert.That(() => afterTwoSeconds.TotalRequestDurationCount(false) == 1);
    }

    [Fact]
    public void Seconds_without_requests_count_as_zero()
    {
        using var harness = new Harness();

        harness.Request(0.001);
        harness.Clock.Advance(10);

        var collection = harness.Collector.Collect(5);

        // The measurement lies outside the window and the quiet seconds yield nothing.
        PAssert.That(() => collection.SecondsCovered == 5);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == 0);
    }

    [Fact]
    public void A_smaller_interval_is_widened_to_the_minimum_window()
    {
        using var harness = new Harness();

        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 3);

        // The measurement spans more seconds than the requested interval.
        PAssert.That(() => collection.SecondsCovered == 3);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == 3);
    }

    [Fact]
    public void A_window_reaching_back_before_the_collector_started_is_clamped()
    {
        using var harness = new Harness();

        harness.Clock.Advance(5);
        var collection = harness.Collector.Collect(60);

        // The window reaches back no further than the time that has elapsed since the start.
        PAssert.That(() => collection.SecondsCovered == 4);
        PAssert.That(() => collection.WindowEndUtc == MutableClock.EndUtcOf(harness.Clock.Second - 2));
    }

    [Fact]
    public void An_interval_outside_the_description_is_rejected()
    {
        using var harness = new Harness();
        var collector = harness.Collector;

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => collector.Collect(0));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => collector.Collect(collector.Description.LargestInterval + 1));
    }

    [Fact]
    public void The_peak_of_active_requests_is_reported_not_the_final_value()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.RequestStarted();
        harness.RequestStarted();
        harness.RequestFinished();
        harness.RequestFinished();
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.MaxActiveRequests == 3);
    }

    [Fact]
    public void Stopping_instrumentation_does_not_erase_the_peak_of_an_earlier_window()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.RequestStarted();
        harness.RequestStarted();

        // A second without measurements: it has no peak of its own and must therefore be derived
        // from the starting level of a later second.
        harness.Clock.Advance(1);

        // The application stops with three requests running: the corresponding -1 measurements never arrive.
        harness.Clock.Advance(1);
        harness.StopInstrumentation();

        // The window from before the stop should still show the peak of that time.
        var collection = harness.Collector.Collect(1, minimumWindow: 1, settleSeconds: 1);

        PAssert.That(() => collection.MaxActiveRequests == 3);
    }

    [Fact]
    public void A_second_that_only_sees_requests_finish_starts_at_the_running_total()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.RequestStarted();
        harness.RequestStarted();
        harness.Clock.Advance(1);

        // In this second a request only finishes; the peak is then the level the second started with.
        harness.RequestFinished();
        harness.Clock.Advance(2);

        var collection = harness.Collector.Collect(1, minimumWindow: 1);

        PAssert.That(() => collection.MaxActiveRequests == 3);
    }

    [Fact]
    public void A_window_without_any_measurement_falls_back_to_the_running_total()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.RequestStarted();

        // The gauge does not move after that, so the quiet seconds contain no peak. Yet two requests
        // are still running and that is what the window describes.
        harness.Clock.Advance(30);

        var collection = harness.Collector.Collect(5);

        PAssert.That(() => collection.MaxActiveRequests == 2);
    }

    [Fact]
    public void Requests_that_finish_after_the_window_do_not_empty_that_window()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.RequestStarted();
        harness.RequestStarted();

        // The requests run through the whole window without the gauge moving again.
        harness.Clock.Advance(10);

        // Only during the settling time, so after the window, do all three finish.
        harness.RequestFinished();
        harness.RequestFinished();
        harness.RequestFinished();

        var collection = harness.Collector.Collect(5);

        // The window itself had three running requests; that they have finished since does not belong in it.
        PAssert.That(() => collection.MaxActiveRequests == 3);
    }

    [Fact]
    public void Requests_that_start_after_the_window_are_not_counted_in_it()
    {
        using var harness = new Harness();

        harness.RequestStarted();
        harness.Clock.Advance(10);

        // After the window a flood of new requests comes in.
        for (var i = 0; i < 20; i++) {
            harness.RequestStarted();
        }

        var collection = harness.Collector.Collect(5);

        PAssert.That(() => collection.MaxActiveRequests == 1);
    }

    [Fact]
    public void Collecting_after_a_cursor_neither_skips_nor_repeats_a_second()
    {
        using var harness = new Harness();

        var start = harness.Collector.Collect(1, minimumWindow: 1);
        var cursor = start.LastSecond;

        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Request(0.001);
        harness.Clock.Advance(2);

        var first = harness.Collector.CollectAfter(cursor);

        // Everything up to and including the last completed second is drained, so the quiet seconds
        // between the last measurement and now are included too. The cursor points to a second from
        // before the start, so the window begins at the first second we could actually see.
        PAssert.That(() => first.WindowEndUtc == MutableClock.EndUtcOf(harness.Clock.Second - 2));
        PAssert.That(() => first.SecondsCovered == 3);
        PAssert.That(() => first.TotalRequestDurationCount(false) == 3);

        // Reading again immediately yields no completed seconds, so nothing is counted twice.
        var immediately = harness.Collector.CollectAfter(first.LastSecond);
        PAssert.That(() => immediately.SecondsCovered == 0);
        PAssert.That(() => immediately.TotalRequestDurationCount(false) == 0);

        harness.Clock.Advance(1);
        harness.Request(0.001);
        harness.Clock.Advance(2);

        var second = harness.Collector.CollectAfter(immediately.LastSecond);
        PAssert.That(() => second.TotalRequestDurationCount(false) == 1);
    }

    [Fact]
    public void Collecting_after_a_stale_cursor_reports_how_much_it_could_recover()
    {
        using var harness = new Harness();

        var cursor = harness.Collector.Collect(1, minimumWindow: 1).LastSecond;

        // The caller stays away much longer than the ring is long.
        harness.Clock.Advance(500);

        var collection = harness.Collector.CollectAfter(cursor);

        // SecondsCovered shows that there is a gap between the cursor and what was still available.
        PAssert.That(() => collection.SecondsCovered == harness.Collector.Description.LargestInterval);
        PAssert.That(() => collection.LastSecond - cursor > collection.SecondsCovered);
    }

    /// <summary>
    /// The reset callback runs exactly in the window where a writer has already claimed the slot
    /// but not yet released it: the data has then been cleared while the release stamp still names
    /// the old second. A reader that only looks at that release would accept a cleared slot as a
    /// valid measurement, so HoldsSecond must return false here.
    /// </summary>
    [Fact]
    public void A_slot_that_is_being_cleared_no_longer_holds_its_previous_second()
    {
        var clock = new MutableClock();
        var ring = new SecondsRingBuffer(new SecondsSinceStart(clock), 64);

        var claimed = ring.TryEnterSlot(null, out _);
        PAssert.That(() => claimed);

        var previousSecond = clock.Second;
        var index = ring.IndexOf(previousSecond);
        PAssert.That(() => ring.HoldsSecond(previousSecond, index));

        // Exactly one lap further, so the same index is reused for a new second.
        clock.Advance(64);
        var currentSecond = clock.Second;

        var holdsPreviousDuringReset = true;
        var entered = ring.TryEnterSlot(_ => holdsPreviousDuringReset = ring.HoldsSecond(previousSecond, index), out var resetIndex);

        PAssert.That(() => entered);
        PAssert.That(() => resetIndex == index);
        PAssert.That(() => !holdsPreviousDuringReset);

        // As soon as the writer releases, the slot indisputably belongs to the new second.
        PAssert.That(() => ring.HoldsSecond(currentSecond, index));
        PAssert.That(() => !ring.HoldsSecond(previousSecond, index));
    }

    /// <summary>
    /// An unused slot carries the empty stamp -1. Whoever looks back from second 0 asks for second -1, and must not
    /// take that empty slot for a recorded second.
    /// </summary>
    [Fact]
    public void An_unused_slot_does_not_hold_the_second_before_the_start()
    {
        var ring = new SecondsRingBuffer(new SecondsSinceStart(new MutableClock()), 64);

        PAssert.That(() => !ring.HoldsSecond(-1, ring.IndexOf(-1)));
    }

    /// <summary>
    /// A writer that is paused in the middle of its reset for a whole rotation, while another writer claims the same
    /// slot for the second one rotation later. When the first writer then finishes, its release must not move the
    /// slot back to its own, older second: the writers of the newer second would wait for a release that never comes.
    /// </summary>
    [Fact]
    public async Task A_reset_that_finishes_after_a_newer_claim_does_not_block_the_newer_second()
    {
        var clock = new MutableClock();
        var ring = new SecondsRingBuffer(new SecondsSinceStart(clock), 64);

        // The first writer claims second 0; during its reset a full rotation passes and second 64 claims the slot.
        var newerClaimed = false;
        _ = ring.TryEnterSlot(_reset => {
            clock.Advance(64);
            newerClaimed = ring.TryEnterSlot(null, out _);
        }, out _);
        PAssert.That(() => newerClaimed);

        // Another writer in second 64.
        var next = Task.Run(() => ring.TryEnterSlot(null, out _), TestContext.Current.CancellationToken);
        bool finished;
        try {
            finished = await next.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        } finally {
            // Let a waiting writer go even when the first wait times out or is cancelled.
            clock.Advance(64);
            _ = ring.TryEnterSlot(null, out _);
            _ = await next.WaitAsync(TimeSpan.FromSeconds(10), CancellationToken.None);
        }

        PAssert.That(() => finished);
    }

    [Fact]
    public void A_smallest_interval_below_one_second_or_above_the_largest_is_rejected()
    {
        foreach (var smallestInterval in new[] { 0, -1, 129, }) {
            var description = new MetricsDescription<TestEndpoint, TestDatabase, TestPool> {
                SmallestInterval = smallestInterval,
                LargestInterval = 128,
                DatabaseDurationHistogram = new TestDatabaseDuration(),
                RequestDurationHistogram = new TestRequestDuration(),
            };

            _ = Assert.Throws<InvalidOperationException>(() => new MetricsCollector(description, TestMetrics.Registry()));
        }
    }

    [Fact]
    public void A_ring_length_that_is_not_a_power_of_two_is_rejected()
        => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new SecondsRingBuffer(new SecondsSinceStart(new MutableClock()), 60));

    /// <summary>
    /// Collecting skips the most recent seconds, so the largest window starts that many seconds before
    /// what a ring of exactly that length still holds. The ring is therefore longer than the largest
    /// window; otherwise the oldest seconds count as quiet while SecondsCovered does include them.
    /// </summary>
    [Fact]
    public void A_window_of_the_largest_interval_still_holds_its_oldest_seconds()
    {
        using var harness = new Harness();
        var largestInterval = harness.Collector.Description.LargestInterval;

        // Longer than the ring, so that every slot has been reused at least once.
        for (var second = 0; second < MetricsCollector.RingCapacity * 2; second++) {
            harness.Clock.Advance(1);
            harness.Request(0.01);
        }

        var collection = harness.Collector.Collect(largestInterval);

        PAssert.That(() => collection.SecondsCovered == largestInterval);
        PAssert.That(() => collection.TotalRequestDurationCount(false) == largestInterval);
    }

    /// <summary>
    /// The increase of a counter over a window is calculated from the level just before that window. For
    /// the largest window too, the ring must still have that level; otherwise the increase within the
    /// first second of the window is lost.
    /// </summary>
    [Fact]
    public void Counters_keep_their_baseline_for_a_window_of_the_largest_interval()
    {
        var clock = new MutableClock();
        using var collector = TestMetrics.Collector(clock);
        var largestInterval = collector.Description.LargestInterval;
        var ring = new SampledRing(new(new SecondsSinceStart(clock), MetricsCollector.RingCapacity), 1, 1, 1);

        // The counter goes up by one every second, for longer than the ring is long.
        for (var count = 1; count <= MetricsCollector.RingCapacity * 2; count++) {
            clock.Advance(1);
            ring.Record([0L,], [count,]);
        }

        var lastSecond = clock.Second - MetricsCollector.DefaultSettleSeconds;
        var window = ring.Collect(lastSecond - largestInterval + 1, lastSecond).AssertNotNull();

        PAssert.That(() => window.CounterIncreases[0] == largestInterval);
    }

    /// <summary>
    /// Whatever the timestamp of the clock is, second 0 begins at the start of the collector, so the first window
    /// counts that second completely.
    /// </summary>
    [Fact]
    public void The_timeline_starts_at_second_0_when_the_collector_starts()
    {
        var clock = new MutableClock();
        clock.Advance(100);
        var startedUtc = clock.GetUtcNow().UtcDateTime;
        using var collector = TestMetrics.Collector(clock);
        clock.Advance(5);

        var collection = collector.Collect(60);

        PAssert.That(() => collection.LastSecond == MetricsCollector.FirstObservableSecond + 5 - MetricsCollector.DefaultSettleSeconds);
        PAssert.That(() => collection.SecondsCovered == collection.LastSecond - MetricsCollector.FirstObservableSecond + 1);
        PAssert.That(() => collection.WindowEndUtc == startedUtc.AddSeconds(collection.LastSecond + 1));
    }

    /// <summary>
    /// The timeline drifts away from real time. A collection follows the timeline for its seconds, but reports the
    /// real time at which its window ended, to the millisecond.
    /// </summary>
    [Fact]
    public void A_window_ends_at_the_real_time_its_last_second_ended_when_the_timeline_lags()
    {
        var clock = new MutableClock();
        using var collector = TestMetrics.Collector(clock);
        clock.Advance(10);

        var inStep = collector.Collect(1);
        clock.TimestampAhead = TimeSpan.FromMilliseconds(-2500);
        var lagging = collector.Collect(1);

        PAssert.That(() => inStep.WindowEndUtc == MutableClock.EndUtcOf(inStep.LastSecond));
        // The timeline has counted two and a half seconds fewer, so its last settled second is an earlier one, which
        // ended two and a half seconds later in real time than it would have in step.
        PAssert.That(() => lagging.LastSecond == inStep.LastSecond - 3);
        PAssert.That(() => lagging.WindowEndUtc == MutableClock.EndUtcOf(lagging.LastSecond).AddMilliseconds(2500));
    }

    /// <summary>
    /// Two collections that follow each other on the timeline also follow each other in real time, apart by exactly
    /// the drift of the timeline in between. Rounding to whole seconds would instead let them overlap or leave a gap
    /// of a whole second.
    /// </summary>
    [Fact]
    public void Consecutive_windows_are_apart_in_real_time_by_exactly_the_drift_in_between()
    {
        var clock = new MutableClock();
        using var collector = TestMetrics.Collector(clock);
        clock.Advance(20);
        var first = collector.CollectAfter(-1);

        clock.Advance(20);
        clock.TimestampAhead = TimeSpan.FromMilliseconds(1);
        var second = collector.CollectAfter(first.LastSecond);

        var secondStartUtc = second.WindowEndUtc.AddSeconds(-second.SecondsCovered);
        PAssert.That(() => secondStartUtc == first.WindowEndUtc.AddMilliseconds(-1));
    }

    /// <summary>
    /// Sampling runs on a thread of its own, and an exception that escapes there brings down the whole
    /// process. It therefore stops and passes the exception on to whoever started it.
    /// </summary>
    [Fact]
    public void An_exception_while_sampling_does_not_escape_the_sampler_thread()
    {
        using var stop = new ManualResetEventSlim();
        var failure = new InvalidOperationException("Sampling went wrong.");
        var samples = 0;
        Exception? reported = null;

        MetricsSampler.SampleUntilStopped(
            stop,
            TimeSpan.FromMilliseconds(1),
            () => {
                samples++;
                throw failure;
            },
            exception => reported = exception
        );

        PAssert.That(() => samples == 1);
        PAssert.That(() => reported == failure);
    }

    /// <summary>
    /// The thread pool measurement reads the actual runtime, so the exact numbers are not fixed.
    /// A sampled window must nevertheless yield plausible levels: at least one thread is running,
    /// and an increase can never be negative.
    /// </summary>
    [Fact]
    public void A_sampled_window_reports_plausible_thread_pool_values()
    {
        var clock = new MutableClock();
        var usage = new ThreadPoolUsage(new(new SecondsSinceStart(clock), 64));

        var firstSecond = clock.Second;
        usage.Measurement();
        clock.Advance(1);
        usage.Measurement();
        var lastSecond = clock.Second;

        var window = usage.Collect(firstSecond, lastSecond).AssertNotNull();
        PAssert.That(() => window.MaxThreadCount > 0);
        PAssert.That(() => window.MinThreadCount > 0);
        PAssert.That(() => window.MaxPendingWorkItems >= 0);
        PAssert.That(() => window.CompletedWorkItems >= 0);
        PAssert.That(() => window.LockContentions >= 0);
    }

    /// <summary>
    /// A window in which no sample was taken must not pretend to have a measurement; otherwise an empty
    /// or reused slot would pass for a valid thread pool level.
    /// </summary>
    [Fact]
    public void A_window_without_a_thread_pool_sample_reports_nothing()
    {
        var clock = new MutableClock();
        var usage = new ThreadPoolUsage(new(new SecondsSinceStart(clock), 64));

        usage.Measurement();
        var sampledSecond = clock.Second;

        clock.Advance(1);
        var unsampledSecond = clock.Second;

        var unsampled = usage.Collect(unsampledSecond, unsampledSecond);
        PAssert.That(() => unsampled == null);
        PAssert.That(() => usage.Collect(sampledSecond, sampledSecond).AssertNotNull().MaxThreadCount > 0);

        // Exactly one lap after the sampled second, the same slot comes up again.
        clock.Advance(63);
        usage.Measurement();
        var recycled = usage.Collect(sampledSecond, sampledSecond);
        PAssert.That(() => recycled == null);
    }

    /// <summary>
    /// A slot is released before sampling writes anything into it. Whoever reads at that moment already
    /// sees the second in place but still the zeros from clearing; those must not count as a level,
    /// because then the counter drops back to zero and the increase over the window becomes negative.
    /// </summary>
    [Fact]
    public void A_claimed_but_not_yet_sampled_slot_is_not_read_as_a_sample()
    {
        var clock = new MutableClock();
        var ring = new SecondsRingBuffer(new SecondsSinceStart(clock), 64);
        var usage = new ThreadPoolUsage(ring);

        var sampledSecond = clock.Second;
        usage.Measurement();

        // Claim the slot of the next second without sampling writing anything into it.
        clock.Advance(1);
        var claimedSecond = clock.Second;
        var entered = ring.TryEnterSlot(null, out _);
        PAssert.That(() => entered);

        PAssert.That(() => usage.Collect(claimedSecond, claimedSecond) == null);
        var window = usage.Collect(sampledSecond, claimedSecond).AssertNotNull();
        PAssert.That(() => window.CompletedWorkItems >= 0);
        PAssert.That(() => window.LockContentions >= 0);
    }

    /// <summary>
    /// The gauges describe a level, so within a window the highest reading counts: a peak that
    /// subsides again between two samples would otherwise remain invisible.
    /// </summary>
    [Fact]
    public void A_window_keeps_the_highest_gauge_reading_of_its_seconds()
    {
        var clock = new MutableClock();
        var usage = new ThreadPoolUsage(new(new SecondsSinceStart(clock), 64));

        var firstSecond = clock.Second;
        usage.Measurement();
        var quiet = usage.Collect(firstSecond, firstSecond).AssertNotNull();

        // Offering work pushes the thread pool up; if it drops back afterwards, the peak remains.
        // The work items must all be finished before the events are disposed: some only start after Set().
        using var blocked = new ManualResetEventSlim();
        var workItems = Environment.ProcessorCount * 4;
        using var finished = new CountdownEvent(workItems);
        for (var i = 0; i < workItems; i++) {
            _ = ThreadPool.UnsafeQueueUserWorkItem(
                _callback => {
                    blocked.Wait();
                    _ = finished.Signal();
                },
                null
            );
        }

        clock.Advance(1);
        usage.Measurement();
        blocked.Set();
        PAssert.That(() => finished.Wait(TimeSpan.FromSeconds(30)));
        var lastSecond = clock.Second;

        var busy = usage.Collect(firstSecond, lastSecond).AssertNotNull();
        PAssert.That(() => busy.MaxThreadCount >= quiet.MaxThreadCount);
        PAssert.That(() => busy.MaxPendingWorkItems >= quiet.MaxPendingWorkItems);
    }

    /// <summary>
    /// The counters increase from process start, so a window should report the increase within that
    /// window and not the level itself. A window that begins after earlier samples calculates from
    /// the last level before it, so that consecutive windows do not count the same work twice.
    /// </summary>
    [Fact]
    public void Counters_are_reported_as_the_increase_within_the_window()
    {
        var clock = new MutableClock();
        var usage = new ThreadPoolUsage(new(new SecondsSinceStart(clock), 64));

        var firstSecond = clock.Second;
        usage.Measurement();
        var completedAfterFirstSample = ThreadPool.CompletedWorkItemCount;

        using var done = new CountdownEvent(50);
        for (var i = 0; i < done.InitialCount; i++) {
            _ = ThreadPool.UnsafeQueueUserWorkItem(_ => done.Signal(), null);
        }

        PAssert.That(() => done.Wait(TimeSpan.FromSeconds(10)));
        PAssert.That(() => SpinWait.SpinUntil(() => ThreadPool.CompletedWorkItemCount >= completedAfterFirstSample + done.InitialCount, TimeSpan.FromSeconds(10)));

        clock.Advance(1);
        var completedBeforeSecondSample = ThreadPool.CompletedWorkItemCount;
        usage.Measurement();
        var lastSecond = clock.Second;

        // The offered work falls within the window, so the increase includes those work items.
        var window = usage.Collect(firstSecond, lastSecond).AssertNotNull();
        PAssert.That(() => window.CompletedWorkItems >= done.InitialCount);

        // A following window begins where this one ended and so does not count that work again: the
        // increase stays within what the runtime counter itself went up by between the two samples.
        // The counter is process-wide, so we read it explicitly instead of assuming that little
        // other work completes in the meantime.
        clock.Advance(1);
        usage.Measurement();
        var completedAfterThirdSample = ThreadPool.CompletedWorkItemCount;
        var next = usage.Collect(lastSecond + 1, clock.Second).AssertNotNull();
        PAssert.That(() => next.CompletedWorkItems <= completedAfterThirdSample - completedBeforeSecondSample);
    }
}
