﻿using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;

namespace ProgressOnderwijsUtils.Test
{
    public class DictionaryMergeShould
    {
        [Test]
        public void ReturnTheSameDictionaryWhenMergingWithEmpty()
        {
            var original = new Dictionary<int, string>() {
                { 1, "foo" },
                { 2, "bar" }
            };
            var empty = new Dictionary<int, string>();
            PAssert.That(() => original.Merge(empty).SequenceEqual(original));
        }

        [Test]
        public void ReturnsContentOfBothDictionaries()
        {
            var first = new Dictionary<int, string>() {
                { 1, "foo" },
            };
            var second = new Dictionary<int, string>() {
                { 2, "bar" }
            };
            var combined = new Dictionary<int, string>() {
                { 1, "foo" },
                { 2, "bar" }
            };
            PAssert.That(() => first.Merge(second).SequenceEqual(combined));
        }

        [Test]
        public void ReturnsValueOfLastDictionaryWhenBothDictionariesContainSameKey()
        {
            var first = new Dictionary<int, string>() {
                { 2, "foo" },
            };
            var second = new Dictionary<int, string>() {
                { 2, "bar" }
            };
            var combined = new Dictionary<int, string>() {
                { 2, "bar" }
            };
            PAssert.That(() => first.Merge(second).SequenceEqual(combined));
        }

        [Test]
        public void ReturnsTheResultOfMultipleMergedArrays()
        {
            var first = new Dictionary<int, string>() {
                { 1, "foo" },
            };
            var second = new Dictionary<int, string>() {
                { 2, "bar" }
            };
            var third = new Dictionary<int, string>() {
                { 3, "baz" }
            };
            var combined = new Dictionary<int, string>() {
                { 1, "foo" },
                { 2, "bar" },
                { 3, "baz" }
            };
            PAssert.That(() => first.Merge(second, third).SequenceEqual(combined));
        }
    }
}
