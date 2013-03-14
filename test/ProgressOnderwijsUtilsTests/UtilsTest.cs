using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ExpressionToCodeLib;
using MoreLinq;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	public sealed class UtilsTest
	{
		[Test]
		public void ToSortableStringTest()
		{
			var cmp = StringComparer.Ordinal;
			var samplePoints = MoreEnumerable
				.Generate((double)long.MinValue, sample => sample + (1.0 + Math.Abs(sample) / 1000.0))
				.TakeWhile(sample => sample < long.MaxValue)
				.Select(d => (long)d)
				.Concat(new[] { long.MinValue, long.MaxValue - 1, -1, 0, 1 });

			foreach (var i in samplePoints)
			{
				var j = i + 1;
				string a = Utils.ToSortableShortString(i);
				string b = Utils.ToSortableShortString(j);
				if (cmp.Compare(a, b) >= 0)
					throw new Exception("numbers " + i + " and " + j + " produce out-of-order strings: " + a + " and " + b);
			}
		}

		[Test]
		[Continuous]
		public void SwapValue()
		{
			int one = 1;
			int other = 2;
			Utils.Swap(ref one, ref other);
			Assert.That(one, Is.EqualTo(2));
			Assert.That(other, Is.EqualTo(1));
		}

		[Test]
		[Continuous]
		public void SwapReference()
		{
			string one = "1";
			string other = "2";
			Utils.Swap(ref one, ref other);
			Assert.That(one, Is.EqualTo("2"));
			Assert.That(other, Is.EqualTo("1"));
		}

		[Test]
		[Continuous]
		public void InClause()
		{
			PAssert.That(() => Utils.SqlInClause(new int[] { }) == "(null)");
			PAssert.That(() => Utils.SqlInClause(new[] { 0 }) == "(0)");
			PAssert.That(() => Utils.SqlInClause(new[] { 0, 1 }) == "(0, 1)");
			PAssert.That(() => Utils.SqlInClause(new[] { "test", "ab'c", "xyz" }) == "('test', 'ab''c', 'xyz')");

		}

		[Test]
		[Continuous]
		public void NUnitSession() { Assert.That(Utils.IsInTestSession()); }

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
		[Continuous]
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
		[Continuous]
		public void IsDbConnFailureTest()
		{
			PAssert.That(() => !Utils.IsDbConnectionFailure(new Exception()));
			PAssert.That(() => !Utils.IsDbConnectionFailure(new EntityException()));
			PAssert.That(() => !Utils.IsDbConnectionFailure(new QueryException()));
			PAssert.That(() => Utils.IsDbConnectionFailure(new QueryException("bla", new EntityException("The underlying provider failed on Open."))));
			PAssert.That(() => Utils.IsDbConnectionFailure(new AggregateException(new QueryException("bla", new EntityException("The underlying provider failed on Open.")), new EntityException("The underlying provider failed on Open."))));
			PAssert.That(() => !Utils.IsDbConnectionFailure(new AggregateException()));
			PAssert.That(() => !Utils.IsDbConnectionFailure(null));
		}

		[Test]
		[Continuous]
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

		[Test,
		 TestCase(DocumentLanguage.Dutch, Taal.NL, Result = true),
		 TestCase(DocumentLanguage.Dutch, Taal.EN, Result = false),
		 TestCase(DocumentLanguage.Dutch, Taal.DU, Result = false),
		 TestCase(DocumentLanguage.English, Taal.NL, Result = false),
		 TestCase(DocumentLanguage.English, Taal.EN, Result = true),
		 TestCase(DocumentLanguage.English, Taal.DU, Result = false),
		 TestCase(DocumentLanguage.German, Taal.NL, Result = false),
		 TestCase(DocumentLanguage.German, Taal.EN, Result = false),
		 TestCase(DocumentLanguage.German, Taal.DU, Result = true),
		 TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.NL, Result = true),
		 TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.EN, Result = true),
		 TestCase(DocumentLanguage.StudentPreferenceNlEn, Taal.DU, Result = false),
		 TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.NL, Result = true),
		 TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.EN, Result = true),
		 TestCase(DocumentLanguage.StudentPreferenceNlEnDu, Taal.DU, Result = true),
		 TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.NL, Result = true),
		 TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.EN, Result = true),
		 TestCase(DocumentLanguage.CoursePreferenceNlEn, Taal.DU, Result = false)]
		[Continuous]
		public bool GenerateForLanguage(DocumentLanguage doc, Taal language) { return Utils.GenerateForLanguage(doc, language); }
	}
}
