using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtils
{
	public static class Utils
	{
		[ExcludeFromNCover]
		public static string TestErrorStackOverflow(int rounds)
		{
			//This is intended for testing error-handling in case of dramatic errors.
			return TestErrorStackOverflow(rounds + 1);
		}

		[ExcludeFromNCover]
		public static void TestErrorOutOfMemory()
		{
			var memorySlurper = new List<byte[]>();
			for (long i = 0; i < Int64.MaxValue; i++) //no way any machine has near 2^70 bytes of RAM - a zettabyte! no way, ever. ;-)
				memorySlurper.Add(Encoding.UTF8.GetBytes(@"This is a simply string which is repeatedly put in memory to test the Out Of Memory condition.  It's encoded to make sure the program really touches the data and that therefore the OS really needs to allocate the memory, and can't just 'pretend'."));
		}

		[ExcludeFromNCover]
		public static void TestErrorNormalException() { throw new ApplicationException("This is a test exception intended to test fault-tolerance.  User's shouldn't see it, of course!"); }

		public static bool ElfProef(int getal)
		{
			int res = 0;
			for (int i = 1; getal != 0; getal /= 10, ++i)
				res += i * (getal % 10);
			return res != 0 && res % 11 == 0;
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

		public static T Retry<T>(Func<T> func, Func<Exception, bool> shouldRetryOnThisFailure)
		{
			const int retryMAX = 5;

			int attempt = 0;
			while (true)
			{
				try
				{
					return func();
				}
				catch (Exception e)
				{
					if (attempt++ >= retryMAX || !shouldRetryOnThisFailure(e))
						throw;
				}
			}
		}

		public static T Retry<T>(CancellationToken cancel, Func<T> func, Func<Exception, bool> shouldRetryOnThisFailure)
		{
			return Retry(
				() =>
				{
					cancel.ThrowIfCancellationRequested();
					return func();
				},
				e => shouldRetryOnThisFailure(e) && !cancel.IsCancellationRequested
				);
		}

		public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<T, IEnumerable<T>> edgeLookup) { return TransitiveClosure(elems, nodes => nodes.SelectMany(edgeLookup)); }

		public static HashSet<T> TransitiveClosure<T>(IEnumerable<T> elems, Func<IEnumerable<T>, IEnumerable<T>> multiEdgeLookup)
		{
			var set = elems.ToSet();
			var distinctNewlyReachable = set.AsEnumerable();
			while (distinctNewlyReachable.Any())
				distinctNewlyReachable = multiEdgeLookup(distinctNewlyReachable).Where(set.Add).ToArray();
			return set;
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

		public static bool IsDbConnectionFailure(Exception e)
		{
			SqlException sqlE = e as SqlException ?? e.InnerException as SqlException;

			return (sqlE != null &&
			        (sqlE.Message.StartsWith("A transport-level error has occurred when receiving results from the server.") ||
			         sqlE.Message.StartsWith("A transport-level error has occurred when sending the request to the server.") ||
			         sqlE.Message.StartsWith("Timeout expired."))) ||
			       (e is EntityException && e.Message == "The underlying provider failed on Open.");
		}

		public static string GetSqlExceptionDetailsString(Exception exception)
		{
			SqlException sql = exception as SqlException ?? exception.InnerException as SqlException;
			return sql == null ? null : String.Format("[code='{0:x}'; number='{1}'; state='{2}']", sql.ErrorCode, sql.Number, sql.State);
		}

		public static bool NUnitSession()
		{
			string procname = Process.GetCurrentProcess().ProcessName;
			return procname.StartsWith("nunit") || procname.StartsWith("pnunit"); //also supports nunit-agent, nunit-console, nunit-x86, etc.
		}

		/// <summary>
		/// Geeft het verschil in maanden tussen twee datums
		/// </summary>
		/// <param name="d1"></param>
		/// <param name="d2"></param>
		/// <returns></returns>
		public static int MaandSpan(DateTime d1, DateTime d2) { return Math.Abs(d1 > d2 ? (12 * (d1.Year - d2.Year) + d1.Month) - d2.Month : (12 * (d2.Year - d1.Year) + d2.Month) - d1.Month); }

		public static DateTime? DateMax(DateTime? d1, DateTime? d2)
		{
			if (d1 == null)
				return d2;

			if (d2 == null)
				return d1;

			return d2 > d1 ? d2 : d1;
		}

		public static bool GenerateForLanguage(DocumentLanguage doc, Taal language)
		{
			bool result;
			switch (doc)
			{
			case DocumentLanguage.Dutch:
				result = language == Taal.NL;
				break;
			case DocumentLanguage.English:
				result = language == Taal.EN;
				break;
			case DocumentLanguage.German:
				result = language == Taal.DU;
				break;
			case DocumentLanguage.StudentPreferenceNlEn:
			case DocumentLanguage.CoursePreferenceNlEn:
			case DocumentLanguage.ProgramPreferenceNlEn:
				result = language == Taal.NL || language == Taal.EN;
				break;
			case DocumentLanguage.StudentPreferenceNlEnDu:
				result = language == Taal.NL || language == Taal.EN || language == Taal.DU;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
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

		enum SampleEnum
		{
			ValueA,
			ValueB
		};

		IEnumerable<TestCaseData> InClauseData()
		{
			yield return new TestCaseData(new int[] { }).Returns("(null)");
			yield return new TestCaseData(new[] { 0 }).Returns("(0)");
			yield return new TestCaseData(new[] { 0, 1 }).Returns("(0, 1)");
			yield return new TestCaseData(new[] { SampleEnum.ValueA, SampleEnum.ValueB }).Returns("(0, 1)");
		}

		IEnumerable<TestCaseData> InClauseStringData() { yield return new TestCaseData(new[] { "test", "ab'c", "xyz" }.ToList()).Returns("('test', 'ab''c', 'xyz')"); }

		[Test, TestCaseSource("InClauseData")]
		public string InClause(IEnumerable<int> values) { return Utils.SqlInClause(values); }

		[Test, TestCaseSource("InClauseStringData")]
		public string InClauseStrings(IEnumerable<string> values) { return Utils.SqlInClause(values); }

		[Test]
		public void NUnitSession() { Assert.That(Utils.NUnitSession()); }

		IEnumerable<TestCaseData> MaandSpan()
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
		public int MaandSpanTest(DateTime d1, DateTime d2) { return Utils.MaandSpan(d1, d2); }

		IEnumerable<TestCaseData> DateMax()
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

		[Test]
		public void DateMaxTest()
		{
			DateTime? d1 = null;
			DateTime? d2 = null;

			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(null));

			d1 = DateTime.Today;
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

			d1 = null;
			d2 = DateTime.Today;
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));

			d1 = DateTime.Today;
			d2 = DateTime.Today;
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

			d1 = DateTime.Today.AddDays(-1);
			d2 = DateTime.Today;
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));

			d1 = DateTime.Today.AddDays(1);
			d2 = DateTime.Today;
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

			d1 = DateTime.Today;
			d2 = DateTime.Today.AddDays(-1);
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d1));

			d1 = DateTime.Today;
			d2 = DateTime.Today.AddDays(1);
			Assert.That(Utils.DateMax(d1, d2), Is.EqualTo(d2));
		}

		[Test, TestCase(DocumentLanguage.Dutch, Taal.NL, Result = true), TestCase(DocumentLanguage.Dutch, Taal.EN, Result = false), TestCase(DocumentLanguage.Dutch, Taal.DU, Result = false), TestCase(DocumentLanguage.English, Taal.NL, Result = false), TestCase(DocumentLanguage.English, Taal.EN, Result = true), TestCase(DocumentLanguage.English, Taal.DU, Result = false), TestCase(DocumentLanguage.German, Taal.NL, Result = false), TestCase(DocumentLanguage.German, Taal.EN, Result = false), TestCase(DocumentLanguage.German, Taal.DU, Result = true), TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.NL, Result = true), TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.EN, Result = true), TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.DU, Result = false), TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.NL, Result = true), TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.EN, Result = true), TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.DU, Result = true), TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.NL, Result = true), TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.EN, Result = true), TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.DU, Result = false)]
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		public bool GenerateForLanguage(DocumentLanguage doc, Taal language) { return Utils.GenerateForLanguage(doc, language); }
	}

	[TestFixture]
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