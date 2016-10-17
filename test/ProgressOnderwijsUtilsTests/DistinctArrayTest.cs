﻿using System;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class DistinctArrayTest
    {
        [Test]
        public void Initialize_with_empty_array_gives_count_0()
        {
            var sut = DistinctArray<int>.Empty;

            PAssert.That(() => sut.Count == 0);
        }

        [Test]
        public void Created_DistinctArray_from_distinct_is_same()
        {
            var distinct = new[] { 1, 2 };
            var sut = distinct.ToDistinctArrayFromDistinct();

            PAssert.That(() => sut.SetEqual(distinct));
        }

        [Test]
        public void Created_DistinctArray_from_distinct_with_custom_comparer_is_same()
        {
            var distinct = new[] { 1, 2 };
            var sut = distinct.ToDistinctArrayFromDistinct(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode()));

            PAssert.That(() => sut.SetEqual(distinct));
        }

        [Test]
        public void Creating_DistinctArray_from_not_distinct_gives_error()
        {
            Assert.Catch<ArgumentException>(() => new[] { 1, 1, 2 }.ToDistinctArrayFromDistinct());
        }

        [Test]
        public void Creating_DistinctArray_from_not_distinct_with_custom_comparer_gives_error()
        {
            Assert.Catch<ArgumentException>(() => new[] { 1, 1, 2 }.ToDistinctArrayFromDistinct(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode())));
        }

        [Test]
        public void Created_DistinctArray_is_distinct()
        {
            var sut = new[] { 1, 1, 2}.ToDistinctArray();

            PAssert.That(() => sut.Count == 2);
            PAssert.That(() => sut.SetEqual(new[] { 1, 2 }));
        }

        [Test]
        public void Created_DistinctArray_with_custom_comparer_is_distinct()
        {
            var sut = new[] { 1, 1, 2 }.ToDistinctArray(new EqualsEqualityComparer<int>((a, b) => a == b, obj => obj.GetHashCode()));

            PAssert.That(() => sut.Count == 2);
            PAssert.That(() => sut.SetEqual(new[] { 1, 2 }));
        }

        [Test]
        public void Indexing_all_items_gives_equals_set()
        {
            var sut = new[] { 1, 1, 2 }.ToDistinctArray();

            PAssert.That(() => sut.Select((item, i) => sut[i]).SetEqual(sut));
        }
    }
}
