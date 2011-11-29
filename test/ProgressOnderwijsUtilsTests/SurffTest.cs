using System;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using ProgressOnderwijsUtils.Surff;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class MetaDataFactoryTest
	{
		[Test]
		public void Generate([Values(false, true)] bool test)
		{
			XmlDocument md = MetaDataFactory.Generate(test);
			Assert.That(md, Is.Not.Null);
		}

		[Test]
		public void GetSPEntity([Values(ServiceProvider.P3W, ServiceProvider.PNet)] ServiceProvider sp)
		{
			Assert.That(MetaDataFactory.GetSPEntity(sp), Is.Not.Null);
		}

		[Test]
		public void GetSPCertificate([Values(ServiceProvider.P3W, ServiceProvider.PNet)] ServiceProvider sp)
		{
			Assert.That(MetaDataFactory.GetSPCertificate(sp), Is.Not.Null);
		}

		[Test]
		public void GetIdPProvider(
			[Values(IdentityProvider.Federatie, IdentityProvider.FederatieRug, IdentityProvider.FederatieFontys, IdentityProvider.Conext)] IdentityProvider idp, 
			[Values(false, true)] bool test)
		{
			AssertIdp(() => MetaDataFactory.GetIdPProvider(idp, test), idp, test);
		}

		[Test]
		public void GetIdPEntity(
			[Values(IdentityProvider.Federatie, IdentityProvider.FederatieRug, IdentityProvider.FederatieFontys, IdentityProvider.Conext)] IdentityProvider idp, 
			[Values(false, true)] bool test)
		{
			AssertIdp(() => MetaDataFactory.GetIdPEntity(idp, test), idp, test);
		}

		[Test]
		public void GetIdPCertificate(
			[Values(IdentityProvider.Federatie, IdentityProvider.FederatieRug, IdentityProvider.FederatieFontys, IdentityProvider.Conext)] IdentityProvider idp, 
			[Values(false, true)] bool test)
		{
			AssertIdp(() => MetaDataFactory.GetIdpCertificate(idp, test), idp, test);
		}

		void AssertIdp(Func<object> method, IdentityProvider idp, bool test)
		{
			Assert.That(method(), Is.Not.Null);
		}
	}

	[TestFixture]
	public class MetaDataTest
	{
		[Test]
		public void IdPSSODescriptor(
			[Values(IdentityProvider.Federatie, IdentityProvider.FederatieRug, IdentityProvider.FederatieFontys, IdentityProvider.Conext )] IdentityProvider idp, 
			[Values(false, true)] bool test)
		{
			SAML20MetaData provider = MetaDataFactory.GetIdPProvider(idp, test);
			string entity = MetaDataFactory.GetIdPEntity(idp, test);
			XElement desc = provider.IdPSSODescriptor(entity);
			string sso = provider.SingleSignOnService(desc);
			Assert.That(sso, Is.Not.Null);
		}
	}
}
