using System.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.WebSupport;

namespace ProgressOnderwijsUtilsTests
{
    [PullRequestTest]
    public class CompressedUtf8StringTest
    {
        [Test]
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
