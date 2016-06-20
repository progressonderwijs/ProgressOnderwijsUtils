using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using ExpressionToCodeLib;
using NUnit.Framework;
using Progress.Business.Inschrijvingen.Studielink;
using Progress.Business.Schemas;
using Progress.Business.Test;
using Progress.Test.Studielink;
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
            var doc = XDocument.Parse("<test>Ƒоо</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            PAssert.That(() => bytes[0] == (byte)'<');
        }

        [Test]
        public void SaveToUtf8IsUtf8()
        {
            var doc = XDocument.Parse("<test>Ƒоо</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒоо</test>");
        }

        [Test]
        public void SaveToUtf8CanRoundTrip()
        {
            var doc = XDocument.Parse("<test>Ƒоо</test>");
            var bytes = XmlCompression.SaveToUtf8(doc);
            var reloaded = XDocument.Load(new MemoryStream(bytes));

            var str = reloaded.ToString();

            PAssert.That(() => str == "<test>Ƒоо</test>");
        }

        [Test]
        public void SaveToUtf8ExcludesXmlDeclaration()
        {
            var doc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf16\"?><test>Ƒоо</test>");

            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒоо</test>");
        }

        [Test]
        public void SaveToUtf8ExcludesIndentation()
        {
            var doc = XDocument.Parse(@"<test>
  <nested>
    <elements>
      <here>
        Ƒоо
      </here>
    </elements>
  </nested>
</test>");

            var bytes = XmlCompression.SaveToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == @"<test><nested><elements><here>
        Ƒоо
      </here></elements></nested></test>");
        }

        [Test]
        public void SaveUsingDeflateWithDictionaryWithoutDictionaryRoundTrips()
        {
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            AssertCompressionCompressesAndRoundTrips(docString, null);
        }

        [Test]
        public void CompressionRoundTripsInComplexExampleWithoutDictionary()
        {
            var xmlString = new ServiceTestBerichten().Bericht(BerichtType.vchmsg03inschrijving);
            var minifiedVchMsg03 = XDocument.Parse(xmlString).ToString(SaveOptions.DisableFormatting);
            AssertCompressionCompressesAndRoundTrips(minifiedVchMsg03, null);
        }

        static void AssertCompressionCompressesAndRoundTrips(string docString, byte[] dictionary)
        {
            var doc = XDocument.Parse(docString);
            var compressedBytes = XmlCompression.SaveUsingDeflateWithDictionary(doc, dictionary);
            PAssert.That(() => compressedBytes.Length < UTF8.GetByteCount(docString));
            var decompressedDoc = XmlCompression.LoadFromDeflateWithDictionary(compressedBytes, dictionary);
            PAssert.That(() => !ReferenceEquals(doc, decompressedDoc), "Using the same reference would be cheating!");
            PAssert.That(() => XNode.DeepEquals(doc, decompressedDoc));
            PAssert.That(() => decompressedDoc.ToString(SaveOptions.DisableFormatting) == docString);
        }

        [Test]
        public void SaveUsingDeflateWithDictionaryRoundTrips()
        {
            var dictionary = UTF8.GetBytes("documentesthis");
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            AssertCompressionCompressesAndRoundTrips(docString, dictionary);
        }

        [Test]
        public void CompressionRoundTripsInComplexExample()
        {
            var xmlString = new ServiceTestBerichten().Bericht(BerichtType.vchmsg03inschrijving);

            var minifiedVchMsg03 = XDocument.Parse(xmlString).ToString(SaveOptions.DisableFormatting);
            var dictionary = UTF8.GetBytes("<typename><tysource sourcetype=\"imsdefault\" /><tyvalue>" + BerichtType.vchmsg03inschrijving + $" xmlns=\"{SchemaSet.KVA5_NS.NamespaceName}\"");
            AssertCompressionCompressesAndRoundTrips(minifiedVchMsg03, dictionary);
        }

        [Test]
        public void SaveUsingDeflateWithDictionaryIsSmaller()
        {
            var dictionary = UTF8.GetBytes("documentesthis");
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            var doc = XDocument.Parse(docString);
            var uncompressedBytes = UTF8.GetBytes(docString).Length;
            var withoutDictionary = XmlCompression.SaveUsingDeflateWithDictionary(doc, null).Length;
            var withDictionary = XmlCompression.SaveUsingDeflateWithDictionary(doc, dictionary).Length;
            PAssert.That(() => withDictionary < withoutDictionary && withoutDictionary < uncompressedBytes);
        }

        [Test]
        public void SaveUsingDeflateWithDictionaryIsSmallerInRealExample()
        {
            var xmlStringWithFormatting = new ServiceTestBerichten().Bericht(BerichtType.vchmsg03inschrijving);
            var docString = XDocument.Parse(xmlStringWithFormatting).ToString(SaveOptions.DisableFormatting);

            var dictionary = UTF8.GetBytes("<typename><tysource sourcetype=\"imsdefault\" /><tyvalue>" + BerichtType.vchmsg03inschrijving + $" xmlns=\"{SchemaSet.KVA5_NS.NamespaceName}\"");
            var doc = XDocument.Parse(docString);
            var uncompressedBytes = UTF8.GetBytes(docString).Length;
            var withoutDictionary = XmlCompression.SaveUsingDeflateWithDictionary(doc, null).Length;
            var withDictionary = XmlCompression.SaveUsingDeflateWithDictionary(doc, dictionary).Length;
            PAssert.That(() => withDictionary < withoutDictionary && withoutDictionary < uncompressedBytes);
        }
    }
}
