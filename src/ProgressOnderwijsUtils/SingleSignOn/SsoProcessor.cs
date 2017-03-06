using System;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using log4net;
using ProgressOnderwijsUtils.Log4Net;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public static class SsoProcessor
    {
        struct AuthnRequest
        {
            public string Destination { get; set; }
            public string Issuer { private get; set; }

            public string Encode()
            {
                var xml = Encoding.UTF8.GetBytes(ToXml().ToString());
                using (var stream = new MemoryStream()) {
                    using (var deflate = new DeflateStream(stream, CompressionMode.Compress))
                        deflate.Write(xml, 0, xml.Length);
                    return Convert.ToBase64String(stream.ToArray());
                }
            }

            XElement ToXml()
            {
                return new XElement(
                    SamlNamespaces.SAMLP_NS + "AuthnRequest",
                    new XAttribute(XNamespace.Xmlns + "saml", SamlNamespaces.SAML_NS.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "sampl", SamlNamespaces.SAMLP_NS.NamespaceName),
                    new XAttribute("ID", "_" + Guid.NewGuid()),
                    new XAttribute("Version", "2.0"),
                    new XAttribute("IssueInstant", DateTime.UtcNow),
                    new XAttribute("Destination", Destination),
                    new XAttribute("ForceAuthn", "false"),
                    new XAttribute("IsPassive", "false"),
                    new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
                    new XElement(SamlNamespaces.SAML_NS + "Issuer", Issuer)
                    );
            }
        }

        const string UID = "urn:mace:dir:attribute-def:uid";
        const string MAIL = "urn:mace:dir:attribute-def:mail";
        const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
        const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";
        static readonly Lazy<ILog> LOG = LazyLog.For(typeof(SsoProcessor));

        public static string GetRedirectUrl(string relayState, ServiceProviderConfig client, string singleSignOnServiceUrl)
        {
            var request = new AuthnRequest {
                Destination = singleSignOnServiceUrl,
                Issuer = client.entity,
            };
            var qs = CreateQueryString(request, relayState, client.certificate);
            return CreateUrl(request, qs);
        }

        public static XElement Response(string samlResponse)
        {
            LOG.Debug(() => "Response");
            return ReceiveSamlResponse(samlResponse);
        }

        public static SsoAttributes? Process(XElement response, X509Certificate2 certificate)
        {
            var assertion = GetAssertion(response, certificate);
            return assertion == null ? default(SsoAttributes?) : GetAttributes(assertion, certificate);
        }

        public static XElement GetAssertion(XElement response, X509Certificate2 certificate)
        {
            LOG.Debug(() => $"GetAssertion(response='{response}')");

            if (response.Descendants(SamlNamespaces.SAMLP_NS + "StatusCode").Single().Attribute("Value").Value == "urn:oasis:names:tc:SAML:2.0:status:Success") {
                var result = response.Descendants(SamlNamespaces.SAML_NS + "Assertion").Single();
                Validate(result, certificate);
                return result;
            }

            LOG.Debug(() => "GetAssertion: not successfull");
            return null;
        }

        public static SsoAttributes GetAttributes(XElement assertion, X509Certificate2 certificate)
        {
            LOG.Debug(() => $"GetAttributes(assertion='{assertion}')");

            Validate(assertion, certificate);
            return new SsoAttributes {
                uid = GetAttribute(assertion, UID),
                domain = GetAttribute(assertion, DOMAIN),
                email = GetAttributes(assertion, MAIL),
                roles = GetAttributes(assertion, ROLE),
            };
        }

        static XElement ReceiveSamlResponse(string SamlResponse)
        {
            var response = SamlResponse;
            if (response != null) {
                var result = XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(response)), LoadOptions.PreserveWhitespace);
                return result.Root;
            }
            return null;
        }

        static string CreateUrl(AuthnRequest req, NameValueCollection qs)
        {
            var builder = new UriBuilder(req.Destination);
            if (string.IsNullOrEmpty(builder.Query)) {
                builder.Query = ToQueryString(qs);
            } else {
                builder.Query = builder.Query.Substring(1) + "&" + ToQueryString(qs);
            }
            return builder.ToString();
        }

        static NameValueCollection CreateQueryString(AuthnRequest req, string relayState, X509Certificate2 cer)
        {
            var result = new NameValueCollection();
            result.Add("SAMLRequest", req.Encode());
            if (!string.IsNullOrWhiteSpace(relayState)) {
                result.Add("RelayState", relayState);
            }
            result.Add("SigAlg", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
            result.Add("Signature", Signature(result, cer.PrivateKey));
            return result;
        }

        static string Signature(NameValueCollection qs, AsymmetricAlgorithm key)
        {
            var data = Encoding.UTF8.GetBytes(ToQueryString(qs));
            var result = ((RSACryptoServiceProvider)key).SignData(data, new SHA1CryptoServiceProvider());
            return Convert.ToBase64String(result);
        }

        static string ToQueryString(NameValueCollection qs)
        {
            var result = new StringBuilder();
            foreach (string key in qs.Keys) {
                if (result.Length > 0) {
                    result.Append("&");
                }
                result.AppendFormat("{0}={1}", key, Uri.EscapeDataString(qs[key]));
            }
            return result.ToString();
        }

        static string GetAttribute(XElement assertion, string key)
        {
            var result = GetNullableAttribute(assertion, key);
            if (result == null) {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            return result;
        }

        static string GetNullableAttribute(XElement assertion, string key)
        {
            return (from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                where attribute.Parent.Attribute("Name").Value == key
                select attribute.Value).SingleOrDefault();
        }

        static string[] GetAttributes(XElement assertion, string key)
        {
            return (from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                where attribute.Parent.Attribute("Name").Value == key
                select attribute.Value).ToArray();
        }

        public static Saml20MetaData GetMetaData(IdentityProviderConfig idp, ServiceProviderConfig sp)
            => ValidatedSaml20MetaData(idp, ConextMetaDataXml(idp, sp));

        static XElement ConextMetaDataXml(IdentityProviderConfig idp, ServiceProviderConfig sp)
        {
            var uri = idp.identity + "?sp-entity-id=" + Uri.EscapeDataString(sp.entity);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                return XElement.Load(reader, LoadOptions.PreserveWhitespace);
        }

        static Saml20MetaData ValidatedSaml20MetaData(IdentityProviderConfig idp, XElement xml)
        {
            Validate(xml, idp.certificate);
            return new Saml20MetaData(xml);
        }

        static readonly XmlSchemaSet schemaSet = new XmlSchemaSet { XmlResolver = null };

        static SsoProcessor()
        {
            var settings = new XmlReaderSettings {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Parse
            };

            var schemaResources = new SingleSignOnSchemaResources();
            foreach (var resName in schemaResources.GetResourceNames()) {
                if (resName.EndsWith(".xsd") || resName.EndsWith(".xsd.intellisensehack")) {
                    using (var stream = schemaResources.GetResource(resName))
                    using (var reader = XmlReader.Create(stream, settings))
                        schemaSet.Add(XmlSchema.Read(reader, null));
                }
            }
        }

        static void Validate(XElement assertion, X509Certificate2 cer)
        {
            Validate(assertion);

            var doc = new XmlDocument {
                PreserveWhitespace = true,
            };

            using (var reader = assertion.CreateReader())
                doc.Load(reader);

            var dsig = new SignedXml(doc);
            dsig.LoadXml(doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
            if (!dsig.CheckSignature(cer.PublicKey.Key)) {
                throw new CryptographicException("metadata not signed");
            }
        }

        public static void Validate(XElement assertion)
        {
            new XDocument(assertion).Validate(schemaSet, null, false);
        }
    }
}
