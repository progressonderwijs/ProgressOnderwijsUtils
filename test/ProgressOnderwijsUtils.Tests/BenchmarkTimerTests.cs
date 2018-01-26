using System;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public class BenchmarkTimerTests
    {
        [Fact]
        public void ArgVerify()
        {
            Assert.Throws<ArgumentException>(() => BenchTimer.BestTime(() => { }, 0));
        }
    }
}