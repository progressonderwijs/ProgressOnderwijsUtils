using System;
using System.Diagnostics;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    static class BenchTimer
    {
        public static TimeSpan MinimumTime(Action a, int numRuns = 5)
        {
            if (numRuns < 1) {
                throw new ArgumentException("Need to test for at least 1 run", nameof(numRuns));
            }
            var bestTime = TimeSpan.MaxValue;
            Stopwatch timer = new Stopwatch();
            for (int i = 0; i < numRuns; i++) {
                timer.Restart();
                a();
                timer.Stop();
                bestTime = timer.Elapsed < bestTime ? timer.Elapsed : bestTime;
            }
            return bestTime;
        }
    }

    public class BenchmarkTimerTests
    {
        [Test]
        public void ArgVerify()
        {
            Assert.Throws<ArgumentException>(() => BenchTimer.MinimumTime(() => { }, 0));
        }
    }
}
