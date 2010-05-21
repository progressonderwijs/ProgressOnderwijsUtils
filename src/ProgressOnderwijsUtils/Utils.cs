using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ProgressOnderwijsUtils;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using System.Reflection;

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
		public static bool IsNull(object anyObject) { return anyObject == null || anyObject == DBNull.Value ? true : false; }

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
			for (long i = 0; i < long.MaxValue;i++ ) //no way any machine has near 2^70 bytes of RAM - a zettabyte! no way, ever. ;-)
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
		/**
		 * <summary>
		 *  Utility om van een serie strings een serie
		 *  Tuples van strings te maken (paren, om precies
		 *  te zijn). 
		 *  Te gebruiken bij Tools.StringExtensions.MultiReplace,
		 *  om de tweede parameter wat gemakkelijker samen te stellen
		 *  </summary>
		 *  <param name="p">[stringa,stringb,stringc,stringd, ... , stringxyz]</param>
		 *  <example>
		 *  Om twee paren van Regex-string en vervangstring te maken bv:
		 * 
		 *  Tuple&lt;string,string&gt;[] tupz = 
		 * 				Tools.Utils.ToTuples(@"\r\n" ," \n" ,
		 * 									 @"\email","e-mail");
		 *  of direct in de MultiReplace-extension van 'n string:
		 * 	 [string].MultiReplace(
		 * 						RegexOptions.Multiline | RegexOptions.IgnoreCase,
		 * 						==&gt; Tools.Utils.ToTuples(@"\r\n", "\n") &lt;==
		 * 					  );
		 *  
		 *  </example>
		 *  <returns>Array van Tuples van twee strings</returns>
		 *  <remarks>
		 *  Bij oneven aantal parameter strings wordt de laatste parameter
		 *  niet gebruikt (de facto verwijderd uit de parameter array).
		 *  </remarks>
		 *  <codefrom value="Renzo Kooi" date="2009/08/18"/>
		 */
		public static Tuple<string, string>[] ToTuples(params string[] p)
		{
			int i = 0, plen = p.Length;
			plen = (plen % 2 != 0) ? plen-- : plen;
			Tuple<string, string>[] tupz = new Tuple<string, string>[p.Length / 2];
			do
			{
				tupz[i / 2] = Tuple.Create<string, string>(p[i], p[i + 1]);
			} while ((i += 2) < plen);
			return tupz;
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

		
		//idee van http://www.b-virtual.be/post/Generic-Cloning-Method-in-c.aspx
		//nog niet nuttig voor het clonen van genericcollection, omdat daarvoor geen (de)serialize is geimplementeerd
		/// <summary>
		/// Deep-clone's a serializable object by serializing it and deserializing the result.  This method won't be very fast; don't use it in an tight loop.
		/// </summary>
		public static T Clone<T>(T objectToClone)
		{
			using(var memoryStream = new MemoryStream())
			{
				var serializer	= new BinaryFormatter();
				serializer.Serialize(memoryStream, objectToClone);
				memoryStream.Position = 0;
				return (T)serializer.Deserialize(memoryStream);
			}
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
			return object.ReferenceEquals(one, other);
		}

		public int GetHashCode(T obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
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