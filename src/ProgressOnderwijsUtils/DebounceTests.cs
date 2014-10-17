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
			bool called = false;
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(10), () =>
				called = true);
			handler();
			Assert.That(called, Is.False);
			Thread.Sleep(20);
			Assert.That(called, Is.True);
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
			PAssert.That(() => elapsedMS >= 35 && elapsedMS < 100);
		}

		[Test]
		public void DebounceCallsHandlersWithMutualExclusion()
		{
			int inCriticalSection = 0;
			var counts = new BlockingCollection<int>();
			var task = new TaskCompletionSource<int>();
			var handler = HandlerUtils.Debounce(TimeSpan.FromMilliseconds(35), () =>
			{
				var count = Interlocked.Increment(ref inCriticalSection);
				counts.Add(count);
				Thread.Sleep(100);
				Interlocked.Decrement(ref inCriticalSection);
			});
			handler();
			var sw = Stopwatch.StartNew();
			task.Task.Wait(500);

			PAssert.That(() => counts.All(i=>i==1));
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
			PAssert.That(() => elapsedMS >= 435 && elapsedMS < 600);
		}
	}
}