using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SListTest
    {
        [Test]
        public void CanContainValues()
        {
            var list = SList.Create(new[] { 1, 3, 4, 1 });
            PAssert.That(() => list.SequenceEqual(new[] { 1, 3, 4, 1 }));
        }

        [Test]
        public void EmptyIsEmpty()
        {
            var list = SList.Create(new int[] { });
            PAssert.That(() => list.IsEmpty);
        }

        [Test]
        public void EmptyIsEmpty2()
        {
            var list = SList<int>.Empty;
            PAssert.That(() => list.IsEmpty);
        }

        [Test]
        public void PrependTest()
        {
            var list = SList<int>.Empty.Prepend(3).Prepend(4);
            PAssert.That(() => list.SequenceEqual(new[] { 4, 3 }));
        }

        [Test]
        public void EqualsTest()
        {
            var a = SList.Create(new[] { 1, 2, 3 });
            var b = SList<int>.Empty.Prepend(3).Prepend(2).Prepend(1);
            var c = SList.Create(new[] { 1, 2, 3, 4 });

            PAssert.That(() => a == b);
            PAssert.That(() => a.GetHashCode() == b.GetHashCode());
            PAssert.That(() => a.Equals(b));
            PAssert.That(() => a.Equals((object)b));
            PAssert.That(() => !(a != b));
            PAssert.That(() => a != c);
            PAssert.That(() => !(a == c));
            PAssert.That(() => !a.Equals(c));
            PAssert.That(() => !a.Equals((object)c));
            PAssert.That(() => !a.Equals(SList<int>.Empty));
        }

        [Test]
        public void ReverseTest()
        {
            var a = SList.Create(new[] { 1, 2, 3 });
            var b = SList<int>.Empty.Prepend(1).Prepend(2).Prepend(3).Reverse();

            PAssert.That(() => a == b);
        }

        [Test]
        public void EnumeratorTest()
        {
            var b = SList<int>.Empty.Prepend(1).Prepend(2).Prepend(3).Reverse();

            PAssert.That(() => StructuralComparisons.StructuralEqualityComparer.Equals(b.ToArray(), new[] { 1, 2, 3 }));
        }

        [Test]
        public void TailTest()
        {
            var list = SList.Create(new[] { 1, 2, 3 });

            PAssert.That(() => list.Tail.Tail.Tail.IsEmpty);
            PAssert.That(() => list.Tail.Prepend(1) == list);
            Assert.Throws<NullReferenceException>(() => { var x = list.Tail.Tail.Tail.Tail; });
        }

        [Test]
        public void HeadTest()
        {
            var list = SList.Create(new[] { 1, 2, 3 });

            PAssert.That(() => list.Head == 1);
            PAssert.That(() => list.Tail.Head == 2);
            PAssert.That(() => list.Tail.Tail.Head == 3);
            Assert.Throws<NullReferenceException>(() => { var x = list.Tail.Tail.Tail.Head; });
        }

        [Test]
        public void HashReasonableness()
        {
            var list = SList.Create(Enumerable.Range(0, 100).Select(i => i * 14678355468 ^ i));
            List<int> hashcodes = new List<int>();
            for (var suffix = list; !suffix.IsEmpty; suffix = suffix.Tail) {
                hashcodes.Add(suffix.GetHashCode());
            }

            PAssert.That(() => hashcodes.Count == hashcodes.Distinct().Count());
        }

        [Test]
        public void HashDiffersPerType()
        {
            PAssert.That(() => SList<int>.Empty.GetHashCode() != SList<double>.Empty.GetHashCode());
        }

        [Test]
        public void HashConsistentWithEquals()
        {
            var lists = new[] {
                SList.Create(new[] { 1, 2, 3 }),
                SList.Create(new[] { 1, 2, 3 }).Reverse().Reverse(),
                SList.Create(new[] { 1, 2, 3, 3 }).Reverse().Reverse(),
                SList.Create(new[] { 1, 2, 3, 3 }),
                SList.Create(new[] { 1, 2, 1 }),
                SList.Create(new[] { 1, 2, 1 }).Reverse(),
            };

            PAssert.That(
                () =>
                    (from aList in lists
                        from bList in lists
                        select aList == bList == (aList.GetHashCode() == bList.GetHashCode())).All(x => x));
        }
    }
}
