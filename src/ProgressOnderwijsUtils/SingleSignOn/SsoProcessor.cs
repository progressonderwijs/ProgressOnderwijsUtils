using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public static class SsoProcessor
    {
        const string UID = "urn:mace:dir:attribute-def:uid";
        const string MAIL = "urn:mace:dir:attribute-def:mail";
        const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
        const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";

        [NotNull]
        public static Uri GetRedirectUrl(AuthnRequest request)
        {
            //Don't escape colon: Uri.ToString doesn't either; and this is just a defense-in-depth we don't need
            //ref: https://github.com/aspnet/HttpAbstractions/commit/1e9d57f80ca883881804292448fff4de8b112733
            string Escape(string str)
                => Uri.EscapeDataString(str).Replace("%3A", ":");
            string EncodeQueryParameter(string key, string value)
                => Escape(key) + "=" + Escape(value);

            var samlRequestQueryString = EncodeQueryParameter("SAMLRequest", request.EncodeAsQueryArgument()) + "&" + EncodeQueryParameter("SigAlg", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
            var rsaPrivateKey = request.Issuer.certificate.GetRSAPrivateKey();
            var base64Signature = Convert.ToBase64String(rsaPrivateKey.SignData(Encoding.UTF8.GetBytes(samlRequestQueryString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            var signedQueryString = samlRequestQueryString + "&" + EncodeQueryParameter("Signature", base64Signature);
            return new Uri(request.Destination + "?" + signedQueryString);
        }

        [CanBeNull]
        static XElement GetAssertion([NotNull] XElement response)
        {
            var statusCodes = response.Descendants(SamlNamespaces.SAMLP_NS + "StatusCode").ToArray();
            if (statusCodes.Length > 1) {
                return null;
            }

            var statusCodeAttribute = statusCodes[0].Attribute("Value")
                ?? throw new InvalidOperationException("Missing status code attribute");

            if (statusCodeAttribute.Value == "urn:oasis:names:tc:SAML:2.0:status:Success") {
                return response.Descendants(SamlNamespaces.SAML_NS + "Assertion").Single();
            }

            return null;
        }

        public static Maybe<SsoAttributes, string> GetAttributes(string rawSamlResponse, [NotNull] X509Certificate2 certificate)
        {
            var rawXml = Encoding.UTF8.GetString(Convert.FromBase64String(rawSamlResponse));
            var xml = XElement.Parse(rawXml);

            try {
                ValidateSchema(xml);
            } catch (XmlSchemaValidationException e) {
                return Maybe.Error($"Response invalid: {e.Message}");
            }

            var doc = new XmlDocument {
                PreserveWhitespace = true,
            };
            doc.LoadXml(rawXml);

            var dsig = new SignedXml(doc);
            dsig.LoadXml(doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
            if (!dsig.CheckSignature(certificate.PublicKey.Key)) {
                return Maybe.Error("Signature invalid");
            }

            var assertion = GetAssertion(xml);
            var authnStatement = assertion?.Element(SamlNamespaces.SAML_NS + "AuthnStatement");
            if (authnStatement == null) {
                return Maybe.Error("Missing AuthnStatement element");
            }

            return Maybe.Ok(
                new SsoAttributes {
                    uid = GetAttribute(assertion, UID),
                    domain = GetNullableAttribute(assertion, DOMAIN),
                    email = GetAttributes(assertion, MAIL),
                    roles = GetAttributes(assertion, ROLE),
                    InResponseTo = GetInResponseTo(assertion),
                    IssueInstant = (DateTime)assertion.Attribute("IssueInstant"),
                    AuthnContextClassRef = (string)authnStatement.Element(SamlNamespaces.SAML_NS + "AuthnContext").Element(SamlNamespaces.SAML_NS + "AuthnContextClassRef"),
                });
        }

        [CanBeNull]
        static string GetInResponseTo([NotNull] XElement assertion)
        {
            // ReSharper disable PossibleNullReferenceException
            var rawInResponseTo = (string)assertion
                .Element(SamlNamespaces.SAML_NS + "Subject")
                .Element(SamlNamespaces.SAML_NS + "SubjectConfirmation")
                .Element(SamlNamespaces.SAML_NS + "SubjectConfirmationData")
                .Attribute("InResponseTo");
            // ReSharper restore PossibleNullReferenceException
            return XmlConvert.DecodeName(rawInResponseTo);
        }

        [NotNull]
        static string GetAttribute([NotNull] XElement assertion, string key)
        {
            var result = GetNullableAttribute(assertion, key);
            if (result == null) {
                throw new InvalidOperationException($"No value for attribute {key}");
            }

            return result;
        }

        [CanBeNull]
        static string GetNullableAttribute([NotNull] XElement assertion, string key)
            => (
                from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                // ReSharper disable PossibleNullReferenceException
                where attribute.Parent.Attribute("Name").Value == key
                // ReSharper restore PossibleNullReferenceException
                select attribute.Value
                ).SingleOrNull();

        [NotNull]
        static string[] GetAttributes([NotNull] XElement assertion, string key)
            => (from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                // ReSharper disable PossibleNullReferenceException
                where attribute.Parent.Attribute("Name").Value == key
                // ReSharper restore PossibleNullReferenceException
                select attribute.Value).ToArray();

        static readonly XmlSchemaSet schemaSet = new XmlSchemaSet { XmlResolver = null };

        static SsoProcessor()
        {
            var settings = new XmlReaderSettings {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Parse
            };

            var schemaResources = new SingleSignOnSchemaResources();
            foreach (var resName in schemaResources.GetResourceNames()) {
                if (resName.EndsWith(".xsd", StringComparison.Ordinal) || resName.EndsWith(".xsd.intellisensehack", StringComparison.Ordinal)) {
                    using (var stream = schemaResources.GetResource(resName))
                    using (var reader = XmlReader.Create(stream, settings)) {
                        schemaSet.Add(XmlSchema.Read(reader, null));
                    }
                }
            }
        }

        public static void ValidateSchema(XElement assertion)
            => new XDocument(assertion).Validate(schemaSet, null, false);
    }
}
