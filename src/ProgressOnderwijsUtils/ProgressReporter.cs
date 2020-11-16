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
        readonly long ReportEveryFractionOfTotal;
        readonly Action<ProgressReport> onReport;
        readonly long reportEveryTicks;
        int stepsDone;
        long nextReportDueAt;

        public ProgressReporter(int TotalSteps, Action<ProgressReport> onReport, int? reportEveryFractionOfTotal = null, TimeSpan? reportEveryTimeSpan = null)
        {
            this.TotalSteps = TotalSteps;
            this.onReport = onReport;
            ReportEveryFractionOfTotal = reportEveryFractionOfTotal ?? 20; //default 1/20 i.e. every five percent
            start = DateTime.Now;
            sw = Stopwatch.StartNew();
            nextReportDueAt = reportEveryTicks = (reportEveryTimeSpan ?? TimeSpan.FromMinutes(1.0)).Ticks;
        }

        public ProgressReporter(int TotalSteps, string reportNameForConsole, int? reportEveryFractionOfTotal = null, TimeSpan? reportEveryTimeSpan = null)
            : this(TotalSteps,
                report =>
                    Console.WriteLine(
                        reportNameForConsole + ": " + report.PercentDone + "%, " + report.TimeLeft.TotalSeconds.ToString("f1") + "s left (" + report.Start + " - " + report.Eta + ")"), reportEveryFractionOfTotal, reportEveryTimeSpan) { }

        public void Step(int steps = 1)
        {
            var newProgressVal = Interlocked.Add(ref stepsDone, steps); //stepsDone++; is thread unsafe.
            var oldProgressVal = newProgressVal - steps;
            var elapsed = sw.Elapsed;
            if (ReportEveryFractionOfTotal * newProgressVal / TotalSteps > ReportEveryFractionOfTotal * oldProgressVal / TotalSteps
                || elapsed.Ticks > nextReportDueAt) {
                var percentProgress = PercentProgress(newProgressVal);
                var elapsedMS = elapsed.TotalMilliseconds;
                var totalMS = elapsedMS * TotalSteps / newProgressVal;
                var scaled = TimeSpan.FromMilliseconds(totalMS);
                var leftOver = scaled - elapsed;
                var now = start + elapsed;
                var eta = start + scaled;
                nextReportDueAt = elapsed.Ticks + reportEveryTicks;
                onReport(new ProgressReport(start, now, eta, newProgressVal, percentProgress, leftOver));
            }
        }

        public int StepsDone
            => Volatile.Read(ref stepsDone);

        int PercentProgress(int newProgressVal)
            => 100 * newProgressVal / TotalSteps;
    }
}
