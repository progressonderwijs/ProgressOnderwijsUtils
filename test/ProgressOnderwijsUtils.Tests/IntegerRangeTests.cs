using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class IntegerRangeTests
    {
        [Fact]
        public void SubdividingIntoEvenlyDivisibleChunks()
        {
            PAssert.That(() => new IntegerRange(0, 10).Subdivide(2).First().Equals(new IntegerRange(0, 5)));
            PAssert.That(() => new IntegerRange(0, 33).Subdivide(3).First().Equals(new IntegerRange(0, 11)));
            PAssert.That(() => new IntegerRange(10, 25).Subdivide(5).SequenceEqual(
                new[] {
                    new IntegerRange(10, 13),
                    new IntegerRange(13, 16),
                    new IntegerRange(16, 19),
                    new IntegerRange(19, 22),
                    new IntegerRange(22, 25),
                }));
        }

        [Fact]
        public void SubdividingAcrossZeroWorks()
        {
            PAssert.That(() => new IntegerRange(-3, 7).Subdivide(2).First().Equals(new IntegerRange(-3, 2)));
            PAssert.That(() => new IntegerRange(-30, -20).Subdivide(2).SequenceEqual(
                new[] {
                    new IntegerRange(-30, -25),
                    new IntegerRange(-25, -20),
                }));
        }

        [Fact]
        public void SubdividingIrregularAlternatesBlockSizes()
        {
            PAssert.That(() => new IntegerRange(0, 10).Subdivide(4).SequenceEqual(
                new[] {
                    new IntegerRange(0, 2),
                    new IntegerRange(2, 5),
                    new IntegerRange(5, 7),
                    new IntegerRange(7, 10),
                }));
        }

        [Fact]
        public void SubdividingAvoidsIntegerOverflow()
        {
            PAssert.That(() => new IntegerRange(int.MinValue, int.MaxValue).Subdivide(4).SequenceEqual(
                new[] {
                    new IntegerRange(int.MinValue, -(1 << 30) - 1),
                    new IntegerRange(-(1 << 30) - 1, -1),
                    new IntegerRange(-1, (1 << 30) - 1),
                    new IntegerRange((1 << 30) - 1, int.MaxValue),
                }));
        }
    }
}
