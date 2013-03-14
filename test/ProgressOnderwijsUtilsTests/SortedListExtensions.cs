using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	[ProgressOnderwijsUtils.Test.Continuous]
	public class SortedListExtensions
	{
		[Test]
		public void SimpleTests()
		{
			var list1 = new RowKey { { "abc", 1 }, { "def", 2 }, { "xyz", 3 }, };
			var list2 = new RowKey { { "def", 2 }, { "xyz", 3 }, { "abc", 1 }, };

			var list3 = new RowKey { { "def", 2 }, { "xyz", 4 }, { "abc", 1 }, };
			var list4 = new RowKey { { "def", 2 }, { "xyz ", 3 }, { "abc", 1 }, };

			var list5 = new RowKey { { "def", 2 }, { "xyz", 3 }, };

			PAssert.That(() => list1.EqualsKeyValue(list2) && list1 != list2);
			PAssert.That(() => !list1.EqualsKeyValue(null));
			PAssert.That(() => !list1.EqualsKeyValue(list3));
			PAssert.That(() => !list1.EqualsKeyValue(list4));
			PAssert.That(() => !list1.EqualsKeyValue(list5));
		}
	}
}
