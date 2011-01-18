using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Linq;

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
			throw new ApplicationException("This is a test exception intended to test fault-tolerance.  User's shouldn't see it, of course!");
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
		public static void Swap<T>(ref T one, ref T other)
		{
			T tmp = one;
			one = other;
			other = tmp;
		}

		/// <summary>
		/// Joins a set of values into SQL syntax; e.g. test, ab'c, xyz turn into "('test', 'ab''c', 'xyz')" and the empty set turns into "(null)".
		/// Single quotes are doubled; however, this is not rigorously safe and as such beware of SQL-injection.
		/// No user-supplied strings should be used with this function!
		/// </summary>
		public static string SqlInClause(IEnumerable<string> values) { return SqlInClauseHelper(values.Select(EscapeSqlString)); }

		/// <summary>
		/// Joins a set of values into SQL syntax; e.g. 1,2,3 turn into "(1,2,3)" and the empty set turns into "(null)"
		/// </summary>
		public static string SqlInClause(IEnumerable<int> values) { return SqlInClauseHelper(values.Select(val => val.ToStringInvariant())); }

		static string EscapeSqlString(string val) { return '\'' + val.Replace("'", "''") + '\''; }
		static string SqlInClauseHelper(IEnumerable<string> values)
		{
			string joined = values.JoinStrings(", ");
			return joined.Length == 0 ? "(null)" : "(" + joined + ")";
		}


		public static bool NUnitSession()
		{
			string procname = Process.GetCurrentProcess().ProcessName;
			return procname.StartsWith("nunit") || procname.StartsWith("pnunit");//also supports nunit-agent, nunit-console, nunit-x86, etc.
		}

		/// <summary>
		/// Geeft het verschil in maanden tussen twee datums
		/// </summary>
		/// <param name="d1"></param>
		/// <param name="d2"></param>
		/// <returns></returns>
		public  static int MaandSpan(DateTime d1, DateTime d2)
		{
			return Math.Abs(d1 > d2 ? (12 * (d1.Year - d2.Year) + d1.Month) - d2.Month : (12 * (d2.Year - d1.Year) + d2.Month) - d1.Month);
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

		enum SampleEnum { ValueA, ValueB };

		private IEnumerable<TestCaseData> InClauseData()
		{
			yield return new TestCaseData(new int[] { }).Returns("(null)");
			yield return new TestCaseData(new[] { 0 }).Returns("(0)");
			yield return new TestCaseData(new[] { 0, 1 }).Returns("(0, 1)");
			yield return new TestCaseData(new[] { SampleEnum.ValueA, SampleEnum.ValueB }).Returns("(0, 1)");
		}

		private IEnumerable<TestCaseData> InClauseStringData()
		{
			yield return new TestCaseData( new[] { "test", "ab'c", "xyz" }.ToList()).Returns("('test', 'ab''c', 'xyz')");
		}

		[Test, TestCaseSource("InClauseData")]
		public string InClause(IEnumerable<int> values)
		{
			return Utils.SqlInClause(values);
		}

		[Test, TestCaseSource("InClauseStringData")]
		public string InClauseStrings(IEnumerable<string> values)
		{
			return Utils.SqlInClause(values);
		}

		[Test]
		public void NUnitSession()
		{
			Assert.That(Utils.NUnitSession());
		}

		private IEnumerable<TestCaseData> MaandSpan()
		{
			yield return new TestCaseData(new DateTime(2000, 1, 1), new DateTime(2000, 1, 1)).Returns(0);
			yield return new TestCaseData(new DateTime(2000, 5, 1), new DateTime(2000, 1, 1)).Returns(4);
			yield return new TestCaseData(new DateTime(2000, 1, 1), new DateTime(2001, 1, 1)).Returns(12);
			yield return new TestCaseData(new DateTime(2001, 1, 1), new DateTime(2000, 1, 1)).Returns(12);
			yield return new TestCaseData(new DateTime(2000, 9, 1), new DateTime(2001, 2, 1)).Returns(5);
			yield return new TestCaseData(new DateTime(2000, 9, 1), new DateTime(2001, 4, 1)).Returns(7);
			yield return new TestCaseData(new DateTime(2001, 6, 1), new DateTime(2000, 9, 1)).Returns(9);
			yield return new TestCaseData(new DateTime(2000, 12, 1), new DateTime(2001, 1, 1)).Returns(1);
		}
		[Test, TestCaseSource("MaandSpan")]
		public int MaandSpanTest(DateTime d1, DateTime d2)
		{
			return Utils.MaandSpan(d1, d2);
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