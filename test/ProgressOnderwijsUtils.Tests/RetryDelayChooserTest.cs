using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class RetryDelayChooserTest
    {
        [Fact]
        public void NoErrorRateMeansFastRetry()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(1.0);
            PAssert.That(() => delay >= TimeSpan.Zero && delay < TimeSpan.FromMilliseconds(16.0));
        }

        [Fact]
        public void FewErrorsMeansRetryInSeconds()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(30.0);
            PAssert.That(() => delay >= TimeSpan.FromSeconds(3) && delay <= TimeSpan.FromSeconds(4));
        }

        [Fact]
        public void SimulatedErrorPerHourResultsInAroundTwoSecondDelay()
        {
            var delayChooser = new RetryDelayChooser(TimeSpan.FromMinutes(5));
            var startMoment = new DateTime(1999, 1, 1).ToUniversalTime(); //arbitrary
            var endMoment = startMoment.AddDays(4);
            for (var moment = startMoment; moment < endMoment; moment = moment.AddHours(1)) {
                delayChooser.RegisterErrorAt(moment);
            }
            var delay = delayChooser.RetryDelayAt(endMoment);
            //so now we've had 30 errors a day for a few days
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalSeconds, 1.951));
        }

        [Fact]
        public void InitialDelayIsZero()
        {
            var delayChooser = new RetryDelayChooser(TimeSpan.FromMinutes(5));
            var startMoment = new DateTime(2020, 2, 20).ToUniversalTime(); //arbitrary
            PAssert.That(() => delayChooser.RetryDelayAt(startMoment) == TimeSpan.Zero);
        }

        [Fact]
        public void OneErrorIsBla()
        {
            var constantFailureDelayTarget = TimeSpan.FromHours(1);
            var delayChooser = new RetryDelayChooser(constantFailureDelayTarget);
            var startMoment = new DateTime(2040, 4, 4).ToUniversalTime(); //arbitrary
            delayChooser.RegisterErrorAt(startMoment);
            PAssert.That(() => Utils.FuzzyEquals(delayChooser.RetryDelayAt(startMoment).TotalSeconds * 299.7252518, constantFailureDelayTarget.TotalSeconds));
        }

        [Fact]
        public void SomeErrorsMeansRetryInMinutes()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(200.0);
            PAssert.That(() => delay >= TimeSpan.FromMinutes(2) && delay <= TimeSpan.FromMinutes(3));
        }

        [Fact]
        public void ManyErrorsMeansRetryInHours()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(2000.0);
            PAssert.That(() => delay >= TimeSpan.FromHours(3) && delay <= TimeSpan.FromHours(5));
        }

        [Fact]
        public void OneContinuouslyFailingProcessRetriesAsPredicted()
        {
            var convergedDelay = ConvergedDelay(1, TimeSpan.FromMinutes(5));
            PAssert.That(() => convergedDelay > TimeSpan.FromMinutes(4.99) && convergedDelay < TimeSpan.FromMinutes(5.01));
        }

        static TimeSpan ConvergedDelay(int parallelFailingProcesses, TimeSpan constantFailureDelayTarget)
        {
            var retryDelayChooser = new RetryDelayChooser(constantFailureDelayTarget);
            var startMoment = new DateTime(2000, 1, 1).ToUniversalTime(); //arbitrary
            var currentMoment = startMoment;
            var errorCountUntilConvergence = 0;

            TimeSpan lastDelay, nextDelay = TimeSpan.Zero;

            do {
                lastDelay = nextDelay;
                errorCountUntilConvergence++;
                if (errorCountUntilConvergence > 10000) {
                    throw new Exception("Convergence is taking too long!");
                }
                nextDelay = retryDelayChooser.RegisterErrorAndGetDelay(currentMoment);
                //assumption: N parallel processes all failing continuously at interval T is roughtly equivalent to one process failing at interval T/N
                currentMoment += TimeSpan.FromSeconds(nextDelay.TotalSeconds / parallelFailingProcesses);
            } while (!Utils.FuzzyEquals(nextDelay.TotalSeconds, lastDelay.TotalSeconds));

            for (var i = 0; i < errorCountUntilConvergence; i++) {
                nextDelay = retryDelayChooser.RegisterErrorAndGetDelay(currentMoment);
                currentMoment += TimeSpan.FromSeconds(nextDelay.TotalSeconds / parallelFailingProcesses);
            }

            return nextDelay;
        }

        [Fact]
        public void ManyContinuouslyFailingProcessesRetryInHours()
        {
            var convergedDelay = ConvergedDelay(100, TimeSpan.FromMinutes(5));
            PAssert.That(() => convergedDelay > TimeSpan.FromMinutes(100) && convergedDelay < TimeSpan.FromMinutes(200));
        }
    }
}
