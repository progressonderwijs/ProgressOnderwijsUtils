using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
    public static class DeepEquals
    {
        public static bool AreEqual(object object1, object object2) { return Compare(new HashSet<ReferencePair>(), object1, object2); }

        static bool Compare(HashSet<ReferencePair> assumeEquals, object object1, object object2)
        {
            if (ReferenceEquals(object1, object2)) {
                return true;
            } else if (object1 == null || object2 == null) {
                return false; //not equal if one of the two is null
            }

            ReferencePair pair = new ReferencePair(object1, object2);
            if (assumeEquals.Contains(pair)) {
                return true;
            } else {
                assumeEquals.Add(pair);
            }

            Type type = object1.GetType();

            if (type.IsPrimitive || type.IsEnum) {
                return object1.Equals(object2);
            }
            if (object1 is IDictionary && object2 is IDictionary) {
                return CompareDictionaries(assumeEquals, (IDictionary)object1, (IDictionary)object2);
            }

            if (type == object2.GetType()) {
                MethodInfo builtin = TypeBuiltinEquals(type);
                if (builtin != null) {
                    return CompareWithBuiltinEquals(builtin, object1, object2);
                }
            }

            return CompareAsEnumerableOrAccessibleMembers(assumeEquals, type, object1, object2);
        }

        public class ReferencePair : IEquatable<ReferencePair>
        {
            readonly object a, b;

            public ReferencePair(object o1, object o2)
            {
                a = o1;
                b = o2;
            }

            public override bool Equals(object obj) { return Equals(obj as ReferencePair); }
            public bool Equals(ReferencePair other) { return other != null && ReferenceEquals(a, other.a) && ReferenceEquals(b, other.b); }
            public override int GetHashCode() => RuntimeHelpers.GetHashCode(a) + 137 * RuntimeHelpers.GetHashCode(b);
            public static bool operator ==(ReferencePair a, ReferencePair b) { return ReferenceEquals(a, b) || !object.ReferenceEquals(a, null) && a.Equals(b); }
            public static bool operator !=(ReferencePair a, ReferencePair b) { return !(a == b); }
        }

        struct AccessibleMember
        {
            public Type DeclaredType;
            public Func<object, object> Getter;
        }

        static IEnumerable<AccessibleMember> GetGetters(Type type)
        {
            var propertyMembers =
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(pi => pi.CanRead && pi.GetIndexParameters().Length == 0)
                    .Select(pi => new AccessibleMember { DeclaredType = pi.PropertyType, Getter = obj => pi.GetValue(obj, null), });
            var fieldMembers = type.GetFields().Select(fi => new AccessibleMember { DeclaredType = fi.FieldType, Getter = fi.GetValue, });
            return fieldMembers.Concat(propertyMembers).ToArray();
        }

        static bool CompareAsEnumerableOrAccessibleMembers(HashSet<ReferencePair> assumeEqual, Type type, object o1_nonnull, object o2_nonnull)
        {
            if (o1_nonnull is IEnumerable && o2_nonnull is IEnumerable) {
                return CompareEnumerables(assumeEqual, (IEnumerable)o1_nonnull, (IEnumerable)o2_nonnull);
            } else if (o2_nonnull.GetType() != type) {
                return false;
            }

            foreach (var member in GetGetters(type)) {
                object v1 = member.Getter(o1_nonnull), v2 = member.Getter(o2_nonnull);
                if (!member.DeclaredType.IsValueType && assumeEqual.Contains(new ReferencePair(v1, v2))) {
                    continue;
                }
                if (!Compare(assumeEqual, v1, v2)) {
                    return false;
                }
            }
            return true;
        }

        static MethodInfo TypeBuiltinEquals(Type type)
        {
            return type.IsValueType
                ? type.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null, new[] { type }, null)
                : type.GetMethod("Equals", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null, new[] { type }, null);
        }

        static bool CompareWithBuiltinEquals(MethodInfo builtinEquals, object o1_nonnull, object o2) { return (bool)builtinEquals.Invoke(o1_nonnull, new[] { o2 }); }

        static bool CompareDictionaries(HashSet<ReferencePair> assumeEqual, IDictionary iDict1, IDictionary iDict2)
        {
            return iDict1.Count == iDict2.Count
                && iDict1.Keys.Cast<object>().All(dict1key => iDict2.Contains(dict1key) && Compare(assumeEqual, iDict1[dict1key], iDict2[dict1key]));
        }

        static bool CompareEnumerables(HashSet<ReferencePair> assumeEqual, IEnumerable ilist1, IEnumerable ilist2)
        {
            return ilist1.Cast<object>().Zip(ilist2.Cast<object>(), (el1, el2) => Compare(assumeEqual, el1, el2)).All(b => b);
        }
    }

    [Continuous]
    public class DeepEqualsTest
    {
        [Test]
        public void RefPair()
        {
            object a = new object(), b = new object(), c = "3", d = 3.ToStringInvariant();
            Func<object, object, DeepEquals.ReferencePair> refpair = (o1, o2) => new DeepEquals.ReferencePair(o1, o2);
            PAssert.That(() => !DeepEquals.AreEqual(a, b) && DeepEquals.AreEqual(c, d));
            PAssert.That(() => refpair(a, b) == refpair(a, b) && !ReferenceEquals(refpair(a, b), refpair(a, b)));
            PAssert.That(() => refpair(a, c) != refpair(a, d) && c.Equals(d));
            PAssert.That(() => refpair(a, b).Equals(refpair(a, b)));
            PAssert.That(() => Equals(refpair(a, b), refpair(a, b)));
            PAssert.That(() => !Equals(null, Equals(refpair(a, b))));
            PAssert.That(() => !Equals(a, Equals(refpair(a, b))));
            PAssert.That(() => !Equals(refpair(a, b), b));
            PAssert.That(() => refpair(null, b).Equals(refpair(null, b)));
            PAssert.That(() => refpair(a, null).Equals(refpair(a, null)));
            PAssert.That(() => !refpair(a, b).Equals(refpair(a, null)));
            PAssert.That(() => !refpair(a, b).Equals(refpair(null, b)));
        }

        [Test]
        public void AnonTypes()
        {
            PAssert.That(() => DeepEquals.AreEqual(new { XYZ = "123", BC = 3m }, new { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new { XYZ = "123", BC = 3m }, new { XYZ = 123, BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3, 3), BC = 3m }, new { XYZ = new[] { 3, 4, 5 }, BC = 3m })); //members must be of same type
            PAssert.That(() => DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3, 3).AsEnumerable(), BC = 3m }, new { XYZ = new[] { 3, 4, 5 }.AsEnumerable(), BC = 3m }));
            //OK, members are of same type
        }

        class XT
        {
            // ReSharper disable UnaccessedField.Local
#pragma warning disable 649
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
            public Recursive Next;
        }

        [Test]
        public void SimpleTypes()
        {
            PAssert.That(() => DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m }, new XT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => DeepEquals.AreEqual(new YT { XYZ = "123", BC = 3m }, new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m }, new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(null, new YT { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
            PAssert.That(() => !DeepEquals.AreEqual(new XT { XYZ = "123", BC = 3m }, null));
        }

        [Test]
        public void RecursiveTypes()
        {
            Recursive a = new Recursive { V = 3 };
            a.Next = a;
            PAssert.That(() => DeepEquals.AreEqual(a, a));

            Recursive b = new Recursive { V = 3, Next = a };
            PAssert.That(() => DeepEquals.AreEqual(a, b));

            Recursive a1 = new Recursive { V = 4 };
            Recursive b1 = new Recursive { V = 5, Next = a1 };
            a1.Next = b1;
            PAssert.That(() => !DeepEquals.AreEqual(a1, b1));

            Recursive a2 = new Recursive { V = 4 };
            Recursive b2 = new Recursive { V = 5, Next = a2 };
            a2.Next = b2;
            PAssert.That(() => DeepEquals.AreEqual(a1, a2));

            Recursive a3 = new Recursive { V = 6 };
            Recursive b3 = new Recursive { V = 6, Next = a3 };
            a3.Next = b3;
            PAssert.That(() => DeepEquals.AreEqual(a3, b3));
        }

        [Test]
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
            PAssert.That(() => DeepEquals.AreEqual(Enumerable.Range(3, 3), new[] { 3, 4, 5 })); //sequences must be of same type.

            PAssert.That(() => DeepEquals.AreEqual(q1, q3));
            PAssert.That(() => !DeepEquals.AreEqual(q1.Reverse(), q3)); //order matters;
        }

        [Test]
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
            PAssert.That(() => DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J), q3.ToDictionary(x => x.I * x.J))); //but dictionaries are...
            PAssert.That(() => !DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J), q3.Skip(1).ToDictionary(x => x.I * x.J))); //and the key-lookup is symmetric (A)
            PAssert.That(() => !DeepEquals.AreEqual(q1.Skip(1).ToDictionary(x => x.I * x.J), q3.ToDictionary(x => x.I * x.J))); //and the key-lookup is symmetric (B)
            PAssert.That(() => !DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J).ToArray(), q3.ToDictionary(x => x.I * x.J).ToArray())); //..and not because they sort.
        }
    }
}
