using System;
using ProgressOnderwijsUtils;
using Xunit;

namespace ProgressOnderwijsUtilsTests
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