namespace ProgressOnderwijsUtils.Tests;

public sealed class BenchmarkTimerTests
{
    [Fact]
    public void ArgVerify()
        => Assert.Throws<ArgumentException>(() => BenchTimer.BestTime(() => { }, 0));
}