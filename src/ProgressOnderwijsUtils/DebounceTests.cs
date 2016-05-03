using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
    public class DebounceTests
    {
        [Test]
        public void DebounceEventuallyCalls()
        {
            var taskCS = new TaskCompletionSource<int>();
            var task = taskCS.Task;
            var handler = HandlerUtils.Debounce(
                TimeSpan.FromMilliseconds(10),
                () =>
                    taskCS.SetResult(0));

            handler();
            Assert.AreNotEqual(TaskStatus.RanToCompletion, task.Status);
            task.Wait(1000);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        [Test]
        public void DebounceCallsAfterTheRightAmountOfTime()
        {
            var task = new TaskCompletionSource<int>();
            var handler = HandlerUtils.Debounce(
                TimeSpan.FromMilliseconds(35),
                () =>
                    task.SetResult(0));
            var sw = Stopwatch.StartNew();
            handler();
            task.Task.Wait(500);

            var elapsedMS = sw.Elapsed.TotalMilliseconds;
            PAssert.That(() => elapsedMS >= 34 && elapsedMS < 100);
        }

        [Test]
        public void DebounceCallsHandlersWithMutualExclusion()
        {
            int inCriticalSection = 0;
            var counts = new BlockingCollection<int>();
            var handler = HandlerUtils.Debounce(
                TimeSpan.FromMilliseconds(35),
                () => {
                    var count = Interlocked.Increment(ref inCriticalSection);
                    counts.Add(count);
                    Thread.Sleep(100);
                    Interlocked.Decrement(ref inCriticalSection);
                });
            for (int n = 0; n < 5; n++) {
                int runs = counts.Count;
                handler();
                handler();

                var sw = Stopwatch.StartNew();
                while (counts.Count == runs && sw.Elapsed.TotalMilliseconds < 300.0) {
                    Thread.Sleep(5);
                }
            }

            PAssert.That(() => counts.All(i => i == 1));
            PAssert.That(() => counts.Count == 5);
        }

        [Test]
        public void LotsOfCallsPreventHandlerFromFiring()
        {
            const int durationThatEventsAreFired = 300;
            const int debounceDurationThreshhold = 50;
            const int numberOfEventFiringThreads = 10;
            const int earliestExpectedDebouncedEventDelay = durationThatEventsAreFired + debounceDurationThreshhold;
            const int gracePeriod = debounceDurationThreshhold * 5;
            const int durationToWaitForDebouncedHandlerToFire = durationThatEventsAreFired + gracePeriod;

            var debouncedHandlerCompletion = new TaskCompletionSource<TimeSpan>();
            var debouncedHandlerTask = debouncedHandlerCompletion.Task;
            var sw = Stopwatch.StartNew();
            var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(debounceDurationThreshhold), () => debouncedHandlerCompletion.SetResult(sw.Elapsed));

            var eventFiringTasks = Enumerable.Range(0, numberOfEventFiringThreads)
                .Select(threadId =>
                    Task.Run(async () => {
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
                    })
                )
                .ToArray();

            Task.Delay(durationThatEventsAreFired).ContinueWith(_ => handler());

            if (!debouncedHandlerTask.Wait(durationToWaitForDebouncedHandlerToFire)) {
                Assert.Fail($"debounced handler failed to run even {gracePeriod}ms after the last event fired");
            }

            var eventFiringTimes = eventFiringTasks.SelectMany(t => t.Result).OrderBy(t => t).ToArray();
            var eventFiringTimeDeltas = eventFiringTimes.Skip(1).Zip(eventFiringTimes, (later, earlier) => later - earlier);

            var actualDebouncedEventDelay = debouncedHandlerTask.Result.TotalMilliseconds;
            var worstEventFiringDelta = eventFiringTimeDeltas.Max().TotalMilliseconds;

            if (worstEventFiringDelta >= debounceDurationThreshhold && actualDebouncedEventDelay < earliestExpectedDebouncedEventDelay) {
                Assert.Inconclusive("The timespan between two event firings was greater than the debounce threshhold - this run is invalid.");
            }

            PAssert.That(() => actualDebouncedEventDelay >= earliestExpectedDebouncedEventDelay, $"worstEventFiringDelta: {worstEventFiringDelta}");
        }
    }
}
