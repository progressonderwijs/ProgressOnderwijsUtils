using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Represents a half-open integer range from Begin (inclusive) to End (exclusive).  E.g. [3, 8) means 3,4,5,6,7
    /// </summary>
    public struct IntegerRange : IEquatable<IntegerRange>
    {
        /// <summary>
        /// The (inclusive) beginning of the range of integers.
        /// </summary>
        public readonly int Begin;

        /// <summary>
        /// The (exclusive) beginning of the range of integers.
        /// </summary>
        public readonly int End;

        /// <summary>
        /// Constructs a half-open integer range from begin (inclusive) to end (exclusive).  E.g. [3, 8) means 3,4,5,6,7
        /// </summary>
        public IntegerRange(int begin, int end)
        {
            Begin = begin;
            End = end;
        }

        public IEnumerable<IntegerRange> Subdivide(int batchCount)
        {
            var doneUpto = Begin;
            var batchIndex = 0;
            var rangeSize = End - Begin;

            while (batchIndex < batchCount) {
                batchIndex++;
                int nextSplit = Begin + (int)(rangeSize * (long)batchIndex / batchCount);
                yield return new IntegerRange(doneUpto, nextSplit);
                doneUpto = nextSplit;
            }
        }

        public override string ToString() => $"[{Begin}, {End})";
        public bool Equals(IntegerRange other) => Begin == other.Begin && End == other.End;
        public override int GetHashCode() => Begin * 397 + End;

        public override bool Equals(object obj)
        {
            var typed = obj as IntegerRange?;
            return typed.HasValue && Equals(typed.Value);
        }
    }

    public class IntegerRangeTests
    {
        [Test]
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

        [Test]
        public void SubdividingAcrossZeroWorks()
        {
            PAssert.That(() => new IntegerRange(-3, 7).Subdivide(2).First().Equals(new IntegerRange(-3, 2)));
            PAssert.That(() => new IntegerRange(-30, -20).Subdivide(2).SequenceEqual(
                new[] {
                    new IntegerRange(-30, -25),
                    new IntegerRange(-25, -20),
                }));
        }

        [Test]
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
    }
}
