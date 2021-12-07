using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils;

public struct ProcessStartSettings
{
    public string? ExecutableName;
    public string? Arguments;
    public string? Stdlnput;
    public string? WorkingDirectory;
    public Dictionary<string, string>? Environment;

    public void PrintProcessArgs(int? exitCode)
    {
        var env = Environment.EmptyIfNull().Select(kvp => "#SET " + kvp.Key + " = " + kvp.Value + "\r\n").JoinStrings();

        Console.WriteLine(env + ExecutableName + " " + Arguments + (exitCode == null ? "" : " [ret:" + exitCode + "]"));
    }

    public Process CreateProcessObj()
    {
        var proc = new Process {
            StartInfo = new ProcessStartInfo {
                CreateNoWindow = true, // we donot need a UI
                RedirectStandardInput = true,
                UseShellExecute = false, //required to be able to redirect streams
                FileName = ExecutableName.AssertNotNull(),
                Arguments = Arguments ?? "",
                WorkingDirectory = WorkingDirectory ?? System.Environment.CurrentDirectory,
            },
            EnableRaisingEvents = true,
        };
        foreach (var kvp in Environment.EmptyIfNull()) {
            proc.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
        }

        return proc;
    }

    public AsyncProcessResult StartProcess(CancellationToken token = new())
    {
        var exitCodeCompletion = new TaskCompletionSource<int>();
        var proc = CreateProcessObj();
        var stopwatch = new Stopwatch();
        var closedParts = 0;

        // ReSharper disable once ConvertToLocalFunction - workaround apparent roslyn bug that considers proc to be nullable otherwise.
        Action MarkOnePartClosed = () => {
            if (Interlocked.Increment(ref closedParts) == 3) {
                proc.Dispose();
            }
        };

        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.RedirectStandardOutput = true;

        proc.Exited += (_, _) => {
            try {
                _ = exitCodeCompletion.TrySetResult(proc.ExitCode);
            } catch (Exception ex) {
                _ = exitCodeCompletion.TrySetException(ex);
            }
            MarkOnePartClosed();
        };

        var stdout = Observable.Create<(ProcessOutputKind Kind, string Content, TimeSpan Offset)>(
            observer => {
                proc.OutputDataReceived += (_, e) => {
                    if (e.Data == null) {
                        observer.OnCompleted();
                        MarkOnePartClosed();
                    } else {
                        observer.OnNext((ProcessOutputKind.StdOutput, e.Data, stopwatch.Elapsed));
                    }
                };
                return Disposable.Empty;
            }
        );
        var stderr = Observable.Create<(ProcessOutputKind Kind, string Content, TimeSpan Offset)>(
            observer => {
                proc.ErrorDataReceived += (_, e) => {
                    if (e.Data == null) {
                        observer.OnCompleted();
                        MarkOnePartClosed();
                    } else {
                        observer.OnNext((ProcessOutputKind.StdError, e.Data, stopwatch.Elapsed));
                    }
                };
                return Disposable.Empty;
            }
        );
        var replayableMergedOutput = stdout.Merge(stderr).Replay();
        _ = replayableMergedOutput.Connect();
        stopwatch.Start();
        _ = proc.Start();
        proc.BeginErrorReadLine();
        proc.BeginOutputReadLine();
        _ = token.Register(
            () => {
                try {
                    if (exitCodeCompletion.TrySetCanceled() && !proc.HasExited) {
                        proc.Kill();
                    }
                } catch (InvalidOperationException) {
                    // already termined, ignore
                }
            },
            false
        );
        WriteStdIn(proc);

        return new AsyncProcessResult(exitCodeCompletion.Task, replayableMergedOutput);
    }

    public int RunProcessWithoutRedirection()
    {
        var proc = CreateProcessObj();
        _ = proc.Start();
        WriteStdIn(proc);
        proc.WaitForExit();
        return proc.ExitCode;
    }

    void WriteStdIn(Process proc)
    {
        StreamWriter procStandardInput;
        try {
            procStandardInput = proc.StandardInput;
        } catch {
            //Beware: microsoft is utterly incompetent, so this code is in an intrinsic race condition with process exit, which you can emulate by sleeping before this try.
            return;
        }
        if (Stdlnput != null) {
            procStandardInput.Write(Stdlnput);
        }
        procStandardInput.Close();
    }
}

public enum ProcessOutputKind { StdOutput, StdError, }

public sealed class AsyncProcessResult
{
    public readonly Task<int> ExitCode;
    public readonly IObservable<(ProcessOutputKind Kind, string Line, TimeSpan OutputMoment)> Output;

    internal AsyncProcessResult(Task<int> exitCode, IObservable<(ProcessOutputKind Kind, string Line, TimeSpan OutputMoment)> output)
    {
        ExitCode = exitCode;
        Output = output;
    }

    public void WriteToConsoleWithPrefix(string prefix)
    {
        var prefixWithSpace = prefix + " ";
        var culture = CultureInfo.InvariantCulture;
        _ = Output.Subscribe(
            outputStreamEvent => {
                Console.WriteLine(prefixWithSpace + Utils.ToFixedPointString(outputStreamEvent.OutputMoment.TotalSeconds, culture, 4) + (outputStreamEvent.Kind == ProcessOutputKind.StdOutput ? "> " : "! ") + outputStreamEvent.Line);
            }
        );
    }

    public Task<string[]> StdOutput()
        => Output.Where(o => o.Kind == ProcessOutputKind.StdOutput).Select(o => o.Line).ToArray().ToTask();

    public Task<string[]> StdError()
        => Output.Where(o => o.Kind == ProcessOutputKind.StdError).Select(o => o.Line).ToArray().ToTask();
}