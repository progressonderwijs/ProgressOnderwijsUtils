using System;
using System.Globalization;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public class DeepEqualsTests
    {
        [Fact]
        public void RefPair()
        {
            object a = new object(), b = new object(), c = "3", d = 3.ToStringInvariant();
            Func<object, object, DeepEquals.ReferencePair> refpair = (o1, o2) => new DeepEquals.ReferencePair(o1, o2);
            PAssert.That(() => !DeepEquals.AreEqual(a, b) && DeepEquals.AreEqual(c, d));
            // ReSharper disable EqualExpressionComparison - kind of the whole point!
            PAssert.That(() => refpair(a, b) == refpair(a, b) && !ReferenceEquals(refpair(a, b), refpair(a, b)));
            // ReSharper restore EqualExpressionComparison
            PAssert.That(() => refpair(a, c) != refpair(a, d) && c.Equals(d));
            PAssert.That(() => refpair(a, b).Equals(refpair(a, b)));
            // ReSharper disable once EqualExpressionComparison - kind of the whole point!
            PAssert.That(() => Equals(refpair(a, b), refpair(a, b)));
            // ReSharper restore EqualExpressionComparison
            PAssert.That(() => !Equals(null, Equals(refpair(a, b))));
            PAssert.That(() => !Equals(a, Equals(refpair(a, b))));
            PAssert.That(() => !Equals(refpair(a, b), b));
            PAssert.That(() => refpair(null, b).Equals(refpair(null, b)));
            PAssert.That(() => refpair(a, null).Equals(refpair(a, null)));
            PAssert.That(() => !refpair(a, b).Equals(refpair(a, null)));
            PAssert.That(() => !refpair(a, b).Equals(refpair(null, b)));
        }

        [Fact]
        public void AnonTypes()
        {
            PAssert.That(
                () =>
                    DeepEquals.AreEqual(new { XYZ = "123", BC = 3m },
                        new { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new { XYZ = "123", BC = 3m }, new { XYZ = 123, BC = 3m }));
            PAssert.That(
                () =>
                    !DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3, 3), BC = 3m },
                        new { XYZ = new[] { 3, 4, 5 }, BC = 3m })); //members must be of same type
            PAssert.That(
                () =>
                    DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3, 3).AsEnumerable(), BC = 3m },
                        new { XYZ = new[] { 3, 4, 5 }.AsEnumerable(), BC = 3m }));
            //OK, members are of same type
        }

        class XT
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            // ReSharper disable UnaccessedField.Local
#pragma warning disable 649
#pragma warning disable 414
            public decimal BC;
            public string XYZ { get; set; }
        }

        struct YT
        {
            public decimal BC;
            public string XYZ { get; set; }
        }

        class Recursive
        {
            public int V = 3;

            [UsedImplicitly] //equality via deep-equals
            public Recursive Next;
        }
#pragma warning restore 649
#pragma warning restore 414
        [Fact]
        public void SimpleTypes()
        {
            PAssert.That(
                () =>
                    DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m },
                        new XT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(
                () =>
                    DeepEquals.AreEqual(new YT { XYZ = "123", BC = 3m },
                        new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(
                () =>
                    !DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m },
                        new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(
                () => !DeepEquals.AreEqual(null, new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m }, null));
        }

        [Fact]
        public void RecursiveTypes()
        {
            var a = new Recursive { V = 3 };
            a.Next = a;
            PAssert.That(() => DeepEquals.AreEqual(a, a));

            var b = new Recursive { V = 3, Next = a };
            PAssert.That(() => DeepEquals.AreEqual(a, b));

            var a1 = new Recursive { V = 4 };
            var b1 = new Recursive { V = 5, Next = a1 };
            a1.Next = b1;
            PAssert.That(() => !DeepEquals.AreEqual(a1, b1));

            var a2 = new Recursive { V = 4 };
            var b2 = new Recursive { V = 5, Next = a2 };
            a2.Next = b2;
            PAssert.That(() => DeepEquals.AreEqual(a1, a2));

            var a3 = new Recursive { V = 6 };
            var b3 = new Recursive { V = 6, Next = a3 };
            a3.Next = b3;
            PAssert.That(() => DeepEquals.AreEqual(a3, b3));
        }

        [Fact]
        public void Sequences()
        {
            var q1 =
                from i in Enumerable.Range(3, 3)
                from j in Enumerable.Range(13, 7)
                where j % i != 0
                orderby i descending, j
                select new { I = i, J = j };

            var q3 =
                from j in Enumerable.Range(13, 7)
                from i in Enumerable.Range(3, 3).Where(i => j % i != 0)
                orderby i descending, j
                select new { I = i, J = j };

            PAssert.That(() => DeepEquals.AreEqual(Enumerable.Range(3, 3).ToArray(), new[] { 3, 4, 5 }));
            PAssert.That(() => !Enumerable.Range(3, 3).ToArray().Equals(new[] { 3, 4, 5 })); //plain arrays are comparable
            PAssert.That(() => DeepEquals.AreEqual(Enumerable.Range(3, 3), new[] { 3, 4, 5 }));
            //sequences must be of same type.

            PAssert.That(() => DeepEquals.AreEqual(q1, q3));
            PAssert.That(() => !DeepEquals.AreEqual(q1.Reverse(), q3)); //order matters;
        }

        [Fact]
        public void Dictionaries()
        {
            var q1 =
                from i in Enumerable.Range(3, 3)
                from j in Enumerable.Range(13, 2)
                where j % i != 0
                orderby i
                select new { I = i, J = j };

            var q3 =
                from j in Enumerable.Range(13, 2)
                from i in Enumerable.Range(3, 3).Where(i => j % i != 0)
                orderby i descending
                select new { I = i, J = j };

            PAssert.That(() => !DeepEquals.AreEqual(q1, q3)); //sequences aren't equal
            PAssert.That(() => DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J), q3.ToDictionary(x => x.I * x.J)));
            //but dictionaries are...
            PAssert.That(
                () => !DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J), q3.Skip(1).ToDictionary(x => x.I * x.J)));
            //and the key-lookup is symmetric (A)
            PAssert.That(
                () => !DeepEquals.AreEqual(q1.Skip(1).ToDictionary(x => x.I * x.J), q3.ToDictionary(x => x.I * x.J)));
            //and the key-lookup is symmetric (B)
            PAssert.That(
                () =>
                    !DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J).ToArray(),
                        q3.ToDictionary(x => x.I * x.J).ToArray())); //..and not because they sort.
        }
    }
}
