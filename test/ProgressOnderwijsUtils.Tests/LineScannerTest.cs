using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class LineScannerTest
    {
        [Fact]
        public void ReadAndPushbackTest()
        {
            var ls = new LineScanner("Hello\r\nWorld!\n");

            PAssert.That(() => !ls.Eof());
            var line = ls.GetLine();
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
