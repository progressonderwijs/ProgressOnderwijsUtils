using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
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
