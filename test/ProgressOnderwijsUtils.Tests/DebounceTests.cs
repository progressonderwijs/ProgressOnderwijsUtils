using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ProgressOnderwijsUtils.Tests;

public sealed class DebounceTests
{
    readonly ITestOutputHelper output;

    public DebounceTests(ITestOutputHelper output)
        => this.output = output;

    [Fact]
    public void DebounceEventuallyCalls()
    {
        var taskCS = new TaskCompletionSource<int>();
        var task = taskCS.Task;
        var handler = HandlerUtils.Debounce(
            TimeSpan.FromMilliseconds(10),
            () =>
                taskCS.SetResult(0)
        );

        handler();
        Assert.NotEqual(TaskStatus.RanToCompletion, task.Status);
        _ = task.Wait(10_000);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
    }

    [Fact(Skip = "Flaky")]
    public void DebounceCallsAfterTheRightAmountOfTime()
    {
        var task = new TaskCompletionSource<int>();
        var handler = HandlerUtils.Debounce(
            TimeSpan.FromMilliseconds(35),
            () =>
                task.SetResult(0)
        );
        var sw = Stopwatch.StartNew();
        handler();
        _ = task.Task.Wait(500);

        var elapsedMS = sw.Elapsed.TotalMilliseconds;
        PAssert.That(() => elapsedMS >= 34 && elapsedMS < 100);
    }

    [Fact]
    public void DebounceCallsHandlersWithMutualExclusion()
    {
        var inCriticalSection = 0;
        var counts = new BlockingCollection<int>();
        var handler = HandlerUtils.Debounce(
            TimeSpan.FromMilliseconds(35),
            () => {
                var count = Interlocked.Increment(ref inCriticalSection);
                counts.Add(count);
                Thread.Sleep(100);
                _ = Interlocked.Decrement(ref inCriticalSection);
            }
        );
        for (var n = 0; n < 5; n++) {
            var runs = counts.Count;
            handler();
            handler();

            var sw = Stopwatch.StartNew();
            while (counts.Count == runs && sw.Elapsed.TotalMilliseconds < 10_000.0) {
                Thread.Sleep(5);
            }
        }

        PAssert.That(() => counts.All(i => i == 1));
        PAssert.That(() => counts.Count == 5);
    }

    [Fact]
    public void LotsOfCallsPreventHandlerFromFiring()
    {
        const int durationThatEventsAreFired = 300;
        const int debounceDurationThreshhold = 50;
        const int numberOfEventFiringThreads = 10;
        const int earliestExpectedDebouncedEventDelay = durationThatEventsAreFired + debounceDurationThreshhold;
        const int gracePeriod = debounceDurationThreshhold * 5;
        const int durationToWaitForDebouncedHandlerToFire = durationThatEventsAreFired + gracePeriod + 10_000;

        var debouncedHandlerCompletion = new TaskCompletionSource<TimeSpan>();
        var debouncedHandlerTask = debouncedHandlerCompletion.Task;
        var sw = Stopwatch.StartNew();
        var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(debounceDurationThreshhold), () => debouncedHandlerCompletion.SetResult(sw.Elapsed));

        var eventFiringTasks = Enumerable.Range(0, numberOfEventFiringThreads)
            .Select(
                threadId =>
                    Task.Run(
                        async () => {
                            var actualTimes = new List<DateTime>(3 * durationThatEventsAreFired / debounceDurationThreshhold);
                            var elapsed = Stopwatch.StartNew();
                            var r = new Random(threadId);
                            while (elapsed.Elapsed < TimeSpan.FromMilliseconds(durationThatEventsAreFired)) {
                                var nextDelay = Task.Delay(r.Next(debounceDurationThreshhold));
                                handler();
                                actualTimes.Add(DateTime.UtcNow);
                                await nextDelay;
                            }
                            return actualTimes.ToArray();
                        }
                    )
            )
            .ToArray();

        _ = Task.Delay(durationThatEventsAreFired).ContinueWith(_ => handler());

        if (!debouncedHandlerTask.Wait(durationToWaitForDebouncedHandlerToFire)) {
            throw new($"debounced handler failed to run even {gracePeriod}ms after the last event fired");
        }

        var eventFiringTimes = eventFiringTasks.SelectMany(t => t.GetAwaiter().GetResult()).OrderBy(t => t).ToArray();
        var eventFiringTimeDeltas = eventFiringTimes.Skip(1).Zip(eventFiringTimes, (later, earlier) => later - earlier);

        var actualDebouncedEventDelay = debouncedHandlerTask.GetAwaiter().GetResult().TotalMilliseconds;
        var worstEventFiringDelta = eventFiringTimeDeltas.Max().TotalMilliseconds;

        if (worstEventFiringDelta >= debounceDurationThreshhold && actualDebouncedEventDelay < earliestExpectedDebouncedEventDelay) {
            output.WriteLine("The timespan between two event firings was greater than the debounce threshhold - this run is inconclusive.");
            return;
        }

        PAssert.That(() => actualDebouncedEventDelay >= earliestExpectedDebouncedEventDelay, $"worstEventFiringDelta: {worstEventFiringDelta}");
    }
}
