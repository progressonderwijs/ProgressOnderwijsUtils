using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class SortedListExtensions
	{
		[Test]
		public void SimpleTests()
		{
			var list1 = new SortedList { { "abc", 1 }, { "def", 2 }, { "xyz", 3 }, };
			var list2 = new SortedList { { "def", 2 }, { "xyz", 3 }, { "abc", 1 }, };

			var list3 = new SortedList { { "def", 2 }, { "xyz", 4 }, { "abc", 1 }, };
			var list4 = new SortedList { { "def", 2 }, { "xyz ", 3 }, { "abc", 1 }, };

			var list5 = new SortedList { { "def", 2 }, { "xyz", 3 }, };

			PAssert.That(() => list1.EqualsKeyValue(list2) && list1 != list2);
			PAssert.That(() => !list1.EqualsKeyValue(null));
			PAssert.That(() => !list1.EqualsKeyValue(list3));
			PAssert.That(() => !list1.EqualsKeyValue(list4));
			PAssert.That(() => !list1.EqualsKeyValue(list5));
		}
	}
}
