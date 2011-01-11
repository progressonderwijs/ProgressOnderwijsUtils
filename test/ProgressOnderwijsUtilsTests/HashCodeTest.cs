using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class HashCodeTest
	{
		[Test]
		public void IsConsistent()
		{
			PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue,null)); //extra null matters
			PAssert.That(() => HashCodeHelper.ComputeHash("test", 3, null, DateTime.MinValue) != HashCodeHelper.ComputeHash("test", 3, DateTime.MinValue, null)); //order matters
		}
	}
}
