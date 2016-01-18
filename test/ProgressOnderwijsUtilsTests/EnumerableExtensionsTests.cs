﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class EnumerableExtensionsTests
    {
#pragma warning disable 1720
        [Test]
        public void IndexOfCheck()
        {
            // ReSharper disable NotAccessedVariable
            int ignore;
            // ReSharper restore NotAccessedVariable
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(2) == 1);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(4) == -1);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(1) == 0);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(3) == 2);
            PAssert.That(() => new[] { 1, 2, 3, 1, 2, 3 }.IndexOf(3) == 2);
            Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(2));
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(x => x == 2) == 1);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(x => x % 7 == 0) == -1);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(x => x < 3) == 0);
            PAssert.That(() => new[] { 1, 2, 3 }.IndexOf(x => x % 2 == 1 && x > 1) == 2);
            PAssert.That(() => new[] { 1, 2, 3, 1, 2, 3 }.IndexOf(x => x % 2 == 1 && x > 1) == 2);
            Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(x => x == 2));
            Assert.Throws<ArgumentNullException>(() => ignore = default(int[]).IndexOf(null));
        }

        [Test]
        public void AsReadOnlyTest()
        {
            var nums = Enumerable.Range(1, 5).Reverse().ToArray();
            var copy = nums.ToReadOnly();
            var view = nums.
                AsReadOnlyView();
            PAssert.That(() => nums.SequenceEqual(copy) && nums.SequenceEqual(view));
            Array.Sort(nums);
            PAssert.That(() => nums.SequenceEqual(view) && !nums.SequenceEqual(copy));
            PAssert.That(() => copy.SequenceEqual(Enumerable.Range(1, 5).Reverse()));
            PAssert.That(() => view.SequenceEqual(Enumerable.Range(1, 5)));
        }

        [Test]
        public void testIndexOf()
        {
            var lst = new List<string> { "een", "twee", "drie" };
            //int[] ints = { 1, 2, 3, 4, 5 };
            Assert.That(lst.IndexOf("twee"), Is.EqualTo(1));
            Assert.That(lst.IndexOf("tweeeneenhalf"), Is.EqualTo(-1));
        }

        [Test]
        public void testFirstIndexOfDups()
        {
            Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(0), Is.EqualTo(0));
            Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(1), Is.EqualTo(2));
            Assert.That(() => new[] { 0, 0, 1, 1, 2, 2 }.IndexOf(2), Is.EqualTo(4));
        }

        [Test]
        public void EmptyIfNullOk()
        {
#pragma warning disable 1720
            PAssert.That(() => new[] { 0, 1, 2, }.EmptyIfNull().SequenceEqual(new[] { 0, 1, 2, }));
            PAssert.That(() => default(int[]).EmptyIfNull().SequenceEqual(new int[] { }));
            PAssert.That(() => default(int[]) == null);
            PAssert.That(() => default(int[]) != default(int[]).EmptyIfNull());
            var arr = new[] { 0, 1, 2, };
            PAssert.That(() => arr.EmptyIfNull() == arr);
#pragma warning restore 1720
        }
    }
}
