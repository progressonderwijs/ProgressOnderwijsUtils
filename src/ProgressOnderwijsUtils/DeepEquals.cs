using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class DeepEquals
    {
        public static bool AreEqual(object object1, object object2) => Compare(new HashSet<ReferencePair>(), object1, object2);

        static bool Compare(HashSet<ReferencePair> assumeEquals, object object1, object object2)
        {
            if (ReferenceEquals(object1, object2)) {
                return true;
            } else if (object1 == null || object2 == null) {
                return false; //not equal if one of the two is null
            }

            var pair = new ReferencePair(object1, object2);
            if (assumeEquals.Contains(pair)) {
                return true;
            } else {
                assumeEquals.Add(pair);
            }

            var type = object1.GetType();

            if (type.IsPrimitive || type.IsEnum) {
                return object1.Equals(object2);
            }
            if (object1 is IDictionary && object2 is IDictionary) {
                return CompareDictionaries(assumeEquals, (IDictionary)object1, (IDictionary)object2);
            }

            if (type == object2.GetType()) {
                var builtin = TypeBuiltinEquals(type);
                if (builtin != null) {
                    return CompareWithBuiltinEquals(builtin, object1, object2);
                }
            }

            return CompareAsEnumerableOrAccessibleMembers(assumeEquals, type, object1, object2);
        }

        public sealed class ReferencePair : IEquatable<ReferencePair>
        {
            readonly object a, b;

            public ReferencePair(object o1, object o2)
            {
                a = o1;
                b = o2;
            }

            public override bool Equals(object obj) => Equals(obj as ReferencePair);
            public bool Equals(ReferencePair other) => other != null && ReferenceEquals(a, other.a) && ReferenceEquals(b, other.b);
            public override int GetHashCode() => RuntimeHelpers.GetHashCode(a) + 137 * RuntimeHelpers.GetHashCode(b);

            public static bool operator ==([CanBeNull] ReferencePair a, [CanBeNull] ReferencePair b)
            {
                return ReferenceEquals(a, b) || !ReferenceEquals(a, null) && a.Equals(b);
            }

            public static bool operator !=([CanBeNull] ReferencePair a, [CanBeNull] ReferencePair b)
            {
                return !(a == b);
            }
        }

        struct AccessibleMember
        {
            public Type DeclaredType;
            public Func<object, object> Getter;
        }

        [NotNull]
        static IEnumerable<AccessibleMember> GetGetters([NotNull] Type type)
        {
            var propertyMembers =
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(pi => pi.CanRead && pi.GetIndexParameters().Length == 0)
                    .Select(pi => new AccessibleMember { DeclaredType = pi.PropertyType, Getter = obj => pi.GetValue(obj, null), });
            var fieldMembers = type.GetFields().Select(fi => new AccessibleMember { DeclaredType = fi.FieldType, Getter = fi.GetValue, });
            return fieldMembers.Concat(propertyMembers).ToArray();
        }

        static bool CompareAsEnumerableOrAccessibleMembers(HashSet<ReferencePair> assumeEqual, Type type, object o1_nonnull, [NotNull] object o2_nonnull)
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

        [CanBeNull]
        static MethodInfo TypeBuiltinEquals([NotNull] Type type)
        {
            return type.IsValueType
                ? type.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null, new[] { type }, null)
                : type.GetMethod("Equals", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null, new[] { type }, null);
        }

        static bool CompareWithBuiltinEquals([NotNull] MethodInfo builtinEquals, object o1_nonnull, object o2) => (bool)builtinEquals.Invoke(o1_nonnull, new[] { o2 });

        static bool CompareDictionaries(HashSet<ReferencePair> assumeEqual, [NotNull] IDictionary iDict1, [NotNull] IDictionary iDict2)
        {
            return iDict1.Count == iDict2.Count
                && iDict1.Keys.Cast<object>().All(dict1key => iDict2.Contains(dict1key) && Compare(assumeEqual, iDict1[dict1key], iDict2[dict1key]));
        }

        static bool CompareEnumerables(HashSet<ReferencePair> assumeEqual, [NotNull] IEnumerable ilist1, [NotNull] IEnumerable ilist2)
        {
            return ilist1.Cast<object>().Zip(ilist2.Cast<object>(), (el1, el2) => Compare(assumeEqual, el1, el2)).All(b => b);
        }
    }
}
