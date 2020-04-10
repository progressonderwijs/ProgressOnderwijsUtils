using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class BenchTimer
    {
        public static TimeSpan BestTime(Action a, int numRuns)
        {
            if (numRuns < 1) {
                throw new ArgumentException("Need to test for at least 1 run", nameof(numRuns));
            }
            var bestTime = TimeSpan.MaxValue;
            var timer = new Stopwatch();
            for (var i = 0; i < numRuns; i++) {
                timer.Restart();
                a();
                timer.Stop();
                bestTime = timer.Elapsed < bestTime ? timer.Elapsed : bestTime;
            }
            return bestTime;
        }

        public static TimeSpan Time(Action a)
        {
            var timer = Stopwatch.StartNew();
            a();
            return timer.Elapsed;
        }
    }
}
