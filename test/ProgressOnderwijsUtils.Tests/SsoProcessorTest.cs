using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using ProgressOnderwijsUtils.SingleSignOn;

namespace ProgressOnderwijsUtils.Tests;

public sealed class SsoProcessorTest
{
    [Fact]
    public void GetRedirectUrl_signature_verifies()
    {
        var certificate = X509CertificateLoader.LoadPkcs12FromFile("testCert.pfx", "testPassword");
        var rawUri = SsoProcessor.GetRedirectUrl(
            new(
                "123",
                "http://example.com",
                new() {
                    certificate = certificate,
                    entity = "http://example.com",
                }
            )
        );
        var querySplit = rawUri.Query[1..].Split(new[] { "&Signature=", }, StringSplitOptions.None);
        var signedData = Encoding.UTF8.GetBytes(querySplit[0]);
        var rsaKey = certificate.GetRSAPublicKey().AssertNotNull();
        var signature = Convert.FromBase64String(Uri.UnescapeDataString(querySplit[1]));
        PAssert.That(() => rsaKey.VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void EncodeAndSignAsFormArgument_signature_verifies()
    {
        var certificate = X509CertificateLoader.LoadPkcs12FromFile("testCert.pfx", "testPassword");
        var base64EncodedRequest = new AuthnRequest(
            "123",
            "http://example.com",
            new() {
                certificate = certificate,
                entity = "http://example.com",
            }
        ).EncodeAndSignAsFormArgument(certificate.GetRSAPrivateKey().AssertNotNull());
        var rawRequest = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedRequest));
        var doc = new XmlDocument();
        doc.LoadXml(rawRequest);
        var signedXml = new SignedXml(doc);
        signedXml.LoadXml(doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
        PAssert.That(() => signedXml.CheckSignature(certificate.GetRSAPublicKey().AssertNotNull()));
    }

    [Fact(Skip = "for manual use")]
    public void SsoProcessor_GetAttributes_returns_ok_for_valid_response()
    {
        var validRawSamlResponse = "...";
        var certificate = X509CertificateLoader.LoadCertificate("..."u8);
        var attributes = SsoProcessor.GetAttributes(validRawSamlResponse, certificate);
        _ = attributes.AssertOk();
    }
}
