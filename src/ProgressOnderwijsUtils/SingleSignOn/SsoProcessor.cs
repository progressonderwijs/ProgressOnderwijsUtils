﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using JetBrains.Annotations;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using ProgressOnderwijsUtils.Log4Net;

namespace ProgressOnderwijsUtils.SingleSignOn
{
    public static class SsoProcessor
    {
        const string UID = "urn:mace:dir:attribute-def:uid";
        const string MAIL = "urn:mace:dir:attribute-def:mail";
        const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
        const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";
        static readonly Lazy<ILog> LOG = LazyLog.For(typeof(SsoProcessor));

        [NotNull]
        public static Uri GetRedirectUrl(AuthnRequest request)
        {
            //Don't escape colon: Uri.ToString doesn't either; and this is just a defense-in-depth we don't need
            //ref: https://github.com/aspnet/HttpAbstractions/commit/1e9d57f80ca883881804292448fff4de8b112733
            string Escape(string str) => Uri.EscapeDataString(str).Replace("%3A", ":");
            string EncodeQueryParameter(string key, string value) => Escape(key) + "=" + Escape(value);

            var samlRequestQueryString = EncodeQueryParameter("SAMLRequest", request.Encode()) + "&" + EncodeQueryParameter("SigAlg", "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
            var rsaPrivateKey = request.Issuer.certificate.GetRSAPrivateKey();
            var base64Signature = Convert.ToBase64String(rsaPrivateKey.SignData(Encoding.UTF8.GetBytes(samlRequestQueryString), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            var signedQueryString = samlRequestQueryString + "&" + EncodeQueryParameter("Signature", base64Signature);
            return new Uri(request.Destination + "?" + signedQueryString);
        }

        [CanBeNull]
        public static XElement Response([CanBeNull] string samlResponse)
        {
            LOG.Debug(() => "Response");
            return samlResponse != null ? XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse)), LoadOptions.PreserveWhitespace).Root : null;
        }

        [CanBeNull]
        public static XElement GetAssertion([NotNull] XElement response, X509Certificate2 certificate)
        {
            LOG.Debug(() => $"GetAssertion(response='{response}')");

            var statusCodes = response.Descendants(SamlNamespaces.SAMLP_NS + "StatusCode").ToArray();
            if (statusCodes.Length > 1) {
                return null;
            }

            var statusCodeAttribute = statusCodes[0].Attribute("Value")
                ?? throw new InvalidOperationException("Missing status code attribute");

            if (statusCodeAttribute.Value == "urn:oasis:names:tc:SAML:2.0:status:Success") {
                var result = response.Descendants(SamlNamespaces.SAML_NS + "Assertion").Single();
                Validate(result, certificate);
                return result;
            }

            LOG.Debug(() => "GetAssertion: not successfull");
            return null;
        }

        public static SsoAttributes GetAttributes(XElement assertion, [NotNull] X509Certificate2 certificate)
        {
            LOG.Debug(() => $"GetAttributes(assertion='{assertion}')");

            Validate(assertion, certificate);
            var authnStatement = assertion.Element(SamlNamespaces.SAML_NS + "AuthnStatement")
                ?? throw new InvalidOperationException("Missing AuthnStatement element");
            return new SsoAttributes {
                uid = GetAttribute(assertion, UID),
                domain = GetAttribute(assertion, DOMAIN),
                email = GetAttributes(assertion, MAIL),
                roles = GetAttributes(assertion, ROLE),
                InResponseTo = GetInResponseTo(assertion),
                AuthnInstant = (DateTime)authnStatement.Attribute("AuthnInstant"),
                AuthnContextClassRef = (string)authnStatement.Element(SamlNamespaces.SAML_NS + "AuthnContext").Element(SamlNamespaces.SAML_NS + "AuthnContextClassRef"),
            };
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
                throw new InvalidOperationException("Sequence contains no elements");
            }
            return result;
        }

        [CanBeNull]
        static string GetNullableAttribute([NotNull] XElement assertion, string key)
        {
            return (
                from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                where attribute.Parent.Attribute("Name").Value == key
                select attribute.Value
                ).SingleOrDefault();
        }

        [NotNull]
        static string[] GetAttributes([NotNull] XElement assertion, string key)
        {
            return (from attribute in assertion.Descendants(SamlNamespaces.SAML_NS + "AttributeValue")
                where attribute.Parent.Attribute("Name").Value == key
                select attribute.Value).ToArray();
        }

        static readonly MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions {
            SizeLimit = 10
        });

        public static Saml20MetaData GetMetaData(IdentityProviderConfig idp, ServiceProviderConfig sp)
        {
            var uri = $"{idp.identity}?{idp.MetaDataQueryParameter}={Uri.EscapeDataString(sp.entity)}";
            return memoryCache.GetOrCreate(uri, entry => {
                var document = DownloadMetaData(uri);
                var validUntil = document.DocumentElement.GetAttribute("validUntil");
                entry.AbsoluteExpiration = string.IsNullOrEmpty(validUntil)
                    ? default(DateTime?)
                    : XmlConvert.ToDateTime(validUntil, XmlDateTimeSerializationMode.RoundtripKind);
                entry.Size = 1;
                return ValidatedSaml20MetaData(idp, document);
            });
        }

        [NotNull]
        static XmlDocument DownloadMetaData([NotNull] string uri)
        {
            var document = new XmlDocument {
                PreserveWhitespace = true,
            };
            document.Load(uri);
            return document;
        }

        [NotNull]
        static Saml20MetaData ValidatedSaml20MetaData(IdentityProviderConfig idp, [NotNull] XmlDocument document)
        {
            ValidateSignature(document, idp.certificate);
            var xml = XElement.Parse(document.OuterXml, LoadOptions.PreserveWhitespace);
            ValidateSchema(xml);
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

        static void Validate([NotNull] XElement assertion, [NotNull] X509Certificate2 cer)
        {
            ValidateSchema(assertion);

            var doc = new XmlDocument {
                PreserveWhitespace = true,
            };
            using (var reader = assertion.CreateReader())
                doc.Load(reader);

            ValidateSignature(doc, cer);
        }

        static void ValidateSignature([NotNull] XmlDocument document, [NotNull] X509Certificate2 cer)
        {
            var dsig = new SignedXml(document);
            dsig.LoadXml(document.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#").Cast<XmlElement>().Single());
            if (!dsig.CheckSignature(cer.PublicKey.Key)) {
                throw new CryptographicException("metadata not signed");
            }
        }

        public static void ValidateSchema(XElement assertion)
        {
            new XDocument(assertion).Validate(schemaSet, null, false);
        }
    }
}
