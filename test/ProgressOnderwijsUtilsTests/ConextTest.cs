using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Conext;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class ConextMetaDataFactoryTest
	{
		[Test]
		public void Generate()
		{
			XmlDocument md = MetaDataFactory.Generate();
			Assert.That(md, Is.Not.Null);
		}

		[TestCase(ServiceProvider.P3W, DatabaseVersion.ProductieDB)]
		[TestCase(ServiceProvider.P3W, DatabaseVersion.TestDB)]
		[TestCase(ServiceProvider.PNet, DatabaseVersion.ProductieDB)]
		[TestCase(ServiceProvider.PNet, DatabaseVersion.TestDB)]
		[TestCase(ServiceProvider.PNet, DatabaseVersion.DevTestDB)]
		[TestCase(ServiceProvider.Student, DatabaseVersion.ProductieDB)]
		[TestCase(ServiceProvider.Student, DatabaseVersion.TestDB)]
		[TestCase(ServiceProvider.Student, DatabaseVersion.DevTestDB)]
		public void GetServiceProvider(ServiceProvider sp, DatabaseVersion db)
		{
			var sut = MetaDataFactory.GetServiceProvider(sp, db);
			Assert.That(sut, Is.Not.Null);
			Assert.That(sut.sp, Is.EqualTo(sp));
			Assert.That(sut.entity, Is.Not.Null);
			Assert.That(sut.index, Is.EqualTo(0));
			Assert.That(sut.certificate, Is.Not.Null);
			Assert.That(sut.certificate.HasPrivateKey);
		}

		[TestCase(IdentityProvider.Conext)]
		[TestCase(IdentityProvider.ConextWayf)]
		public void GetIdentityProvider(IdentityProvider idp)
		{
			var sut = MetaDataFactory.GetIdentityProvider(idp);
			Assert.That(sut, Is.Not.Null);
			Assert.That(sut.idp, Is.EqualTo(idp));
			Assert.That(sut.metadata, Is.Not.Null);
			Assert.That(sut.identity, Is.Not.Null);
			Assert.That(sut.certificate, Is.Not.Null);
			Assert.That(!sut.certificate.HasPrivateKey);
		}

		[TestCase(IdentityProvider.ConextWayf, null, null)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.DevTestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.DevTestDB)]
		public void GetEntities(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
		{
			var sut = MetaDataFactory.GetEntities(idp, sp, db);
			Assert.That(sut, Is.Not.Null);
		}

		[TestCase(IdentityProvider.ConextWayf, null, null)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.DevTestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.DevTestDB)]
		public void GetMetaData(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
		{
			var sut = Saml20MetaData(idp, sp, db);
			Assert.That(sut, Is.Not.Null);
		}

		[TestCase(IdentityProvider.ConextWayf, null, null)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.DevTestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB)]
		[TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.DevTestDB)]
		public void SingleSignOnService(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
		{
			var sut = Saml20MetaData(idp, sp, db);
			foreach (var entity in MetaDataFactory.GetEntities(idp, sp, db).Values.Distinct())
			{
				var sso = sut.SingleSignOnService(entity);
				Assert.That(sso, Is.Not.Null);
				Assert.That(Uri.IsWellFormedUriString(sso, UriKind.Absolute));
			}
		}

		private static Saml20MetaData Saml20MetaData(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
		{
			IdentityProviderConfig server = MetaDataFactory.GetIdentityProvider(idp);
			ServiceProviderConfig? client = sp.HasValue && db.HasValue
				? MetaDataFactory.GetServiceProvider(sp.Value, db.Value)
				: default(ServiceProviderConfig?);

			var sut = MetaDataFactory.GetMetaData(server, client);
			return sut;
		}
	}
}
