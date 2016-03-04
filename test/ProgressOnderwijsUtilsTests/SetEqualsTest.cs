using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace Progress.Business.Test.Tools
{
    class SetEqualsTest
    {
        [Test]
        public void SetEqualWorksOnEmpty()
        {
            PAssert.That(() => new int[0].SetEqual(new List<int>()));
        }

        [Test]
        public void SetEqualWorksOnEqualStringSets()
        {
            PAssert.That(() => new[] { "asb", "wer" }.SetEqual(new[] { "wer", "asb" }));
        }

        [Test]
        public void SetEqualWorksOnEqualIntSetsIgnoringCardinality()
        {
            PAssert.That(() => new[] { 1, 2, 4, 1 }.SetEqual(new[] { 1, 2, 2, 4 }));
        }

        [Test]
        public void SetEqualWorksOnUnequalStringSetsOfSameLength()
        {
            PAssert.That(() => !new[] { "asb", "werX" }.SetEqual(new[] { "wer", "asb" }));
        }

        [Test]
        public void SetEqualWorksOnUnequalStringSetsWithSubsetRelationship()
        {
            PAssert.That(() => !new[] { "asb", "wer", "qwe" }.SetEqual(new[] { "wer", "asb" }));
        }

        [Test]
        public void SetEqualWorksOnUnequalStringSetsWithSupersetRelationship()
        {
            PAssert.That(() => !new[] { "asb", "wer" }.SetEqual(new[] { "wer", "asb", "qwe" }));
        }

        [Test]
        public void SetEqualWorksOnEnums()
        {
            PAssert.That(() => new[] { RegexOptions.CultureInvariant, RegexOptions.Multiline }.SetEqual(new[] { RegexOptions.CultureInvariant, RegexOptions.Multiline }));
        }

        [Test]
        public void SetEqualWorksOnEqualStringSetsDueToComparerer()
        {
            PAssert.That(() => new[] { "asb", "wer" }.SetEqual(new[] { "wer", "aSB" }, StringComparer.OrdinalIgnoreCase));
            PAssert.That(() => !new[] { "asb", "wer" }.SetEqual(new[] { "wer", "aSB" }));
        }
    }
}
