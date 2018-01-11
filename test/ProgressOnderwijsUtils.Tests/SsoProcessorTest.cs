using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
            var querySplit = rawUri.Query.Substring(1).Split(new[] { "&Signature=" }, StringSplitOptions.None);
            var signedData = Encoding.UTF8.GetBytes(querySplit[0]);
            var rsaKey = (RSA)certificate.PublicKey.Key;
            var signature = Convert.FromBase64String(Uri.UnescapeDataString(querySplit[1]));
            PAssert.That(() => rsaKey.VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        }
    }
}
