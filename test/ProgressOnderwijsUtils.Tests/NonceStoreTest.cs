using System;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class NonceStoreTest
    {
        [NotNull]
        public static object[][] NonceStoreItemEqualityData()
        {
            return new[] {
                new object[] { null, null, true }, new object[] { null, new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false }, new object[] { new TimestampedNonce(new DateTime(2000, 1, 1), 37L), null, false }, new object[] { new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), true }, new object[] { new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 42L), false }, new object[] { new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2001, 1, 1), 37L), false }, new object[] { new TimestampedNonce(new DateTime(2000, 1, 1), 42L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false }, new object[] { new TimestampedNonce(new DateTime(2001, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false }
            };
        }

        [Theory]
        [MemberData(nameof(NonceStoreItemEqualityData))]
        public void NonceStoreItemEquality(TimestampedNonce lhs, TimestampedNonce rhs, bool expectEqual)
        {
            var result = lhs.Equals(rhs);
            PAssert.That(() => lhs.GetHashCode() == rhs.GetHashCode() == expectEqual);
            PAssert.That(() => result == expectEqual);
        }

        [Fact]
        public void Generate()
        {
            var sut = new NonceStore();
            var firstNonce = sut.Generate();
            var secondNonce = sut.Generate();
            PAssert.That(() => firstNonce != secondNonce);
        }

        [Fact] // we cannot use TestCaseSource as it will be executed upon loading, and these tests much later!
        public void IsInWindow()
        {
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;
            PAssert.That(() => new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(localNow, 37L), utcNow));
            PAssert.That(() => new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(utcNow, 37L), utcNow));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(localNow.AddHours(-1), 37L), utcNow));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(utcNow.AddHours(-1), 37L), utcNow));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(localNow.AddHours(1), 37L), utcNow));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(utcNow.AddHours(1), 37L), utcNow));
        }

        [Fact]
        public void IsNotKnown()
        {
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;
            foreach (var now in new[] { utcNow, localNow }) {
                var sut = new NonceStore();
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L), utcNow));
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), -1L), utcNow));
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(2), 13L), utcNow));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L), utcNow));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), -1L), utcNow));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L), utcNow));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(2), 13L), utcNow));
            }
        }

        [Fact]
        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            var sut = new NonceStore();
            var timestampedNonce = new TimestampedNonce(now, 37L);
            PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(timestampedNonce, now), "initial");
            PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(timestampedNonce, now), "next");
            //timetravel 20 minutes into the future...
            PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddMinutes(17), 37L), now.AddMinutes(20)), "future");
            //EVIL effectively time-travel to the past here...
            PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(timestampedNonce, now), "past");

            //so if the system clock is reset, some nonces may be usable again - that's OK.
        }
    }
}
