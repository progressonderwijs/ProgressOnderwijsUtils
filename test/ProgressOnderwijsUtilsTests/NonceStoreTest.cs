using System;
using System.Collections.Generic;
using System.Threading;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    public class NonceStoreTest
    {
        public static object[][] NonceStoreItemEqualityData()
        {
            return new[]
            {
                new object[] {null, null, true}
                , new object[] {null, new TimestampedNonce(new DateTime(2000,1,1), 37L), false}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 37L), null, false}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), true}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 42L), false}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 42L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false}
                , new object[] {new TimestampedNonce(new DateTime(2000, 1, 1), 37L), new TimestampedNonce(new DateTime(2000, 1, 1), 37L), false}
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
            PAssert.That(() => new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.Now, 37L)));
            PAssert.That(() => new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.UtcNow, 37L)));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.Now.AddHours(-1), 37L)));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.UtcNow.AddHours(-1), 37L)));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.Now.AddHours(1), 37L)));
            PAssert.That(() => !new NonceStore().IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.UtcNow.AddHours(1), 37L)));
        }

        [Fact]
        public void IsNotKnown()
        {
            foreach (var now in new[] {DateTime.UtcNow, DateTime.Now})
            {
                var sut = new NonceStore();
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L)));
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), -1L)));
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L)));
                PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(2), 13L)));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L)));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), -1L)));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(1), 13L)));
                PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now.AddSeconds(2), 13L)));
            }
        }

        [Fact]
        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            var sut = new NonceStore(TimeSpan.FromSeconds(0.5), 1);
            PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now, 37L)));
            Thread.Sleep(TimeSpan.FromSeconds(1));
            PAssert.That(() => !sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(now, 37L)));
            PAssert.That(() => sut.IsFreshAndPreviouslyUnusedNonce(new TimestampedNonce(DateTime.UtcNow, 37L)));
        }
    }
}