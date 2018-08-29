using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;
using Xunit.Abstractions;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ProcessRunnerTest
    {
        readonly ITestOutputHelper output;

        public ProcessRunnerTest(ITestOutputHelper output)
            => this.output = output;

        [Fact]
        public void CanCollectOutputErrorAndCode()
        {
            void DoTest()
            {
                var result = new ProcessStartSettings {
                    ExecutableName = "xcopy"
                }.StartProcess(CancellationToken.None);

                result.ExitCode.Wait();

                PAssert.That(() => result.ExitCode.Result == 4);
                PAssert.That(() => result.StdError().Result.SequenceEqual(new[] { "Invalid number of parameters" }));
                PAssert.That(() => result.StdOutput().Result.SequenceEqual(new[] { "0 File(s) copied" }));
            }

            try {
                DoTest();
            } catch {
                //very rarely processes are flaky outside of our control; that's out of scope for this test.
                DoTest();
            }
        }

        [Fact]
        public void CanBeCancelled()
        {
            var cancel = new CancellationTokenSource();

            var timer = Stopwatch.StartNew();

            var result = new ProcessStartSettings {
                ExecutableName = "ping",
                Arguments = "localhost -n 100",
            }.StartProcess(cancel.Token);
            result.Output.Subscribe(o => output.WriteLine(o.Line));

            var hasStartedPinging = result.Output.Any(o => o.Line.StartsWith("Pinging", StringComparison.Ordinal)).Wait();
            var elapsedAfterFirstOutput = timer.Elapsed;
            PAssert.That(() => hasStartedPinging && !result.ExitCode.IsCompleted && elapsedAfterFirstOutput < TimeSpan.FromSeconds(4));
            cancel.Cancel();
            Task.WaitAny(result.ExitCode); //WaitAny does not throw, unlike .Wait()

            var elapsedAfterExit = timer.Elapsed;
            PAssert.That(() => result.ExitCode.IsCompleted && elapsedAfterExit < TimeSpan.FromSeconds(8));

            var outputCompletedInTime = result.Output.ToTask(CancellationToken.None).Wait(1000);
            PAssert.That(() => outputCompletedInTime);
        }
    }
}
