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
			: this(TotalSteps, report =>
				Console.WriteLine(actionName +": "+report.PercentDone + "%, " + report.TimeLeft.TotalSeconds.ToString("f1") + "s left (" + report.Start + " - " + report.Eta + ")"))
		{
		}

		public void Step()
		{
			int newProgressVal = Interlocked.Increment(ref stepsDone); //stepsDone++; is thread unsafe.
			var percentProgress = 100 * newProgressVal / TotalSteps;
			if(percentProgress > 100 * (newProgressVal - 1) / TotalSteps)
			{
				var elapsed = sw.Elapsed;
				double elapsedMS = elapsed.TotalMilliseconds;
				double scaledMS = elapsedMS * TotalSteps / newProgressVal;
				var scaled = TimeSpan.FromMilliseconds(scaledMS);
				var leftOver = scaled - elapsed;
				DateTime now = start + elapsed;
				DateTime eta = start + scaled;
				report(new ProgressReport(start, now, eta, newProgressVal, percentProgress, leftOver));
			}
		}
	}
}