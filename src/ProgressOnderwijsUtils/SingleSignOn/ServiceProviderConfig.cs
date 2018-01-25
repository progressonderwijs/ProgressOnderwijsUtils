using System.Security.Cryptography.X509Certificates;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public struct ServiceProviderConfig
    {
        public string entity;
        public X509Certificate2 certificate;
    }
}
