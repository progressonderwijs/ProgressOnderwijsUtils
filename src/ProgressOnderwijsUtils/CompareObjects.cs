using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpressionToCodeLib;
using System.Globalization;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class DeepEquals
	{
		public static bool AreEqual(object object1, object object2)
		{
			return Compare(new HashSet<ReferencePair>(), object1, object2);
		}

		static bool Compare(HashSet<ReferencePair> assumeEquals, object object1, object object2)
		{
			if (ReferenceEquals(object1, object2)) return true;
			else if (object1 == null || object2 == null) return false; //not equal if one of the two is null

			ReferencePair pair = new ReferencePair(object1, object2);
			if (assumeEquals.Contains(pair)) return true;
			else assumeEquals.Add(pair);

			Type type = object1.GetType();

			if (type.IsPrimitive || type.IsEnum) return object1.Equals(object2);
			if (object1 is IDictionary && object2 is IDictionary)
				return CompareDictionaries(assumeEquals, (IDictionary)object1, (IDictionary)object2);


			if (type == object2.GetType())
			{
				MethodInfo builtin = TypeBuiltinEquals(type);
				if(builtin != null)
					return CompareWithBuiltinEquals(builtin, object1, object2);
			}

			return  CompareAsEnumerableOrAccessibleMembers(assumeEquals, type, object1, object2);
			
		}

		public class ReferencePair : IEquatable<ReferencePair>
		{
			readonly object a, b;
			public ReferencePair(object o1, object o2) { a = o1; b = o2; }
			public override bool Equals(object obj)
			{
				return Equals(obj as ReferencePair);
			}
			public bool Equals(ReferencePair other)
			{
				return other != null && ReferenceEquals(a, other.a) && ReferenceEquals(b, other.b);
			}
			public override int GetHashCode()
			{
				return RuntimeHelpers.GetHashCode(a) + 137 * RuntimeHelpers.GetHashCode(b);
			}
			public static bool operator ==(ReferencePair a, ReferencePair b)
			{
				return ReferenceEquals(a, b) || !object.ReferenceEquals(a, null) && a.Equals(b);
			}
			public static bool operator !=(ReferencePair a, ReferencePair b)
			{
				return !(a == b);
			}
		}

		struct AccessibleMember
		{
			public Type DeclaredType;
			public Func<object, object> Getter;
		}

		static IEnumerable<AccessibleMember> GetGetters(Type type)
		{
			var propertyMembers = type.GetProperties().Where(pi => pi.CanRead && pi.GetIndexParameters().Length == 0).Select(pi => new AccessibleMember { DeclaredType = pi.PropertyType, Getter = obj => pi.GetValue(obj, null), });
			var fieldMembers = type.GetFields().Select(fi => new AccessibleMember { DeclaredType = fi.FieldType, Getter = fi.GetValue, });
			return fieldMembers.Concat(propertyMembers).ToArray();
		}

		static bool CompareAsEnumerableOrAccessibleMembers(HashSet<ReferencePair> assumeEqual, Type type, object o1_nonnull, object o2_nonnull)
		{
			if (o1_nonnull is IEnumerable && o2_nonnull is IEnumerable)
				return CompareEnumerables(assumeEqual, (IEnumerable)o1_nonnull, (IEnumerable)o2_nonnull);
			else if (o2_nonnull.GetType() != type)
				return false;

			foreach (var member in GetGetters(type))
			{
				object v1 = member.Getter(o1_nonnull), v2 = member.Getter(o2_nonnull);
				if (!member.DeclaredType.IsValueType && assumeEqual.Contains(new ReferencePair(v1, v2)))
					continue;
				if (!Compare(assumeEqual, v1, v2))
					return false;
			}
			return true;
		}

		static MethodInfo TypeBuiltinEquals(Type type)
		{
			return type.GetMethod("Equals", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.ExactBinding, null, new[] { type }, null);
		}

		static bool CompareWithBuiltinEquals(MethodInfo builtinEquals, object o1_nonnull, object o2) { return (bool)builtinEquals.Invoke(o1_nonnull, new[] { o2 }); }

		static bool CompareDictionaries(HashSet<ReferencePair> assumeEqual, IDictionary iDict1, IDictionary iDict2)
		{
			if (iDict1.Count != iDict2.Count) return false;
			return iDict1.Keys.Cast<object>().All(dict1key => iDict2.Contains(dict1key) && Compare(assumeEqual, iDict1[dict1key], iDict2[dict1key]));
		}

		static bool CompareEnumerables(HashSet<ReferencePair> assumeEqual, IEnumerable ilist1, IEnumerable ilist2)
		{
			return ilist1.Cast<object>().Zip(ilist2.Cast<object>(), (el1, el2) => Compare(assumeEqual, el1, el2)).All(b => b);
		}
	}

	[TestFixture]
	public class DeepEqualsTest
	{
		[Test]
		public void AnonTypes()
		{
			PAssert.That(() => DeepEquals.AreEqual(new { XYZ = "123", BC = 3m }, new { XYZ = 123m.ToString(CultureInfo.InvariantCulture), BC = 3m }));
			PAssert.That(() =>!DeepEquals.AreEqual(new { XYZ = "123", BC = 3m }, new { XYZ = 123, BC = 3m }));
			PAssert.That(() =>!DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3,3), BC = 3m }, new { XYZ = new[]{3,4,5}, BC = 3m }));//members must be of same type
			PAssert.That(() => DeepEquals.AreEqual(new { XYZ = Enumerable.Range(3, 3).AsEnumerable(), BC = 3m }, new { XYZ = new[] { 3, 4, 5 }.AsEnumerable(), BC = 3m }));//OK, members are of same type
		}
		[Test]
		public void Sequences()
		{
			var q1 = 
					from i in Enumerable.Range(3, 3)
					from j in Enumerable.Range(13, 7)
					where j % i != 0
					orderby i descending,j
					select new { I = i, J = j };

			var q3 = 
					 from j in Enumerable.Range(13, 7)
					 from i in Enumerable.Range(3, 3).Where(i=> j % i != 0)
					 orderby i descending, j
					 select new { I = i, J = j };


			PAssert.That(() => DeepEquals.AreEqual(Enumerable.Range(3, 3).ToArray(), new[] { 3, 4, 5 }));
			PAssert.That(() => !Enumerable.Range(3, 3).ToArray().Equals(new[] { 3, 4, 5 }));//plain arrays are comparable
			PAssert.That(() => DeepEquals.AreEqual(Enumerable.Range(3, 3), new[] { 3, 4, 5 }));//sequences must be of same type.

			PAssert.That(() => DeepEquals.AreEqual(q1,q3));
			PAssert.That(() => !DeepEquals.AreEqual(q1.Reverse(), q3));//order matters;

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

			PAssert.That(() => !DeepEquals.AreEqual(q1, q3));//sequences aren't equal
			PAssert.That(() => DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J), q3.ToDictionary(x => x.I * x.J)));//but dictionaries are...
			PAssert.That(() => !DeepEquals.AreEqual(q1.ToDictionary(x => x.I * x.J).ToArray(), q3.ToDictionary(x => x.I * x.J).ToArray()));//..and not because they sort.
		}

	}

}