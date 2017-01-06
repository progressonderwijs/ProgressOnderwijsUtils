﻿using System;
using NUnit.Framework;
using Progress.Business.Test;

namespace ProgressOnderwijsUtilsTests
{
    public sealed class ProgressDateTimeTest : TestsWithBusinessConnection
    {
        [Test]
        public void DateShiftWithDays([Values(-100, -12, 0, 17, 537)] int daysToAdd)
        {
            conn.Cache.ProgressDateTime.TimeTravelToDate(DateTime.Now + TimeSpan.FromDays(daysToAdd));
            Assert.That(conn.Cache.ProgressDateTime.Now.Subtract(DateTime.Now).Days, Is.EqualTo(daysToAdd));
        }

        [Test]
        public void DateShiftWithDate([Values(-100, -12, 0, 17, 537)] int daysToAdd)
        {
            conn.Cache.ProgressDateTime.TimeTravelToDate(DateTime.Now.AddDays(daysToAdd));
            Assert.That(conn.Cache.ProgressDateTime.Now.Subtract(DateTime.Now).Days, Is.EqualTo(daysToAdd));
        }

        [Test]
        public void DateUnshiftedByDefault()
        {
            Assert.AreEqual(conn.Cache.ProgressDateTime.Now, DateTime.Now);
            Assert.AreEqual(conn.Cache.ProgressDateTime.Today, DateTime.Today);
        }
    }
}
