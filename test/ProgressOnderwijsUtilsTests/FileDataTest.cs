using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class FileDataTest
	{
		[Test]
		public void EqualityMakesSense()
		{
			FileData empty = default(FileData),
				basic = new FileData { Content = new byte[] { 1, 2, 3 }, ContentType = "ab", FileName = "xyz" },
				same = new FileData { Content = new byte[] { 1, 2, 3 }, ContentType = "aab".Substring(1), FileName = "Xxyz".Substring(1) },
				diffdata = new FileData { Content = new byte[] { 1, 2, 4 }, ContentType = "ab", FileName = "xyz" },
				difftype = new FileData { Content = new byte[] { 1, 2, 3 }, ContentType = "abc", FileName = "xyz" },
				diffname = new FileData { Content = new byte[] { 1, 2, 3 }, ContentType = "ab", FileName = "xYz" }
				;

			PAssert.That(() => same == basic && basic == same);
			PAssert.That(() => same.GetHashCode() == basic.GetHashCode());

			PAssert.That(() => empty != basic && basic != empty);
			PAssert.That(() => empty.GetHashCode() != basic.GetHashCode());

			PAssert.That(() => diffdata != basic && diffdata != empty);
			PAssert.That(() => diffdata.GetHashCode() != basic.GetHashCode());

			PAssert.That(() => difftype != basic && difftype != empty);
			PAssert.That(() => difftype.GetHashCode() != basic.GetHashCode());
	
			PAssert.That(() => diffname != basic && diffname != empty);
			PAssert.That(() => diffname.GetHashCode() != basic.GetHashCode());//TODO: case-sensitivity?
		}
	}
}
