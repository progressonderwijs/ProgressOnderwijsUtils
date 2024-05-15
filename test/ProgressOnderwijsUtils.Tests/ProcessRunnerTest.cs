using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ProgressOnderwijsUtils.Tests;

public sealed class ProcessRunnerTest
{
    readonly ITestOutputHelper output;

    public ProcessRunnerTest(ITestOutputHelper output)
        => this.output = output;

    [Fact]
    public async Task CanCollectOutputErrorAndCode()
    {
        async Task DoTest()
        {
            var result = new ProcessStartSettings {
                ExecutableName = "xcopy",
            }.StartProcess(CancellationToken.None);

            var exitCode = await result.ExitCode;
            var stdErr = await result.StdError();
            var stdOut = await result.StdOutput();
            PAssert.That(() => exitCode == 4);
            PAssert.That(() => stdErr.SequenceEqual(new[] { "Invalid number of parameters", }));
            PAssert.That(() => stdOut.SequenceEqual(new[] { "0 File(s) copied", }));
        }

        try {
            await DoTest();
        } catch {
            //very rarely processes are flaky outside of our control; that's out of scope for this test.
            await DoTest();
        }
    }

    [Fact(Timeout = 5000)]
    public async Task CanCollectOutputAfterUsingWriteToConsoleWithPrefix()
    {
        async Task DoTest()
        {
            var result = new ProcessStartSettings {
                ExecutableName = "xcopy",
            }.StartProcess(CancellationToken.None);

            result.WriteToConsoleWithPrefix("x");
            var stdOut = await result.StdOutput();
            PAssert.That(() => stdOut.SequenceEqual(new[] { "0 File(s) copied", }));
        }

        try {
            await DoTest();
        } catch {
            //very rarely processes are flaky outside of our control; that's out of scope for this test.
            await DoTest();
        }
    }

    [Fact]
    public async Task CanBeCancelled()
    {
        // ReSharper disable MethodSupportsCancellation
        var cancel = new CancellationTokenSource();

        var timer = Stopwatch.StartNew();

        var result = new ProcessStartSettings {
            ExecutableName = "ping",
            Arguments = "localhost -n 100",
        }.StartProcess(cancel.Token);
        _ = result.Output.Subscribe(o => output.WriteLine(o.Line));

        var hasStartedPinging = await result.Output.Any(o => o.Line.StartsWith("Pinging", StringComparison.Ordinal));
        var elapsedAfterFirstOutput = timer.Elapsed;
        PAssert.That(() => hasStartedPinging && !result.ExitCode.IsCompleted && elapsedAfterFirstOutput < TimeSpan.FromSeconds(4));
        await cancel.CancelAsync();
        await result.ExitCode.ContinueWith(_ => { });

        var elapsedAfterExit = timer.Elapsed;
        PAssert.That(() => result.ExitCode.IsCompleted && elapsedAfterExit < TimeSpan.FromSeconds(8));

        _ = await result.Output.ToTask(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(1));
        // ReSharper restore MethodSupportsCancellation
    }

    [Fact]
    public void SupportsLargeIO()
    {
        var token = new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token;
        var inputLine = new string('a', 1022);
        var inputLineCount = 32768;
        //32 MB streamed; 64MB in UCS2 encoding in .net memory.
        var inputLines = Enumerable.Repeat(inputLine, inputLineCount);
        var result = new ProcessStartSettings {
            ExecutableName = "findstr",
            Arguments = "^",
            Stdlnput = inputLines.JoinStrings("\n"),
        }.StartProcess(token);
        var collected = new List<string>();
        _ = result.Output.Subscribe(o => collected.Add(o.Line));

        _ = result.Output.Wait();
        _ = result.ExitCode.Wait(100);
        var finalExitCodeStatus = result.ExitCode.Status;

        var outputLineCount = collected.Count;
        PAssert.That(() => outputLineCount == inputLineCount && finalExitCodeStatus == TaskStatus.RanToCompletion);
        PAssert.That(() => collected.Distinct().Single() == inputLine);
    }
}
