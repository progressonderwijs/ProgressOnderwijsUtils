using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ProgressOnderwijsUtils.Log4Net;
using log4net;

namespace ProgressOnderwijsUtils.Conext
{
	public enum ServiceProvider
	{
		P3W,
		PNet,
		Student,
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

	public static class SSO
	{
		private const string UID = "urn:mace:dir:attribute-def:uid";
		private const string MAIL = "urn:mace:dir:attribute-def:mail";
		private const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
		private const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";
		private static readonly XNamespace SAML_NS = "urn:oasis:names:tc:SAML:2.0:assertion";

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
				Issuer = new Issuer(client.entity),
				ForceAuthn = false,
				ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect",
				AssertionConsumerServiceIndex = client.index,
			};
			SendAuthnRequest(request, response, client.certificate, relayState);
		}

		public static object Response(HttpRequest request, out string relayState)
		{
			LOG.Debug(() => string.Format("Response"));

			object result;
			try
			{
				XmlElement response;
				ComponentSpace.SAML2.Profiles.SSOBrowser.ServiceProvider.ReceiveSAMLResponseByHTTPPost(request, out response, out relayState);
				result = new SAMLResponse(response); // TODO: check signature !!!
			}
			catch (SAMLProfileException e)
			{
				LOG.Debug(() => "Response: ", e);
				relayState = ""; // to avoid null-reference exceptions
				result = null;
			}

			return result;
		}

		public static Surff.Attributes? Process(object response, IdentityProvider idp)
		{
			var assertion = GetAssertion(response);
			return assertion == null ? default(Surff.Attributes?) : GetAttributes(assertion, idp);
		}

		public static XmlElement GetAssertion(object response)
		{
			LOG.Debug(() => string.Format("GetAssertion(response='{0}')", response));

			SAMLResponse resp = response as SAMLResponse;
			if (resp != null && resp.IsSuccess())
			{
				return resp.GetSignedAssertions()[0];
			}
			return null;
		}

		public static Surff.Attributes GetAttributes(XmlElement assertion, IdentityProvider idp)
		{
			LOG.Debug(() => string.Format("GetAttributes(assertion='{0}', idp='{1}')", assertion, idp));

			XElement asserted = VerifyAssertion(assertion, idp);
			return new Surff.Attributes
			{
				uid = GetAttribute(asserted, UID),
				domain = GetAttribute(asserted, DOMAIN),
				email = GetAttributes(asserted, MAIL),
				roles = GetAttributes(asserted, ROLE),
			};
		}

		private static void SendAuthnRequest(AuthnRequest req, HttpResponse response, X509Certificate2 cer, string relayState)
		{
			ComponentSpace.SAML2.Profiles.SSOBrowser.ServiceProvider.SendAuthnRequestByHTTPRedirect(
				response, 
				req.Destination, 
				req.ToXml(), 
				relayState,
				cer.PrivateKey);
		}

		private static XElement VerifyAssertion(XmlElement result, IdentityProvider idp)
		{
			IdentityProviderConfig config = MetaDataFactory.GetIdentityProvider(idp);
			if (!SAMLAssertionSignature.Verify(result, config.certificate))
			{
				throw new CryptographicException();
			}
			return XElement.Load(result.CreateNavigator().ReadSubtree());
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

			return (from attribute in assertion.Descendants(SAML_NS + "AttributeValue")
			        where attribute.Parent.Attribute("Name").Value == key
			        select attribute.Value).SingleOrDefault();
		}

		private static IEnumerable<string> GetAttributes(XElement assertion, string key)
		{
			return (from attribute in assertion.Descendants(SAML_NS + "AttributeValue")
			        where attribute.Parent.Attribute("Name").Value == key
			        select attribute.Value).ToArray();
		}
	}
}
