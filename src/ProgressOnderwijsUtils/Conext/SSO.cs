using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Xml.Linq;
using log4net;
using ProgressOnderwijsUtils.Log4Net;

namespace ProgressOnderwijsUtils.Conext
{
	public enum ServiceProvider
	{
		P3W,
		PNet,
		Student,
		StudentOAuth,
	}

	public enum IdentityProvider
	{
		Conext,
		ConextWayf,
	}

	public enum Entity
	{
		Unknown,

		Fontys,
		Stenden,
		UvA,
		VU,
		RuG,
	}

	[Serializable]
	public struct Attributes
	{
		public string uid;
		public string domain;
		public IEnumerable<string> email;
		public IEnumerable<string> roles;

		public override string ToString()
		{
			return string.Format("uid='{0}'; domain='{1}'; emails='{2}'; roles='{3}'",
				uid, domain, StringUtils.ToFlatDebugString(email), StringUtils.ToFlatDebugString(roles));
		}
	}

	public static class SSO
	{
		private struct AuthnRequest
		{
			public string Destination { get; set; }
			public string Issuer { private get; set; }

			public string Encode()
			{
				byte[] xml = Encoding.UTF8.GetBytes(ToXml().ToString());
				using (MemoryStream stream = new MemoryStream())
				{
					using (DeflateStream deflate = new DeflateStream(stream, CompressionMode.Compress))
					{
						deflate.Write(xml, 0, xml.Length);
					}
					return Convert.ToBase64String(stream.ToArray());
				}
			}

			private XElement ToXml()
			{
				return new XElement(SchemaSet.SAMLP_NS + "AuthnRequest",
					new XAttribute(XNamespace.Xmlns + "saml", SchemaSet.SAML_NS.NamespaceName),
					new XAttribute(XNamespace.Xmlns + "sampl", SchemaSet.SAMLP_NS.NamespaceName),
					new XAttribute("ID", "_" + Guid.NewGuid()),
					new XAttribute("Version", "2.0"),
					new XAttribute("IssueInstant", DateTime.UtcNow),
					new XAttribute("Destination", Destination),
					new XAttribute("ForceAuthn", "false"),
					new XAttribute("IsPassive", "false"),
					new XAttribute("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"),
					new XElement(SchemaSet.SAML_NS + "Issuer", Issuer)
				);
			}
		}

		private const string UID = "urn:mace:dir:attribute-def:uid";
		private const string MAIL = "urn:mace:dir:attribute-def:mail";
		private const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
		private const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";

		private static readonly ILog LOG = LogManager.GetLogger(typeof(SSO));

		public static void Request(HttpResponse response, ServiceProvider sp, DatabaseVersion db, IdentityProvider idp, Entity entity, string relayState = null)
		{
			LOG.Debug(() => string.Format("Request(sp='{0}', db='{1}', idp='{2}', entity='{3}')", sp, db, idp, entity));

			ServiceProviderConfig client = MetaDataFactory.GetServiceProvider(sp, db);
			IdentityProviderConfig server = MetaDataFactory.GetIdentityProvider(idp);
			IDictionary<Entity, string> entities = MetaDataFactory.GetEntities(idp, sp, db);

			Saml20MetaData md = MetaDataFactory.GetMetaData(server, client);
			AuthnRequest request = new AuthnRequest
			{
				Destination = md.SingleSignOnService(entities[entity]),
				Issuer = client.entity,
			};
			SendSamlRequest(response, request, relayState, client.certificate);
		}

		public static XElement Response(HttpRequest request, out string relayState)
		{
			LOG.Debug(() => string.Format("Response"));

			return ReceiveSamlResponse(request, out relayState);
		}

		public static Attributes? Process(XElement response, IdentityProvider idp)
		{
			var assertion = GetAssertion(response, idp);
			return assertion == null ? default(Attributes?) : GetAttributes(assertion, idp);
		}

		public static XElement GetAssertion(XElement response, IdentityProvider idp)
		{
			LOG.Debug(() => string.Format("GetAssertion(response='{0}', idp='{1}')", response, idp));

			if (response.Descendants(SchemaSet.SAMLP_NS + "StatusCode").Single().Attribute("Value").Value == "urn:oasis:names:tc:SAML:2.0:status:Success")
			{
				var result = response.Descendants(SchemaSet.SAML_NS + "Assertion").Single();
				// TODO: MetaDataFactory.Validate(result, MetaDataFactory.GetIdentityProvider(idp).certificate);
				return result;
			}

			LOG.Debug(() => string.Format("GetAssertion: not successfull"));
			return null;
		}

		public static Attributes GetAttributes(XElement assertion, IdentityProvider idp)
		{
			LOG.Debug(() => string.Format("GetAttributes(assertion='{0}')", assertion));

			//TODO: MetaDataFactory.Validate(assertion, MetaDataFactory.GetIdentityProvider(idp).certificate);
			return new Attributes
			{
				uid = GetAttribute(assertion, UID),
				domain = GetAttribute(assertion, DOMAIN),
				email = GetAttributes(assertion, MAIL),
				roles = GetAttributes(assertion, ROLE),
			};
		}

		private static void SendSamlRequest(HttpResponse response, AuthnRequest req, string relayState, X509Certificate2 cer)
		{
			var qs = CreateQueryString(req, relayState, cer);
			var url = CreateUrl(req, qs);
			response.Redirect(url);
		}

		private static XElement ReceiveSamlResponse(HttpRequest request, out string relayState)
		{
			relayState = request.Form["RelayState"];
			var response = request.Form["SAMLResponse"];
			if (response != null)
			{
				var result = XDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(response)));
				result.Validate(null);
				return result.Root;
			}
			return null;
		}

		private static string CreateUrl(AuthnRequest req, NameValueCollection qs)
		{
			UriBuilder builder = new UriBuilder(req.Destination);
			if (string.IsNullOrEmpty(builder.Query))
			{
				builder.Query = ToQueryString(qs);
			}
			else
			{
				builder.Query = builder.Query.Substring(1) + "&" + ToQueryString(qs);
			}
			return builder.ToString();
		}

		private static NameValueCollection CreateQueryString(AuthnRequest req, string relayState, X509Certificate2 cer)
		{
			NameValueCollection result = new NameValueCollection();
			result.Add("SAMLRequest", req.Encode());
			if (!string.IsNullOrWhiteSpace(relayState))
			{
				result.Add("RelayState", relayState);
			}
			result.Add("SigAlg", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
			result.Add("Signature", Signature(result, cer.PrivateKey));
			return result;
		}

		private static string Signature(NameValueCollection qs, AsymmetricAlgorithm key)
		{
			byte[] data = Encoding.UTF8.GetBytes(ToQueryString(qs));
			byte[] result = ((RSACryptoServiceProvider)key).SignData(data, new SHA1CryptoServiceProvider());
			return Convert.ToBase64String(result);
		}

		private static string ToQueryString(NameValueCollection qs)
		{
			StringBuilder result = new StringBuilder();
			foreach (string key in qs.Keys)
			{
				if (result.Length > 0)
				{
					result.Append("&");
				}
				result.AppendFormat("{0}={1}", key, Uri.EscapeDataString(qs[key]));
			}
			return result.ToString();
		}

		private static string GetAttribute(XElement assertion, string key)
		{
			string result = GetNullableAttribute(assertion, key);
			if (result == null) throw new InvalidOperationException("Sequence contains no elements");
			return result;
		}

		private static string GetNullableAttribute(XElement assertion, string key)
		{
			//LOG.Debug(() => string.Format("GetAttribute(assertion='{0}', key='{1}'", assertion, key));

			return (from attribute in assertion.Descendants(SchemaSet.SAML_NS + "AttributeValue")
			        where attribute.Parent.Attribute("Name").Value == key
			        select attribute.Value).SingleOrDefault();
		}

		private static IEnumerable<string> GetAttributes(XElement assertion, string key)
		{
			return (from attribute in assertion.Descendants(SchemaSet.SAML_NS + "AttributeValue")
			        where attribute.Parent.Attribute("Name").Value == key
			        select attribute.Value).ToArray();
		}
	}
}
