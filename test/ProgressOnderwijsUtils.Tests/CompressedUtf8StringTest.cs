using System.Linq;
using ExpressionToCodeLib;
using Xunit;
using ProgressOnderwijsUtils.WebSupport;

namespace ProgressOnderwijsUtils.Tests
{
    
    public sealed class CompressedUtf8StringTest
    {
        [Fact]
        public void IsReversible()
        {
            var sampledata = typeof(CompressedUtf8StringTest).Assembly.GetTypes().Select(t => t.FullName).JoinStrings("\n");
            PAssert.That(() => sampledata.Length > 1024);
            var zipped = new CompressedUtf8String(sampledata).GzippedUtf8String;
            PAssert.That(() => zipped.Length < sampledata.Length);
            PAssert.That(() => new CompressedUtf8String(zipped).StringData == sampledata);
        }
    }
}
