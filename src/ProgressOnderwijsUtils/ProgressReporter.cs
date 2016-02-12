using System;
using System.Diagnostics;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public struct ProgressReport
    {
        public readonly DateTime Start, Current, Eta;
        public readonly int StepsDone, PercentDone;
        public readonly TimeSpan TimeLeft;

        public ProgressReport(DateTime start, DateTime current, DateTime eta, int stepsDone, int percentDone, TimeSpan timeLeft)
        {
            Start = start;
            Current = current;
            Eta = eta;
            StepsDone = stepsDone;
            PercentDone = percentDone;
            TimeLeft = timeLeft;
        }
    }

    public class ProgressReporter
    {
        readonly Stopwatch sw;
        readonly DateTime start;
        readonly int TotalSteps;
        readonly Action<ProgressReport> report;
        int stepsDone;

        public ProgressReporter(int TotalSteps, Action<ProgressReport> report)
        {
            this.TotalSteps = TotalSteps;
            this.report = report;
            start = DateTime.Now;
            sw = Stopwatch.StartNew();
        }

        public ProgressReporter(int TotalSteps, string actionName)
            : this(TotalSteps,
                report =>
                    Console.WriteLine(
                        actionName + ": " + report.PercentDone + "%, " + report.TimeLeft.TotalSeconds.ToString("f1") + "s left (" + report.Start + " - " + report.Eta + ")")) { }

        public void Step(int steps=1)
        {
            int newProgressVal = Interlocked.Add(ref stepsDone,steps); //stepsDone++; is thread unsafe.
            var oldProgressVal = newProgressVal - steps;
            var percentProgress = PercentProgress(newProgressVal);
            if (percentProgress / 5 > PercentProgress(oldProgressVal) / 5) {
                var elapsed = sw.Elapsed;
                var elapsedMS = elapsed.TotalMilliseconds;
                var totalMS = elapsedMS * TotalSteps / newProgressVal;
                var scaled = TimeSpan.FromMilliseconds(totalMS);
                var leftOver = scaled - elapsed;
                var now = start + elapsed;
                var eta = start + scaled;
                report(new ProgressReport(start, now, eta, newProgressVal, percentProgress, leftOver));
            }
        }

        public int StepsDone => Volatile.Read(ref stepsDone);

        int PercentProgress(int newProgressVal) => 100 * newProgressVal / TotalSteps;
    }
}
