#nullable disable
using System.Xml.Linq;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public static class SamlNamespaces
    {
        public static readonly XNamespace SAML_NS = "urn:oasis:names:tc:SAML:2.0:assertion";
        public static readonly XNamespace SAMLP_NS = "urn:oasis:names:tc:SAML:2.0:protocol";
        public static readonly XNamespace SAMLMD_NS = "urn:oasis:names:tc:SAML:2.0:metadata";
    }
}
