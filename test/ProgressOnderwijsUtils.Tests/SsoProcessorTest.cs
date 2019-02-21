using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SingleSignOn;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class SsoProcessorTest
    {
        [Fact]
        public void GetRedirectUrl_signature_verifies()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var rawUri = SsoProcessor.GetRedirectUrl(new AuthnRequest("123", "http://example.com", new ServiceProviderConfig {
                certificate = certificate,
                entity = "http://example.com"
            }));
            var querySplit = rawUri.Query.Substring(1).Split(new[] { "&Signature=" }, StringSplitOptions.None);
            var signedData = Encoding.UTF8.GetBytes(querySplit[0]);
            var rsaKey = (RSA)certificate.PublicKey.Key;
            var signature = Convert.FromBase64String(Uri.UnescapeDataString(querySplit[1]));
            PAssert.That(() => rsaKey.VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        }

        [Fact]
        public void EncodeAndSignAsFormArgument_signature_verifies()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var base64EncodedRequest = new AuthnRequest("123", "http://example.com", new ServiceProviderConfig {
                certificate = certificate,
                entity = "http://example.com"
            }).EncodeAndSignAsFormArgument(certificate.GetRSAPrivateKey());
            var rawRequest = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedRequest));
            var doc = new XmlDocument();
            doc.LoadXml(rawRequest);
            var signedXml = new SignedXml(doc);
            signedXml.LoadXml(doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
            PAssert.That(() => signedXml.CheckSignature(certificate.GetRSAPublicKey()));
        }

        [Fact(Skip = "for manual use")]
        public void SsoProcessor_GetAttributes_doesnt_throw_for_valid_response()
        {
            var validRawSamlResponse = "...";
            var certificate = new X509Certificate2(Encoding.UTF8.GetBytes(@"..."));
            SsoProcessor.GetAttributes(validRawSamlResponse, certificate);
        }
    }
}
