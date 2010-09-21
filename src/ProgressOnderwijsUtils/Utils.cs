using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;

namespace ProgressOnderwijsUtils
{
	public static class Utils
	{
		/// <summary>
		/// Checks if any object is null or DbNull
		/// </summary>
		/// <editinfo date="2010/05/20" editedby="RK"/>
		/// <param name="anyObject"></param>
		/// <returns></returns>
		public static bool IsNull(object anyObject) { return anyObject == null || anyObject == DBNull.Value; }

		/// <summary>
		/// Checks object is neither null nor DBNull
		/// </summary>
		/// <editinfo date="2010/08/03" editedby="AvdP"/>
		/// <param name="anyObject"></param>
		/// <returns></returns>
		public static bool IsNotNull(object anyObject) { return !IsNull(anyObject); }

		public static string UitgebreideFout(Exception e, string tekst)
		{
			return tekst + e.Message + "  " + e.TargetSite.Name + " " + e.StackTrace;
		}

		public static string TestErrorStackOverflow(int rounds)
		{
			//This is intended for testing error-handling in case of dramatic errors.
			return TestErrorStackOverflow(rounds + 1);
		}

		public static void TestErrorOutOfMemory()
		{
			List<byte[]> memorySlurper = new List<byte[]>();
			for (long i = 0; i < long.MaxValue; i++) //no way any machine has near 2^70 bytes of RAM - a zettabyte! no way, ever. ;-)
			{
				memorySlurper.Add(Encoding.UTF8.GetBytes(@"This is a simply string which is repeatedly put in memory to test the Out Of Memory condition.  It's encoded to make sure the program really touches the data and that therefore the OS really needs to allocate the memory, and can't just 'pretend'."));
			}
		}

		public static void TestErrorNormalException()
		{
			throw new ApplicationException("This is a test exception intended to test fault-tolerance.  " +
										   "User's shouldn't see it, of course!");
		}

		public static bool ElfProef(int getal)
		{
			int res = 0;
			for (int i = 1; getal != 0; getal /= 10, ++i)
				res += i * (getal % 10);
			return res != 0 && res % 11 == 0;
		}

		//idee van http://www.b-virtual.be/post/Generic-Cloning-Method-in-c.aspx
		//nog niet nuttig voor het clonen van genericcollection, omdat daarvoor geen (de)serialize is geimplementeerd
		/// <summary>
		/// Deep-clone's a serializable object by serializing it and deserializing the result.  This method won't be very fast; don't use it in an tight loop.
		/// </summary>
		public static T Clone<T>(T objectToClone)
		{
			using (var memoryStream = new MemoryStream())
			{
				var serializer = new BinaryFormatter();
				serializer.Serialize(memoryStream, objectToClone);
				memoryStream.Position = 0;
				return (T)serializer.Deserialize(memoryStream);
			}
		}

		/// <summary>
		/// Swap two objects.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="one"></param>
		/// <param name="other"></param>
		public static void Swap<T>(ref T one, ref T other)
		{
			T tmp = one;
			one = other;
			other = tmp;
		}

		public static string InClause(IEnumerable<int> values)
		{
			StringBuilder sb = new StringBuilder();
			foreach (int value in values)
			{
				if (sb.Length > 0)
				{
					sb.Append(", ");
				}
				sb.Append(value.ToStringInvariant());
			}
			if (sb.Length == 0)
			{
				sb.Append("null");
			}
			sb.Insert(0, "(");
			sb.Append(")");
			return sb.ToString();
		}
	}



	/// <summary>
	/// Equality comparer that will compare on reference equality.
	/// </summary>
	/// <remarks>This might be handy to have collections on reference equality while the elements are value comparable.</remarks>
	/// <typeparam name="T"></typeparam>
	public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
	{
		public bool Equals(T one, T other)
		{
			return ReferenceEquals(one, other);
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	public abstract class AnInnerClass<T>
	{
		protected T Outer { get; private set; }

		protected AnInnerClass(T outer)
		{
			Outer = outer;
		}
	}

	[TestFixture]
	public class UtilsTest
	{
		[Test]
		public void SwapValue()
		{
			int one = 1;
			int other = 2;
			Utils.Swap(ref one, ref other);
			Assert.That(one, Is.EqualTo(2));
			Assert.That(other, Is.EqualTo(1));
		}

		[Test]
		public void SwapReference()
		{
			string one = "1";
			string other = "2";
			Utils.Swap(ref one, ref other);
			Assert.That(one, Is.EqualTo("2"));
			Assert.That(other, Is.EqualTo("1"));
		}

		private IEnumerable<TestCaseData> InClauseData()
		{
			yield return new TestCaseData(new List<int>()).Returns("(null)");
			yield return new TestCaseData(new List<int> { 0 }).Returns("(0)");
			yield return new TestCaseData(new List<int> { 0, 1 }).Returns("(0, 1)");
		}

		[Test, TestCaseSource("InClauseData")]
		public string InClause(IEnumerable<int> values)
		{
			return Utils.InClause(values);
		}
	}

	[TestFixture]
	public class ReferenceEqualityComparerTest
	{
		private struct TestType
		{
			private int value;

			public TestType(int value)
			{
				this.value = value;
			}
		}

		private static readonly TestType t1 = new TestType(1);
		private static readonly TestType t2 = new TestType(1);

		[Test]
		public void TestValue()
		{
			HashSet<TestType> sut = new HashSet<TestType>();
			Assert.That(sut.Add(t1));
			Assert.That(!sut.Add(t2));
		}

		[Test]
		public void TestReference()
		{
			HashSet<TestType> sut = new HashSet<TestType>(new ReferenceEqualityComparer<TestType>());
			Assert.That(sut.Add(t1));
			Assert.That(sut.Add(t2));
		}
	}
}