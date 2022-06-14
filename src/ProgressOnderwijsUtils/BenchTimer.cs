namespace ProgressOnderwijsUtils;

public static class BenchTimer
{
    public sealed record BenchMark(TimeSpan Min, TimeSpan Max, TimeSpan Median, TimeSpan Mean);

    public static BenchMark Time(Action a, int numRuns)
    {
        if (numRuns < 5) {
            throw new ArgumentException("Need to test for at least 5 runs", nameof(numRuns));
        }
        if (numRuns % 2 == 0) {
            throw new ArgumentException("Need to test an odd number of runs", nameof(numRuns));
        }

        var times = Enumerable.Range(0, numRuns)
            .Select(_ => Time(a))
            .OrderBy(t => t)
            .ToArray();

        return new(times.First(), times.Last(), times.SkipLast(2).ToArray()[numRuns / 2], TimeSpan.FromTicks((long)Math.Round(times.SkipLast(2).Average(time => time.Ticks))));
    }

    public static TimeSpan Time(Action a)
    {
        var timer = Stopwatch.StartNew();
        a();
        return timer.Elapsed;
    }
}
