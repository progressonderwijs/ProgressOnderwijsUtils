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
                , new object[] {null, new NonceStoreItem("c", null, "n"), false}
                , new object[] {new NonceStoreItem("c", null, "n"), null, false}
                , new object[] {new NonceStoreItem("c", null, "n"), new NonceStoreItem("c", null, "n"), true}
                , new object[] {new NonceStoreItem("c", null, "n"), new NonceStoreItem("c", null, "0"), false}
                , new object[] {new NonceStoreItem("c", null, "n"), new NonceStoreItem("1", null, "n"), false}
                , new object[] {new NonceStoreItem("c", null, "0"), new NonceStoreItem("c", null, "n"), false}
                , new object[] {new NonceStoreItem("1", null, "n"), new NonceStoreItem("c", null, "n"), false}
            };
        }

        [Theory]
        [MemberData(nameof(NonceStoreItemEqualityData))]
        public void NonceStoreItemEquality(NonceStoreItem lhs, NonceStoreItem rhs, bool expectEqual)
        {
            var result = !(lhs != rhs);
            if (lhs != null && rhs != null)
            {
                PAssert.That(() => lhs.GetHashCode() == rhs.GetHashCode() == expectEqual);
            }
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
            PAssert.That(() => new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now, "n")));
            PAssert.That(() => new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow, "n")));
            PAssert.That(() => !new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now.AddHours(-1), "n")));
            PAssert.That(() => !new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow.AddHours(-1), "n")));
            PAssert.That(() => !new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now.AddHours(1), "n")));
            PAssert.That(() => !new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow.AddHours(1), "n")));
        }

        [Fact]
        public void IsNotKnown()
        {
            foreach (var now in new[] {DateTime.UtcNow, DateTime.Now})
            {
                var sut = new NonceStore();
                PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n1")));
                PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n2")));
                PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c2", now.AddSeconds(1), "n1")));
                PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(2), "n1")));
                PAssert.That(() => !sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n1")));
                PAssert.That(() => !sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n2")));
                PAssert.That(() => !sut.IsOriginal(new NonceStoreItem("c2", now.AddSeconds(1), "n1")));
                PAssert.That(() => !sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(2), "n1")));
            }
        }

        [Fact]
        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            var sut = new NonceStore(TimeSpan.FromSeconds(0.5), 1);
            PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c", now, "n")));
            Thread.Sleep(TimeSpan.FromSeconds(1));
            PAssert.That(() => !sut.IsOriginal(new NonceStoreItem("c", now, "n")));
            PAssert.That(() => sut.IsOriginal(new NonceStoreItem("c", DateTime.UtcNow, "n")));
        }
    }
}