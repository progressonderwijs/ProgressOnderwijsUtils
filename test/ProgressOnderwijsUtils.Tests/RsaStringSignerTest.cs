using System.Security.Cryptography.X509Certificates;

namespace ProgressOnderwijsUtils.Tests;

public sealed class RsaStringSignerTest
{
    [Fact]
    public void Signature_verifies()
    {
        var certificate = X509CertificateLoader.LoadPkcs12FromFile("testCert.pfx", "testPassword");
        var signed = RsaStringSigner.SignJson(certificate, "https://example.com");

        PAssert.That(() => RsaStringSigner.VerifySignedJson<string>(certificate, signed) == "https://example.com");
    }

    [Fact]
    public void Signature_can_fail()
    {
        var certificate = X509CertificateLoader.LoadPkcs12FromFile("testCert.pfx", "testPassword");
        var signed = RsaStringSigner.SignJson(certificate, "https://example.com").Replace("example", "evilexample");

        PAssert.That(() => RsaStringSigner.VerifySignedString(certificate, signed) == null);
    }
}
