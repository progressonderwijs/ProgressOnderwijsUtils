using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	[ProgressOnderwijsUtils.Test.Continuous]
	public sealed class SelectItemListTest : WebSessionTestSuiteBase
	{
		IReadOnlyList<SelectItem<int?>>  sut;

		[SetUp]
		public void SetUp()
		{
			sut = Applicatie.KoppelTabel(conn, "land", session.Organisatie.OrganisatieId, session.Language, session.Account.Id);
		}

		[Test]
		public void GetItemByValue()
		{
			AssertItem(sut.GetItem((int)Land.Nederland), (int)Land.Nederland);
		}

		[Test]
		public void GetItemByText()
		{
			AssertItem(sut.GetItem(session.Language, "Nederland"), (int)Land.Nederland);
		}

		void AssertItem(SelectItem<int?> item, int? expected)
		{
			Assert.That(item.Value, Is.EqualTo(expected));
		}
	}
}
