using System.Linq;
using ProgressOnderwijsUtils.Collections;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class ArrayBuilderTests
    {
        [Fact]
        public void ExhaustivelyTestIntArraysOfSmallSize()
        {
            for (int size = 0; size < 100; size++) {
                var builder = new ArrayBuilder<int>();
                for (int i = 0; i < size; i++) {
                    builder.Add(i);
                }
                Assert.Equal(Enumerable.Range(0, size), builder.ToArray());
            }
        }

        [InlineData(200)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(3000)]
        [InlineData(10_000)]
        [InlineData(100_000)]
        [InlineData(1000_000)]
        [InlineData(10_000_000)]
        [Theory]
        public void TestLargeIntArrays(int size)
        {
            var builder = new ArrayBuilder<int>();
            for (int i = 0; i < size; i++) {
                builder.Add(i);
            }
            Assert.Equal(Enumerable.Range(0, size), builder.ToArray());
        }

        [Fact]
        public void ExhaustivelyTestStringArraysOfSmallSize()
        {
            for (int size = 0; size < 100; size++) {
                var builder = new ArrayBuilder<string>();
                for (int i = 0; i < size; i++) {
                    builder.Add(i.ToString());
                }
                Assert.Equal(Enumerable.Range(0, size).Select(n => n.ToString()), builder.ToArray());
            }
        }
    }
}
