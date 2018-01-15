using System;
using System.Security.Cryptography.X509Certificates;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class SignerTest
    {
        [Fact]
        public void Signature_verifies()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var signed = Signer.SignJson(certificate, "https://example.com");

            PAssert.That(() => Signer.VerifySignedJson<string>(certificate, signed) == "https://example.com");
        }

        [Fact]
        public void Signature_can_fail()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var signed = Signer.SignJson(certificate, "https://example.com").Replace("example", "evilexample");

            PAssert.That(() => Signer.VerifySignedString(certificate, signed) == null);
        }
    }
}
