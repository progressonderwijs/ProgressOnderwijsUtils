using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture] public class UtilsTest
	{
		[Test]
		public void IntentionallyBrokenTest()
		{
			throw new Exception("Eamon broke this to test jenkin's email functionality 2012-01-07 (will be disabled soon)");
		}
	}
}