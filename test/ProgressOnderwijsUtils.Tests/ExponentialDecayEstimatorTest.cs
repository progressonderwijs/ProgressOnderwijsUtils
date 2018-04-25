using System;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ExponentialDecayEstimatorTest
    {
        readonly TimeSpan halflife = TimeSpan.FromDays(1.0);
        readonly DateTime someTime = new DateTime(2020, 6, 6, 0, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void LogOfHalfIsCorrect()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            PAssert.That(() => Math.Exp(ExponentialDecayEstimator.LogOfHalf) == 0.5);
        }

        [Fact]
        public void ValueIsInitially0()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime).RawValue, 0.0));
        }

        [Fact]
        public void ValueCanBeAdded()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            estimator.AddAmount(someTime, 1.337);
            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime).RawValue, 1.337));
        }

        [Fact]
        public void AddingAtSameMomentIsEquivalentToOneAdd()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            estimator.AddAmount(someTime, 1.337);
            estimator.AddAmount(someTime, 1.337);
            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime).RawValue, 2 * 1.337));
        }

        [Fact]
        public void ValueIsHalvedAfterHalflife()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            estimator.AddAmount(someTime, 20);

            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime + halflife).RawValue, 10));
        }

        [Fact]
        public void PastValueDoesNotIncrease()
        {
            //Times in the past should not increase the value so that small clock skew errors never cause increases.

            //is this a good idea?
            var estimator = new ExponentialDecayEstimator(halflife);
            estimator.AddAmount(someTime, 20);

            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime - halflife).RawValue, 20));
        }

        [Fact]
        public void AddingValuesOverTimeAccountsForDecay()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            estimator.AddAmount(someTime, 20);
            estimator.AddAmount(someTime + halflife, 20);
            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(someTime + halflife + halflife).RawValue, 15));
        }

        [Fact]
        public void FarFutureValueCausesNoNumericalInstability()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            var someLargeAmount = double.MaxValue;
            estimator.AddAmount(someTime, someLargeAmount);
            var farFutureMoment = someTime.AddDays(halflife.TotalDays * 10000);
            PAssert.That(() => Utils.FuzzyEquals(estimator.ValueAt(farFutureMoment).RawValue, 0));
        }

        [Fact]
        public void EstimatesTrueRateWellEnough()
        {
            var estimator = new ExponentialDecayEstimator(halflife);
            var startingMoment = someTime;
            var endAt = startingMoment + TimeSpan.FromSeconds(halflife.TotalSeconds * 100);
            var actualRatePerHalfLife = 1337.0;
            var stepSizeInHalfLives = 0.001;
            var stepSize = TimeSpan.FromSeconds(halflife.TotalSeconds * stepSizeInHalfLives);
            var random = new Random(42);
            var currentEstimation = estimator.ValueAt(startingMoment);
            for (var currentMoment = startingMoment; currentMoment < endAt; currentMoment = currentMoment + stepSize) {
                var randomValueMeanOne = random.NextDouble() * 2;

                currentEstimation = estimator.AddAmount(currentMoment, randomValueMeanOne * stepSizeInHalfLives * actualRatePerHalfLife);
            }

            PAssert.That(() => Math.Abs(currentEstimation.EstimatedEventCountPerHalflife - actualRatePerHalfLife) < actualRatePerHalfLife * 0.05,
                "if I observe 1000 events per half-life for 100 halflives, then the current estimate should be within 5% of the true value");
        }
    }
}
