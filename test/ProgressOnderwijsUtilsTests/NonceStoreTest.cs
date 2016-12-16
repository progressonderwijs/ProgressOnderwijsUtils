using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public class NonceStoreTest
    {
        static IEnumerable<TestCaseData> NonceStoreItemEqualityData()
        {
            yield return new TestCaseData(null, null).Returns(true);
            yield return new TestCaseData(null, new NonceStoreItem("c", null, "n")).Returns(false);
            yield return new TestCaseData(new NonceStoreItem("c", null, "n"), null).Returns(false);
            yield return new TestCaseData(new NonceStoreItem("c", null, "n"), new NonceStoreItem("c", null, "n")).Returns(true);
            yield return new TestCaseData(new NonceStoreItem("c", null, "n"), new NonceStoreItem("c", null, "0")).Returns(false);
            yield return new TestCaseData(new NonceStoreItem("c", null, "n"), new NonceStoreItem("1", null, "n")).Returns(false);
            yield return new TestCaseData(new NonceStoreItem("c", null, "0"), new NonceStoreItem("c", null, "n")).Returns(false);
            yield return new TestCaseData(new NonceStoreItem("1", null, "n"), new NonceStoreItem("c", null, "n")).Returns(false);
        }

        [Test, TestCaseSource(nameof(NonceStoreItemEqualityData))]
        public bool NonceStoreItemEquality(NonceStoreItem lhs, NonceStoreItem rhs)
        {
            var result = !(lhs != rhs);
            if (lhs != null && rhs != null) {
                Assert.That(lhs.GetHashCode() == rhs.GetHashCode(), Is.EqualTo(result));
            }
            return result;
        }

        [Test]
        public void Generate()
        {
            var sut = new NonceStore();
            Assert.That(sut.Generate(), Is.Not.EqualTo(sut.Generate()));
        }

        [Test] // we cannot use TestCaseSource as it will be executed upon loading, and these tests much later!
        public void IsInWindow()
        {
            Assert.That(new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now, "n")));
            Assert.That(new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow, "n")));
            Assert.That(!new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now.AddHours(-1), "n")));
            Assert.That(!new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow.AddHours(-1), "n")));
            Assert.That(!new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.Now.AddHours(1), "n")));
            Assert.That(!new NonceStore().IsOriginal(new NonceStoreItem("c", DateTime.UtcNow.AddHours(1), "n")));
        }

        [Test]
        public void IsNotKnown([Values(false, true)] bool utc)
        {
            var now = utc ? DateTime.UtcNow : DateTime.Now;

            var sut = new NonceStore();
            Assert.That(sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n1")));
            Assert.That(sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n2")));
            Assert.That(sut.IsOriginal(new NonceStoreItem("c2", now.AddSeconds(1), "n1")));
            Assert.That(sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(2), "n1")));
            Assert.That(!sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n1")));
            Assert.That(!sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(1), "n2")));
            Assert.That(!sut.IsOriginal(new NonceStoreItem("c2", now.AddSeconds(1), "n1")));
            Assert.That(!sut.IsOriginal(new NonceStoreItem("c1", now.AddSeconds(2), "n1")));
        }

        [Test]
        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            var sut = new NonceStore(new TimeSpan(0, 0, 1), 1);
            Assert.That(sut.IsOriginal(new NonceStoreItem("c", now, "n")));
            Thread.Sleep(1000);
            Assert.That(!sut.IsOriginal(new NonceStoreItem("c", now, "n")));
            Assert.That(sut.IsOriginal(new NonceStoreItem("c", DateTime.UtcNow, "n")));
        }
    }
}
