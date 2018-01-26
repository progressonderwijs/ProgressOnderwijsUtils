using System.Collections.Generic;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class DictionaryExtensionsTests
    {
        [Fact]
        public void GetDefault()
        {
            var sut = new Dictionary<int, int> {{0, 0}};
            PAssert.That(() => sut.GetOrDefault(0, 1) == 0);
            PAssert.That(() => sut.GetOrDefault(1, 2) == 2);
            PAssert.That(() => !sut.ContainsKey(1));
        }

        [Fact]
        public void SetDefault()
        {
            IDictionary<int, int> sut = new Dictionary<int, int> {{0, 0}};
            PAssert.That(() => sut.GetOrAdd(0, 1) == 0);
            PAssert.That(() => sut.GetOrAdd(1, 2) == 2);
            PAssert.That(() => sut.ContainsKey(1));
        }
    }
}