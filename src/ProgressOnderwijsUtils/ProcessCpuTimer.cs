using System;
using System.Diagnostics;

namespace ProgressOnderwijsUtils
{
    public sealed class ProcessCpuTimer
    {
        readonly TimeSpan start;
        readonly Stopwatch wallclock;
        static readonly Process currentProcess = Process.GetCurrentProcess();

        ProcessCpuTimer()
        {
            start = currentProcess.TotalProcessorTime;
            wallclock = Stopwatch.StartNew();
        }

        /// <summary>
        /// Includes time on other threads; this may lead to over-reporting in concurrency situations such as a production webserver.
        /// </summary>
        public TimeSpan CpuTime()
            => currentProcess.TotalProcessorTime - start;

        public TimeSpan WallClockTime()
            => wallclock.Elapsed;

        public static ProcessCpuTimer StartNew()
            => new ProcessCpuTimer();
    }
}
