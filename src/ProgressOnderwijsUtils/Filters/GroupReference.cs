using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	[Serializable]
	public class GroupReference : IEquatable<GroupReference>
	{
		public GroupReference(int groupid, string name)
		{
			GroupId = groupid;
			Name = name;
		}

		public int GroupId { get; private set; }
		public string Name { get; private set; }

		public bool Equals(GroupReference other)
		{
			return other != null && other.GroupId == GroupId;
		}

		public override bool Equals(object obj)
		{
			return obj is GroupReference && Equals(obj as GroupReference);
		}

		public override int GetHashCode()
		{
			return GroupId;
		}

		public static bool operator ==(GroupReference left, GroupReference right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(GroupReference left, GroupReference right)
		{
			return !Equals(left, right);
		}
	}

	[TestFixture]
	[ProgressOnderwijsUtils.Test.Continuous]
	public class GroupReferenceTest
	{
		[Test]
		public void Properties()
		{
			GroupReference sut = new GroupReference(1, "test");
			Assert.That(sut.GroupId, Is.EqualTo(1));
			Assert.That(sut.Name, Is.EqualTo("test"));
		}

		private IEnumerable<TestCaseData> EquatableData()
		{
			yield return new TestCaseData(default(GroupReference), default(GroupReference), true);
			yield return new TestCaseData(default(GroupReference), new GroupReference(1, "rhs"), false);
			yield return new TestCaseData(new GroupReference(1, "lhs"), default(GroupReference), false);
			yield return new TestCaseData(new GroupReference(1, "lhs"), new GroupReference(1, "rhs"), true);
			yield return new TestCaseData(new GroupReference(1, "lhs"), new GroupReference(2, "rhs"), false);
		}

		[Test, TestCaseSource("EquatableData")]
		public void Equatable(GroupReference lhs, GroupReference rhs, bool equals)
		{
			Assert.That(lhs == rhs, Is.EqualTo(equals));
			Assert.That(lhs != rhs, Is.EqualTo(!equals));
			if (lhs != null) Assert.That(lhs.Equals((object)rhs), Is.EqualTo(equals));
			if (rhs != null) Assert.That(rhs.Equals((object)lhs), Is.EqualTo(equals));
		}

		[Test]
		public void HashCode()
		{
			ISet<GroupReference> set = new HashSet<GroupReference>();
			Assert.That(set.Add(new GroupReference(1, "first")));
			Assert.That(set.Add(new GroupReference(2, "second")));
			Assert.That(!set.Add(new GroupReference(2, "third")));
		}
	}
}
