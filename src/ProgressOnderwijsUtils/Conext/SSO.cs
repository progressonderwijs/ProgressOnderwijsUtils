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
	public struct Attributes
	{
		public string uid;
		public string domain;
		public IEnumerable<string> email;
		public IEnumerable<string> roles;

		public override string ToString()
		{
			return string.Format("uid='{0}'; domain='{1}'; emails='{2}'; roles='{3}'",
				uid, domain, email.ToStringFlattened(), roles.ToStringFlattened());
		}
	}

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

	public static class SSO
	{
		private const string UID = "urn:mace:dir:attribute-def:uid";
		private const string MAIL = "urn:mace:dir:attribute-def:mail";
		private const string DOMAIN = "urn:mace:terena.org:attribute-def:schacHomeOrganization";
		private const string ROLE = "urn:mace:dir:attribute-def:eduPersonAffiliation";
		private static readonly XNamespace SAML_NS = "urn:oasis:names:tc:SAML:2.0:assertion";

		private static readonly ILog LOG = LogManager.GetLogger(typeof(SSO));

		public static void Request(HttpResponse response, ServiceProvider sp, IdentityProvider idp, DatabaseVersion db, string relayState = null)
		{
			LOG.Debug(() => string.Format("Request(sp='{0}', idp='{1}', db='{2}')", sp, idp, db));

			ServiceProviderConfig client = MetaDataFactory.GetServiceProvider(sp, db);
			IdentityProviderConfig server = MetaDataFactory.GetIdentityProvider(idp);
			Saml20MetaData md = MetaDataFactory.GetMetaData(server, client);

			AuthnRequest request = new AuthnRequest
			{
				Destination = md.SingleSignOnService(server.identity), // TODO !!!
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

		public static Attributes? Process(object response, IdentityProvider idp)
		{
			LOG.Debug(() => string.Format("Process(response='{0}', idp='{1}')", response, idp));

			SAMLResponse resp = (SAMLResponse)response;
			IdentityProviderConfig config = MetaDataFactory.GetIdentityProvider(idp);

			Attributes? result = null;
			if (resp.IsSuccess())
			{
				XElement assertion = GetAssertion(resp, config);
				result = new Attributes
				{
					uid = GetAttribute(assertion, UID),
					domain = GetAttribute(assertion, DOMAIN),
					email = GetAttributes(assertion, MAIL),
					roles = GetAttributes(assertion, ROLE),
				};
			}
			return result;
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

		private static XElement GetAssertion(SAMLResponse response, IdentityProviderConfig idp)
		{
			XmlElement result = response.GetSignedAssertions()[0];
			if (!SAMLAssertionSignature.Verify(result, idp.certificate))
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
