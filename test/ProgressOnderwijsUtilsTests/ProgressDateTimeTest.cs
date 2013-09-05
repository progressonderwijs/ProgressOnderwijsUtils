using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	public sealed class ProgressDateTimeTest
	{

		[TearDown]
		public void TearDown()
		{
			ProgressDateTime.Reset();
		}

		[Test]
		public void DateShiftWithDays([Values(-100, -12, 0, 17, 537)] int daysToAdd)
		{
			ProgressDateTime.DaysToAdd = daysToAdd;
			Assert.That(ProgressDateTime.Now.Subtract(DateTime.Now).Days, Is.EqualTo(daysToAdd));
		}

		[Test]
		public void DateShiftWithDate([Values(-100, -12, 0, 17, 537)] int daysToAdd)
		{
			ProgressDateTime.SetDate(DateTime.Now.AddDays(daysToAdd));
			Assert.That(ProgressDateTime.Now.Subtract(DateTime.Now).Days, Is.EqualTo(daysToAdd));
		}

	}
}
