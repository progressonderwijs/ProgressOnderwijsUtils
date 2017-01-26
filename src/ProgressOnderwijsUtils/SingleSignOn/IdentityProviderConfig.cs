using System.Security.Cryptography.X509Certificates;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public struct IdentityProviderConfig
    {
        public string metadata;
        public string identity;
        public X509Certificate2 certificate;
    }
}