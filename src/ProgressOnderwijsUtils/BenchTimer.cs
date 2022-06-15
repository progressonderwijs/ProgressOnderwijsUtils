namespace ProgressOnderwijsUtils;

public static class BenchTimer
{
    public sealed record BenchMark(TimeSpan Min, TimeSpan Max, TimeSpan Median, TimeSpan Mean);

    /// <summary>
    /// Run an action numRuns times and calculate benchmark on the elapsed times.
    /// </summary>
    /// <param name="action">The action to measure</param>
    /// <param name="numRuns">The number if times the action is measured</param>
    /// <param name="numOutliers">The number of slowest runs that will be excluded from the mean and median results.</param>
    public static BenchMark Time(Action action, int numRuns, int numOutliers)
    {
        if (numRuns < 1) {
            throw new ArgumentException("Need to test for at least 1 run", nameof(numRuns));
        }
        if (numRuns <= numOutliers) {
            throw new ArgumentException("Need to determine mean and median on at least 1 run", nameof(numRuns));
        }
        if ((numRuns - numOutliers) % 2 == 0) {
            throw new ArgumentException("Need to test an odd number of runs", nameof(numRuns));
        }

        var times = Enumerable.Range(0, numRuns)
            .Select(_ => Time(action))
            .OrderBy(t => t)
            .ToArray();

        return new(
            times.First(),
            times.Last(),
            times.SkipLast(numOutliers).ToArray()[(numRuns - numOutliers) / 2],
            TimeSpan.FromTicks((long)Math.Round(times.SkipLast(numOutliers).Average(time => time.Ticks)))
        );
    }

    public static TimeSpan Time(Action action)
    {
        var timer = Stopwatch.StartNew();
        action();
        return timer.Elapsed;
    }
}
