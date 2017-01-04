using System;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Progress.Business.Data;
using Progress.Business.SingleSignOn;
using Progress.WebApp.RequestRouting;

namespace ProgressOnderwijsUtilsTests
{
    public static class ConextTestHelpers
    {
        public static Saml20MetaData Saml20MetaData(IdentityProvider idp, ServiceProvider? sp, PnetOmgeving? db)
        {
            var server = MetaDataFactory.GetIdentityProvider(idp);
            var client = sp.HasValue && db.HasValue
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
            var md = MetaDataFactory.Generate();
            Assert.That(md, Is.Not.Null);
        }

        [TestCase(ServiceProvider.P3W, PnetOmgeving.Productie)]
        [TestCase(ServiceProvider.P3W, PnetOmgeving.Test)]
        [TestCase(ServiceProvider.PNet, PnetOmgeving.Productie)]
        [TestCase(ServiceProvider.PNet, PnetOmgeving.Test)]
        [TestCase(ServiceProvider.PNet, PnetOmgeving.Acceptatie)]
        [TestCase(ServiceProvider.Student, PnetOmgeving.Productie)]
        [TestCase(ServiceProvider.Student, PnetOmgeving.Test)]
        [TestCase(ServiceProvider.StudentOAuth, PnetOmgeving.Productie)]
        [TestCase(ServiceProvider.StudentOAuth, PnetOmgeving.Test)]
        public void GetServiceProvider(ServiceProvider sp, PnetOmgeving db)
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
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Acceptatie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.TestingContinuously)]
        public void GetEntities(IdentityProvider idp, ServiceProvider? sp, PnetOmgeving? db)
        {
            var sut = MetaDataFactory.GetEntities(idp, sp, db);
            Assert.That(sut, Is.Not.Null);
        }

        [TestCase(IdentityProvider.ConextWayf, null, null)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Acceptatie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Test)]
        public void GetMetaData(IdentityProvider idp, ServiceProvider? sp, PnetOmgeving? db)
        {
            var sut = ConextTestHelpers.Saml20MetaData(idp, sp, db);
            Assert.That(sut, Is.Not.Null);
        }
    }

    public sealed class Saml20MetaDataTest
    {
        [TestCase(IdentityProvider.ConextWayf, null, null)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Acceptatie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Test)]
        public void GetEntities(IdentityProvider idp, ServiceProvider? sp, PnetOmgeving? db)
        {
            var sut = ConextTestHelpers.Saml20MetaData(idp, sp, db);
            Assert.That(sut.GetEntities(), Is.EquivalentTo(MetaDataFactory.GetEntities(idp, sp, db).Values.Distinct()));
        }

        [TestCase(IdentityProvider.ConextWayf, null, null)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.P3W, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.PNet, PnetOmgeving.Acceptatie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.Student, PnetOmgeving.Test)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Productie)]
        [TestCase(IdentityProvider.Conext, ServiceProvider.StudentOAuth, PnetOmgeving.Test)]
        public void SingleSignOnService(IdentityProvider idp, ServiceProvider? sp, PnetOmgeving? db)
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
                idp = IdentityProvider.Conext,
                uri = "http://www.nrc.nl",
            };

            Assert.That(SingleSignOnHandler.Deserialize(SingleSignOnHandler.Serialize(sut)), Is.EqualTo(sut));
        }

        [Test]
        public void GeneratePostToRedirect([Values(false, true)] bool newSession)
        {
            var state = new SingleSignOnHandler.RelayState {
                idp = IdentityProvider.Conext,
                uri = "https://localhost/webstatic/fontys?pc=123",
            };
            SingleSignOnHandler.GeneratePostToRedirect(state, new XElement("assertion"));
        }
    }
}
