using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
		public void GetCertificate([Values(false, true)] bool self, [Values(false, true)] bool test)
		{
			X509Certificate2 cer = MetaDataFactory.GetCertificate(self, test);
			Assert.That(cer, Is.Not.Null);
		}

		[Test]
		public void GetIdPEntity([Values(null, "rug", "fontys")] string organisation, [Values(false, true)] bool test)
		{
			string idp = MetaDataFactory.GetIdPEntity(organisation, test);
			Assert.That(idp, Is.Not.Null);
		}

		[Test]
		public void GetIdPInstance([Values(false, true)] bool self, [Values(false, true)] bool test)
		{
			SAML20MetaData md = MetaDataFactory.GetIdPInstance(self, test);
			Assert.That(md, Is.Not.Null);
		}
	}

	[TestFixture]
	public class MetaDataTest
	{
		private IEnumerable<TestCaseData> IdPSSODescriptorData()
		{
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity(null, false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity(null, true)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity("rug", false));
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity("rug", true));
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity("fontys", false));
			yield return new TestCaseData(false, false, MetaDataFactory.GetIdPEntity("fontys", true)).Throws(typeof(InvalidOperationException));

			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity(null, false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity(null, true)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity("rug", false));
			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity("rug", true));
			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity("fontys", false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(false, true, MetaDataFactory.GetIdPEntity("fontys", true));

			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity(null, false));
			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity(null, true)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity("rug", false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity("rug", true)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity("fontys", false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, false, MetaDataFactory.GetIdPEntity("fontys", true)).Throws(typeof(InvalidOperationException));

			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity(null, false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity(null, true));
			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity("rug", false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity("rug", true)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity("fontys", false)).Throws(typeof(InvalidOperationException));
			yield return new TestCaseData(true, true, MetaDataFactory.GetIdPEntity("fontys", true)).Throws(typeof(InvalidOperationException));
		}

		[Test, TestCaseSource("IdPSSODescriptorData")]
		public void IdPSSODescriptor(bool self, bool test, string id)
		{
			SAML20MetaData sut = MetaDataFactory.GetIdPInstance(self, test);
			XElement idp = sut.IdPSSODescriptor(id);
			string sso = sut.SingleSignOnService(idp);
			Assert.That(sso, Is.Not.Null);
		}
	}
}
