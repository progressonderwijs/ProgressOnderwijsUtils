#nullable disable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class SetEqualsTest
    {
        [Fact]
        public void SetEqualWorksOnEmpty()
            => PAssert.That(() => new int[0].SetEqual(new List<int>()));

        [Fact]
        public void SetEqualWorksOnEqualStringSets()
            => PAssert.That(() => new[] { "asb", "wer" }.SetEqual(new[] { "wer", "asb" }));

        [Fact]
        public void SetEqualWorksOnEqualIntSetsIgnoringCardinality()
            => PAssert.That(() => new[] { 1, 2, 4, 1 }.SetEqual(new[] { 1, 2, 2, 4 }));

        [Fact]
        public void SetEqualWorksOnUnequalStringSetsOfSameLength()
            => PAssert.That(() => !new[] { "asb", "werX" }.SetEqual(new[] { "wer", "asb" }));

        [Fact]
        public void SetEqualWorksOnUnequalStringSetsWithSubsetRelationship()
            => PAssert.That(() => !new[] { "asb", "wer", "qwe" }.SetEqual(new[] { "wer", "asb" }));

        [Fact]
        public void SetEqualWorksOnUnequalStringSetsWithSupersetRelationship()
            => PAssert.That(() => !new[] { "asb", "wer" }.SetEqual(new[] { "wer", "asb", "qwe" }));

        [Fact]
        public void SetEqualWorksOnEnums()
            => PAssert.That(() => new[] { RegexOptions.CultureInvariant, RegexOptions.Multiline }.SetEqual(new[] { RegexOptions.CultureInvariant, RegexOptions.Multiline }));

        [Fact]
        public void SetEqualWorksOnEqualStringSetsDueToComparerer()
        {
            PAssert.That(() => new[] { "asb", "wer" }.SetEqual(new[] { "wer", "aSB" }, StringComparer.OrdinalIgnoreCase));
            PAssert.That(() => !new[] { "asb", "wer" }.SetEqual(new[] { "wer", "aSB" }));
        }
    }
}
