using System;
using System.Collections.Generic;

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
            var rangeSize = (long)End - Begin;
            for (var batchIndex = 1L; batchIndex <= batchCount; batchIndex++) {
                var nextSplit = Begin + (int)(rangeSize * batchIndex / batchCount);
                yield return new IntegerRange(doneUpto, nextSplit);
                doneUpto = nextSplit;
            }
        }

        public override string ToString() => $"[{Begin}, {End})";
        public bool Equals(IntegerRange other) => Begin == other.Begin && End == other.End;
        public override int GetHashCode() => Begin * 397 + End;

        public override bool Equals(object obj)
        {
            return obj is IntegerRange typed && Equals(typed);
        }
    }
}
