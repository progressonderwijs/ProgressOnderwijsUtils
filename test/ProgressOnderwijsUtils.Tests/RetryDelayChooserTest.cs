using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class RetryDelayChooserTest
    {
        [Fact]
        public void AlmostNoErrorsMeansRetryInMilliseconds()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(1.0);
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalMilliseconds, 3.492));
        }

        [Fact]
        public void FewErrorsMeansRetryInSeconds()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(30.0);
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalSeconds, 3.143));
        }

        [Fact]
        public void AFewSimulatedErrorsPerHour_ResultsIn_LowDelay()
        {
            var delayChooser = new RetryDelayChooser(TimeSpan.FromMinutes(10));
            var moment = new DateTime(1999, 1, 1).ToUniversalTime(); //arbitrary
            for (var i = 0; i < 1000; i++) {
                delayChooser.RegisterErrorAt(moment);
                moment = moment.AddHours(1);
            }
            var delay = delayChooser.RetryDelayAt(moment);
            //so now we've had 24 errors a day for a few days
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalMinutes, 0.2171));
        }

        [Fact]
        public void InitialDelayIsZero()
        {
            var delayChooser = new RetryDelayChooser(TimeSpan.FromMinutes(5));
            var startMoment = new DateTime(2020, 2, 20).ToUniversalTime(); //arbitrary
            PAssert.That(() => delayChooser.RetryDelayAt(startMoment) == TimeSpan.Zero);
        }

        [Fact]
        public void ImmediatelyAfterJustOneErrorTheDelayIsStillRelativelySmall()
        {
            var constantFailureDelayTarget = TimeSpan.FromMinutes(1);
            var delayChooser = new RetryDelayChooser(constantFailureDelayTarget);
            var startMoment = new DateTime(2040, 4, 4).ToUniversalTime(); //arbitrary
            delayChooser.RegisterErrorAt(startMoment);
            var actualRetryDelayAfterOneError = delayChooser.RetryDelayAt(startMoment);
            PAssert.That(() => Utils.FuzzyEquals(actualRetryDelayAfterOneError.TotalSeconds /constantFailureDelayTarget.TotalSeconds, 0.00116));
        }

        [Fact]
        public void SomeErrorsMeansRetryInMinutes()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(200.0);
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalMinutes, 2.328));
        }

        [Fact]
        public void After22ErrorsHalfOfDelayLimitIsReached()
        {
            var chooser = new RetryDelayChooser(TimeSpan.FromHours(1));
            var currentMoment = new DateTime(2025, 6, 1).ToUniversalTime(); //whenever
            var errorsNecessaryToReachHalfOfDelayLimit = 0;
            while (errorsNecessaryToReachHalfOfDelayLimit < 10_000) {
                var delay = chooser.RegisterErrorAndGetDelay(currentMoment);
                if (delay.TotalHours >= 0.5) { //half of the convergened limit
                    break;
                }
                currentMoment += delay;
                errorsNecessaryToReachHalfOfDelayLimit++;
            }

            PAssert.That(() => errorsNecessaryToReachHalfOfDelayLimit == 22);
        }

        [Fact]
        public void ManyErrorsMeansRetryInHours()
        {
            var delay = new RetryDelayChooser(TimeSpan.FromMinutes(5)).ErrorsPerDayToRetryDelay(2000.0);
            PAssert.That(() => Utils.FuzzyEquals(delay.TotalHours, 3.88));
        }

        [Fact]
        public void OneContinuouslyFailingProcessRetriesAsPredicted()
        {
            var convergedDelay = ConvergedDelay(1, TimeSpan.FromMinutes(5));
            PAssert.That(() => Utils.FuzzyEquals(convergedDelay.TotalMinutes, 5));
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

            //then do a few more rounds for better accuracy.
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
