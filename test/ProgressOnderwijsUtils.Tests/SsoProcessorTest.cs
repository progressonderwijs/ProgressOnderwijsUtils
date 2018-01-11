using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using ExpressionToCodeLib;
using ProgressOnderwijsUtils.SingleSignOn;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public class SsoProcessorTest
    {
        [Fact]
        public void GetRedirectUrl_signature_verifies()
        {
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");
            var rawUri = SsoProcessor.GetRedirectUrl(new AuthnRequest("123", "http://example.com", new ServiceProviderConfig {
                certificate = certificate,
                entity = "http://example.com"
            }));
            var query = HttpUtility.ParseQueryString(rawUri.Query);
            var signedData = Encoding.UTF8.GetBytes($"SAMLRequest={Uri.EscapeDataString(query["SAMLRequest"])}&SigAlg={Uri.EscapeDataString("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")}");
            var signature = Convert.FromBase64String(query["Signature"]);

            var signatureVerified = ((RSA)certificate.PublicKey.Key).VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            PAssert.That(() => signatureVerified);
        }
    }
}
