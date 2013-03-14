using System.Collections.Generic;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	[TestFixture]
	[ProgressOnderwijsUtils.Test.Continuous]
	public class ReferenceEqualityComparerTest
	{
		struct TestType
		{
			int value;

			public TestType(int value) { this.value = value; }
		}

		static readonly TestType t1 = new TestType(1);
		static readonly TestType t2 = new TestType(1);

		[Test]
		public void TestValue()
		{
			var sut = new HashSet<TestType>();
			Assert.That(sut.Add(t1));
			Assert.That(!sut.Add(t2));
		}

		[Test]
		public void TestReference()
		{
			var sut = new HashSet<TestType>(new ReferenceEqualityComparer<TestType>());
			Assert.That(sut.Add(t1));
			Assert.That(sut.Add(t2));
		}
	}
}
