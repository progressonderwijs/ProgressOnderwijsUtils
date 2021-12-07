using System.Security.Cryptography.X509Certificates;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class RsaStringSignerTest
    {
        [Fact]
        public void Signature_verifies()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var signed = RsaStringSigner.SignJson(certificate, "https://example.com");

            PAssert.That(() => RsaStringSigner.VerifySignedJson<string>(certificate, signed) == "https://example.com");
        }

        [Fact]
        public void Signature_can_fail()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var signed = RsaStringSigner.SignJson(certificate, "https://example.com").Replace("example", "evilexample");

            PAssert.That(() => RsaStringSigner.VerifySignedString(certificate, signed) == null);
        }
    }
}
