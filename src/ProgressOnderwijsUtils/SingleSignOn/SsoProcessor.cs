﻿using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
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

        public static string GetRedirectUrl(AuthnRequest request)
        {
            var qs = CreateQueryString(request, null, request.Issuer.certificate);
            return CreateUrl(request, qs);
        }

        public static XElement Response(string samlResponse)
        {
            LOG.Debug(() => "Response");
            return samlResponse != null ? XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse)), LoadOptions.PreserveWhitespace).Root : null;
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
                InResponseTo = GetInResponseTo(assertion),
                AuthnInstant = (DateTime)assertion.Element(SamlNamespaces.SAML_NS + "AuthnStatement").Attribute("AuthnInstant"),
            };
        }

        static string GetInResponseTo(XElement assertion)
        {
            var rawInResponseTo = (string)assertion
                .Element(SamlNamespaces.SAML_NS + "Subject")
                .Element(SamlNamespaces.SAML_NS + "SubjectConfirmation")
                .Element(SamlNamespaces.SAML_NS + "SubjectConfirmationData")
                .Attribute("InResponseTo");
            return XmlConvert.DecodeName(rawInResponseTo);
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

        static XmlDocument DownloadMetaData(string uri)
        {
            var document = new XmlDocument {
                PreserveWhitespace = true,
            };
            document.Load(uri);
            return document;
        }

        static Saml20MetaData ValidatedSaml20MetaData(IdentityProviderConfig idp, XmlDocument document)
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

        static void Validate(XElement assertion, X509Certificate2 cer)
        {
            ValidateSchema(assertion);

            var doc = new XmlDocument {
                PreserveWhitespace = true,
            };
            using (var reader = assertion.CreateReader())
                doc.Load(reader);

            ValidateSignature(doc, cer);
        }

        static void ValidateSignature(XmlDocument document, X509Certificate2 cer)
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
