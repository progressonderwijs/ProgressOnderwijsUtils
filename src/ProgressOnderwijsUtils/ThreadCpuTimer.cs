using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public sealed class ThreadCpuTimer
    {
        readonly TimeSpan start;
        readonly Stopwatch wallclock;
        readonly ProcessThread thread;

        ThreadCpuTimer()
        {
#pragma warning disable 612,618
            var id = AppDomain.GetCurrentThreadId();
#pragma warning restore 612,618
            thread = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Single(pt => pt.Id == id);
            start = thread.TotalProcessorTime;
            wallclock = Stopwatch.StartNew();
        }

        /// <summary>
        /// May return null when no longer on the same OS thread.
        /// </summary>
        public double? CpuMilliseconds()
        {
#pragma warning disable 612,618
            var currentId = AppDomain.GetCurrentThreadId();
#pragma warning restore 612,618
            return thread.Id != currentId ? default(double?) : (thread.TotalProcessorTime - start).TotalMilliseconds;
        }

        public double WallClockMilliseconds() { return wallclock.Elapsed.TotalMilliseconds; }
        public static ThreadCpuTimer StartNew() { return new ThreadCpuTimer(); }
    }
}
