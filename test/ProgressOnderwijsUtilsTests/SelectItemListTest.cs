using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class SelectItemListTest : WebSessionTestSuiteBase
    {
        IReadOnlyList<SelectItem<int?>> sut;

        [SetUp]
        public void SetUp() { sut = Applicatie.KoppelTabel(conn, Session, "land"); }

        [Test]
        public void GetItemByValue() { AssertItem(sut.GetItem((int)Land.Nederland), (int)Land.Nederland); }

        [Test]
        public void GetItemByText() { AssertItem(sut.GetItem(Session.Language, "Nederland"), (int)Land.Nederland); }

        static void AssertItem(SelectItem<int?> item, int? expected) { Assert.That(item.Value, Is.EqualTo(expected)); }
    }
}
