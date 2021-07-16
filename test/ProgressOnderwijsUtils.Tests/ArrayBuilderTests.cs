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
            for (var size = 0; size < 100; size++) {
                var builder = new ArrayBuilder<int>();
                for (var i = 0; i < size; i++) {
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
            for (var i = 0; i < size; i++) {
                builder.Add(i);
            }
            Assert.Equal(Enumerable.Range(0, size), builder.ToArray());
        }

        [Fact]
        public void ExhaustivelyTestStringArraysOfSmallSize()
        {
            for (var size = 0; size < 100; size++) {
                var builder = new ArrayBuilder<string>();
                for (var i = 0; i < size; i++) {
                    builder.Add(i.ToString());
                }
                Assert.Equal(Enumerable.Range(0, size).Select(n => n.ToString()), builder.ToArray());
            }
        }

        [Fact(Skip = "slow and tricky on 32-bit runners.")]
        public void TestMaxSizeArray()
        {
            var builder = new ArrayBuilder<byte>();
            var approxMaxSize = int.MaxValue - 100;
            for (uint i = 0; i < approxMaxSize; i++) {
                builder.Add((byte)i);
            }
            var twoGbArray = builder.ToArray();
            Assert.Equal(approxMaxSize, twoGbArray.Length);
            Assert.Equal(37, twoGbArray[37]);
            Assert.Equal(1234567 % 256, twoGbArray[1234567]);
            Assert.Equal(approxMaxSize - 1 & 0xff, twoGbArray[approxMaxSize - 1]);
        }
    }
}
