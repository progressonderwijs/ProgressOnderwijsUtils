using ExpressionToCodeLib;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public class Md5HelpersTest
    {
        [Test]
        public void BasicChecks()
        {
            const string strA = @" ASDF#VA#Q$T*B#$%(DFB	<script>";
            PAssert.That(
                () =>
                    Md5Helpers.MD5ComputeHash(strA) != strA && Md5Helpers.MD5VerifyHash(strA, Md5Helpers.MD5ComputeHash(strA))
                        && !Md5Helpers.MD5VerifyHash(strA.Substring(1), Md5Helpers.MD5ComputeHash(strA)));
        }
    }
}
