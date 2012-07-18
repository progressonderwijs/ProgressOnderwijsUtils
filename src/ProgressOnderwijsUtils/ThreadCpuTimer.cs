using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils
{
	public sealed class ThreadCpuTimer
	{
		readonly TimeSpan start;


		readonly ProcessThread thread;

		ThreadCpuTimer()
		{
			var id = AppDomain.GetCurrentThreadId();
			thread = Process.GetCurrentProcess().Threads.Cast<ProcessThread>().Single(pt => pt.Id == id);
			start = thread.TotalProcessorTime;
		}

		/// <summary>
		/// May return null when no longer on the same OS thread.
		/// </summary>
		public double? ElapsedMilliseconds()
		{
			var currentId = AppDomain.GetCurrentThreadId();
			return thread.Id != currentId ? default(double?) : (thread.TotalProcessorTime - start).TotalMilliseconds;
		}

		public static ThreadCpuTimer StartNew() { return new ThreadCpuTimer(); }
	}
}
