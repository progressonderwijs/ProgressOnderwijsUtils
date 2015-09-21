﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Progress.Sso;
using ProgressOnderwijsUtils;
using Progress.WebApp.RequestRouting;

namespace ProgressOnderwijsUtilsTests
{
    public static class ConextTestHelpers
    {
        public static Saml20MetaData Saml20MetaData(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
        {
            IdentityProviderConfig server = MetaDataFactory.GetIdentityProvider(idp);
            ServiceProviderConfig? client = sp.HasValue && db.HasValue
                ? MetaDataFactory.GetServiceProvider(sp.Value, db.Value)
                : default(ServiceProviderConfig?);

            var sut = MetaDataFactory.GetMetaData(server, client);
            return sut;
        }
    }

    public sealed class ConextMetaDataFactoryTest
    {
        [Test]
        public void Generate()
        {
            XmlDocument md = MetaDataFactory.Generate();
            Assert.That(md, Is.Not.Null);
        }

        [TestCase(ServiceProvider.P3W, DatabaseVersion.ProductieDB), TestCase(ServiceProvider.P3W, DatabaseVersion.TestDB),
         TestCase(ServiceProvider.PNet, DatabaseVersion.ProductieDB), TestCase(ServiceProvider.PNet, DatabaseVersion.TestDB),
         TestCase(ServiceProvider.PNet, DatabaseVersion.AcceptatieDB),
         TestCase(ServiceProvider.PNet, DatabaseVersion.OntwikkelDB), TestCase(ServiceProvider.PNet, DatabaseVersion.BronHODB),
         TestCase(ServiceProvider.Student, DatabaseVersion.ProductieDB), TestCase(ServiceProvider.Student, DatabaseVersion.TestDB),
         TestCase(ServiceProvider.Student, DatabaseVersion.OntwikkelDB), TestCase(ServiceProvider.StudentOAuth, DatabaseVersion.ProductieDB),
         TestCase(ServiceProvider.StudentOAuth, DatabaseVersion.TestDB), TestCase(ServiceProvider.StudentOAuth, DatabaseVersion.OntwikkelDB)]
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

        [TestCase(IdentityProvider.Conext), TestCase(IdentityProvider.ConextWayf)]
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

        [TestCase(IdentityProvider.ConextWayf, null, null), TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.AcceptatieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.BronHODB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.OntwikkelDB)]
        public void GetEntities(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
        {
            var sut = MetaDataFactory.GetEntities(idp, sp, db);
            Assert.That(sut, Is.Not.Null);
        }

        [TestCase(IdentityProvider.ConextWayf, null, null), TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.AcceptatieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.BronHODB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.OntwikkelDB)]
        public void GetMetaData(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
        {
            var sut = ConextTestHelpers.Saml20MetaData(idp, sp, db);
            Assert.That(sut, Is.Not.Null);
        }
    }

    public sealed class Saml20MetaDataTest
    {
        [TestCase(IdentityProvider.ConextWayf, null, null), TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.AcceptatieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.BronHODB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.OntwikkelDB)]
        public void GetEntities(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
        {
            var sut = ConextTestHelpers.Saml20MetaData(idp, sp, db);
            Assert.That(sut.GetEntities(), Is.EquivalentTo(MetaDataFactory.GetEntities(idp, sp, db).Values.Distinct()));
        }

        [TestCase(IdentityProvider.ConextWayf, null, null), TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.P3W, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.TestDB), TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.AcceptatieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.PNet, DatabaseVersion.BronHODB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.Student, DatabaseVersion.OntwikkelDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.ProductieDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.TestDB),
         TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, DatabaseVersion.OntwikkelDB)]
        public void SingleSignOnService(IdentityProvider idp, ServiceProvider? sp, DatabaseVersion? db)
        {
            var sut = ConextTestHelpers.Saml20MetaData(idp, sp, db);
            foreach (var entity in MetaDataFactory.GetEntities(idp, sp, db).Values.Distinct()) {
                var sso = sut.SingleSignOnService(entity);
                Assert.That(sso, Is.Not.Null);
                Assert.That(Uri.IsWellFormedUriString(sso, UriKind.Absolute));
            }
        }
    }

    public sealed class SingleSignOnHandlerTest
    {
        [Test]
        public void RelayStateSerialization()
        {
            var sut = new SingleSignOnHandler.RelayState {
                newSession = false,
                idp = IdentityProvider.Conext,
                uri = "http://www.nrc.nl",
            };

            Assert.That(SingleSignOnHandler.Deserialize(SingleSignOnHandler.Serialize(sut)), Is.EqualTo(sut));
        }

        [Test]
        public void GeneratePostToRedirect([Values(false, true)] bool newSession)
        {
            var state = new SingleSignOnHandler.RelayState {
                newSession = newSession,
                idp = IdentityProvider.Conext,
                uri = "https://localhost/webstatic/fontys?pc=123",
            };
            var sut = SingleSignOnHandler.GeneratePostToRedirect(state, new XElement("assertion"));
        }
    }
}
