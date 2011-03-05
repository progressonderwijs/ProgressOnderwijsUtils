using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Test.CodeStyle;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.WebSupport;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class CompressedUtf8StringTest
	{
		[Test]
		public void IsReversible()
		{
			var sampledata=KoppelvlakSanityTest.KoppelvlakTypes.Select(t => t.FullName).JoinStrings("\n");
			PAssert.That(() => sampledata.Length > 1024);
			var zipped = new CompressedUtf8String(sampledata).GzippedUtf8String;
			PAssert.That(() => zipped.Length < sampledata.Length);
			PAssert.That(() => new CompressedUtf8String(zipped).StringData == sampledata);
		}
	}
}
