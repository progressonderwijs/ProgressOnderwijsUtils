namespace ProgressOnderwijsUtils.Tests;

public sealed class BenchmarkTimerTests
{
    [Fact]
    public void ArgVerifyPositive()
        => Assert.Throws<ArgumentException>(() => BenchTimer.Time(() => { }, 0));

    [Fact]
    public void ArgVerifyOdd()
        => Assert.Throws<ArgumentException>(() => BenchTimer.Time(() => { }, 8));

    [Fact]
    public void Time_benchmark()
    {
        var period = TimeSpan.FromMilliseconds(10);
        var benchMark = BenchTimer.Time(
            () => {
                Thread.Sleep(period);
                period = period.Add(TimeSpan.FromMilliseconds(10));
            },
            7
        );

        PAssert.That(() => benchMark.Min < benchMark.Median);
        PAssert.That(() => benchMark.Min < benchMark.Mean);
        PAssert.That(() => benchMark.Median < benchMark.Max);
        PAssert.That(() => benchMark.Mean < benchMark.Max);
    }
}
