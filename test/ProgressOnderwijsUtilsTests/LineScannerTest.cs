using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
	[Continuous]
	public class LineScannerTest
	{

		[Test]
		public void ReadAndPushbackTest()
		{
			var ls = new LineScanner("Hello\r\nWorld!\n");
			string line;

			PAssert.That(() => !ls.Eof());
			line = ls.GetLine();
			PAssert.That(() => line == "Hello");

			PAssert.That(() => !ls.Eof());
			line = ls.GetLine();
			PAssert.That(() => line == "World!");

			PAssert.That(() => !ls.Eof());
			line = ls.GetLine();
			PAssert.That(() => line == "");

			PAssert.That(() => ls.Eof());
			line = ls.GetLine();
			PAssert.That(() => line == null);

			PAssert.That(() => ls.Eof());
			ls.PushBack();
			PAssert.That(() => !ls.Eof());
			line = ls.GetLine();
			PAssert.That(() => ls.Eof());
			PAssert.That(() => line == "");
			
			line = ls.GetLine();
			PAssert.That(() => ls.Eof());
			PAssert.That(() => line == null);
		}
	}
}
