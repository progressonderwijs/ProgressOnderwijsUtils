using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public struct AuthnRequest
    {
        public string ID { get; }
        public string Destination { get; }
        public ServiceProviderConfig Issuer { get; }
        public bool ForceAuthn { get; set; }
        public string AuthnContextClassRef { get; set; }

        public AuthnRequest(string id, string destination, ServiceProviderConfig issuer)
        {
            ID = id;
            Destination = destination;
            Issuer = issuer;
            ForceAuthn = false;
            AuthnContextClassRef = null;
        }

        [NotNull]
        public string EncodeAsQueryArgument()
        {
            var xml = Encoding.UTF8.GetBytes(ToXml().ToString());
            using (var stream = new MemoryStream()) {
                using (var deflate = new DeflateStream(stream, CompressionMode.Compress)) {
                    deflate.Write(xml, 0, xml.Length);
                }
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        [NotNull]
        public string EncodeAndSignAsFormArgument([NotNull] RSA key)
        {
            var doc = new XmlDocument { PreserveWhitespace = false };
            doc.Load(ToXml().CreateReader());
            var signedXml = new SignedXml(doc) { SigningKey = key };
            var reference = new Reference { Uri = "" };
            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            signedXml.AddReference(reference);
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue(key));
            signedXml.KeyInfo = keyInfo;
            signedXml.ComputeSignature();
            var xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(doc.InnerXml));
        }

        [NotNull]
        XElement ToXml()
        {
            return new XElement(
                SamlNamespaces.SAMLP_NS + "AuthnRequest",
                new XAttribute(XNamespace.Xmlns + "saml", SamlNamespaces.SAML_NS.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "sampl", SamlNamespaces.SAMLP_NS.NamespaceName),
                new XAttribute("ID", XmlConvert.EncodeLocalName(ID)),
                new XAttribute("Version", "2.0"),
                new XAttribute("IssueInstant", DateTime.UtcNow),
                new XAttribute("Destination", Destination),
                new XAttribute("ForceAuthn", ForceAuthn),
                new XAttribute("IsPassive", "false"),
                new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"),
                new XElement(SamlNamespaces.SAML_NS + "Issuer", Issuer.entity),
                AuthnContextClassRef == null
                    ? null
                    : new XElement(
                        SamlNamespaces.SAMLP_NS + "RequestedAuthnContext",
                        new XElement(SamlNamespaces.SAML_NS + "AuthnContextClassRef", AuthnContextClassRef)
                    )
            );
        }
    }
}
