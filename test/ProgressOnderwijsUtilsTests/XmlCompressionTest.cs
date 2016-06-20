using System.IO;
using System.Text;
using System.Xml.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Test;
using ProgressOnderwijsUtils;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    class XmlCompressionTest
    {
        [Test]
        public void TrivialDocIsNotChanged()
        {
            var doc = XDocument.Parse("<test/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test />");
        }

        [Test]
        public void DocumentWithUnusedRootNamespaceHasNamespaceRemoved()
        {
            var doc = XDocument.Parse("<test xmlns:bla='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test />");
        }

        [Test]
        public void DocumentWithDefaultNsIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\" />");
        }

        [Test]
        public void XmlDeclarationHasNoImpact()
        {
            var doc = XDocument.Parse("<?xml version=\"1.0\"?><test xmlns='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\" />");
        }

        [Test]
        public void DocumentWithChildNamespaceIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns='bla'><hmm:this xmlns:hmm='foo'/></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\"><hmm:this xmlns:hmm=\"foo\" /></test>");
        }

        [Test]
        public void DocumentWithNamespaceUsedInDescendantIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns:hmm='bla'><hmm:this /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><hmm:this /></test>");
        }

        [Test]
        public void DocumentWithNamespaceUsedInDescendantAttributeIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns:hmm='bla'><this><that hmm:id='' /></this></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><this><that hmm:id=\"\" /></this></test>");
        }

        [Test]
        public void UnusualAliasesAreNormalized()
        {
            var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsd:foo='' notxsi:nil='true' /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsd:foo=\"\" xsi:nil=\"true\" /></test>");
        }

        [Test]
        public void UnusedAreRemoved()
        {
            var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsi:nil='true' /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsi:nil=\"true\" /></test>");
        }

        static readonly Encoding UTF8 = Encoding.UTF8;

        [Test]
        public void SaveToUtf8ContainsNoByteOrderMark()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            PAssert.That(() => bytes[0] == (byte)'<');
        }

        [Test]
        public void SaveToUtf8IsUtf8()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Test]
        public void SaveToUtf8CanRoundTrip()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            var reloaded = XDocument.Load(new MemoryStream(bytes));

            var str = reloaded.ToString();

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Test]
        public void SaveToUtf8ExcludesXmlDeclaration()
        {
            var doc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf16\"?><test>Ƒϕϕ</test>");

            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Test]
        public void SaveToUtf8ExcludesIndentation()
        {
            var doc = XDocument.Parse(@"<test>
  <nested>
    <elements>
      <here>
        Ƒϕϕ
      </here>
    </elements>
  </nested>
</test>");

            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == @"<test><nested><elements><here>
        Ƒϕϕ
      </here></elements></nested></test>");
        }
    }
}
