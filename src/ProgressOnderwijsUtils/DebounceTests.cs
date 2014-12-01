using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public class DebounceTests
	{
		[Test]
		public void DebounceEventuallyCalls()
		{
			var taskCS = new TaskCompletionSource<int>();
			var task = taskCS.Task;
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(10), () =>
				taskCS.SetResult(0));

			handler();
			Assert.AreNotEqual(TaskStatus.RanToCompletion, task.Status);
			task.Wait(1000);
			Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
		}

		[Test]
		public void DebounceCallsAfterTheRightAmountOfTime()
		{
			var task = new TaskCompletionSource<int>();
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(35), () =>
				task.SetResult(0));
			var sw = Stopwatch.StartNew();
			handler();
			task.Task.Wait(500);

			var elapsedMS = sw.Elapsed.TotalMilliseconds;
			PAssert.That(() => elapsedMS >= 34 && elapsedMS < 100);
		}

		[Test]
		public void DebounceCallsHandlersWithMutualExclusion()
		{
			int inCriticalSection = 0;
			var counts = new BlockingCollection<int>();
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(35), () =>
			{
				var count = Interlocked.Increment(ref inCriticalSection);
				counts.Add(count);
				Thread.Sleep(100);
				Interlocked.Decrement(ref inCriticalSection);
			});
			for (int n = 0; n < 5; n++)
			{
				int runs = counts.Count;
				handler();
				handler();


				var sw = Stopwatch.StartNew();
				while (counts.Count == runs && sw.Elapsed.TotalMilliseconds<300.0)
					Thread.Sleep(5);
			}
			
			PAssert.That(() => counts.All(i=>i==1));
			PAssert.That(() => counts.Count == 5);
		}

		[Test]
		public void LotsOfCallsPreventHandlerFromFiring()
		{
			var task = new TaskCompletionSource<TimeSpan>();
			var sw = Stopwatch.StartNew();
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(35), () =>
				task.SetResult(sw.Elapsed));

			Enumerable.Range(0, 5).Select(n =>
				Task.Run(async () =>
				{
					var elapsed = Stopwatch.StartNew();
					var r = new Random(n);
					while (elapsed.Elapsed < TimeSpan.FromMilliseconds(400))
					{
						handler();
						await Task.Delay(r.Next(35));
					}
				})).ToArray();

			Task.Delay(400).ContinueWith(_ => handler());

			if (!task.Task.Wait(600))
				Assert.Fail("debounced handler failed to run even 100 ms after the last");

			var elapsedMS = task.Task.Result.TotalMilliseconds;
			PAssert.That(() => elapsedMS >= 434 && elapsedMS < 600);
		}
	}
}