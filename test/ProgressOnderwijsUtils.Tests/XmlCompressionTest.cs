using System.Text;
using System.Xml.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Tests
{
    public sealed class XmlCompressionTest
    {
        [Fact]
        public void TrivialDocIsNotChanged()
        {
            var doc = XDocument.Parse("<test/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test />");
        }

        [Fact]
        public void DocumentWithUnusedRootNamespaceHasNamespaceRemoved()
        {
            var doc = XDocument.Parse("<test xmlns:bla='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test />");
        }

        [Fact]
        public void DocumentWithDefaultNsIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\" />");
        }

        [Fact]
        public void XmlDeclarationHasNoImpact()
        {
            var doc = XDocument.Parse("<?xml version=\"1.0\"?><test xmlns='bla'/>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\" />");
        }

        [Fact]
        public void DocumentWithChildNamespaceIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns='bla'><hmm:this xmlns:hmm='foo'/></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns=\"bla\"><hmm:this xmlns:hmm=\"foo\" /></test>");
        }

        [Fact]
        public void DocumentWithNamespaceUsedInDescendantIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns:hmm='bla'><hmm:this /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><hmm:this /></test>");
        }

        [Fact]
        public void DocumentWithNamespaceUsedInDescendantAttributeIsUnchanged()
        {
            var doc = XDocument.Parse("<test xmlns:hmm='bla'><this><that hmm:id='' /></this></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:hmm=\"bla\"><this><that hmm:id=\"\" /></this></test>");
        }

        [Fact]
        public void UnusualAliasesAreNormalized()
        {
            var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsd:foo='' notxsi:nil='true' /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsd:foo=\"\" xsi:nil=\"true\" /></test>");
        }

        [Fact]
        public void UnusedAreRemoved()
        {
            var doc = XDocument.Parse("<test xmlns:notxsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:notxsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this notxsi:nil='true' /></test>");
            XmlCompression.CleanupNamespaces(doc);
            var output = doc.ToString(SaveOptions.DisableFormatting);
            PAssert.That(() => output == "<test xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><this xsi:nil=\"true\" /></test>");
        }

        static readonly Encoding UTF8 = Encoding.UTF8;

        [Fact]
        public void SaveToUtf8ContainsNoByteOrderMark()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.ToUtf8(doc);
            PAssert.That(() => bytes[0] == (byte)'<');
        }

        [Fact]
        public void SaveToUtf8IsUtf8()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.ToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Fact]
        public void SaveToUtf8CanRoundTrip()
        {
            var doc = XDocument.Parse("<test>Ƒϕϕ</test>");
            var bytes = XmlCompression.ToUtf8(doc);
            var reloaded = XmlCompression.FromUtf8(bytes);

            var str = reloaded.ToString();

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Fact]
        public void SaveToUtf8ExcludesXmlDeclaration()
        {
            var doc = XDocument.Parse("<?xml version=\"1.0\" encoding=\"utf16\"?><test>Ƒϕϕ</test>");

            var bytes = XmlCompression.ToUtf8(doc);
            var str = UTF8.GetString(bytes);

            PAssert.That(() => str == "<test>Ƒϕϕ</test>");
        }

        [Fact]
        public void SaveToUtf8ExcludesIndentation()
        {
            var doc = XDocument.Parse(
                @"<test>
    <nested>
        <elements>
            <here>
                Ƒϕϕ
            </here>
        </elements>
    </nested>
</test>"
            );

            var utf8BytesFromXml = XmlCompression.ToUtf8(doc);
            var stringFromBytes = UTF8.GetString(utf8BytesFromXml);

            //XDocument.Parse/Serialize appears to sometimes lose CR's
            //The behavior differs at least between net462 on windows and netcoreapp20 on linux
            PAssert.That(
                () => stringFromBytes.Replace("\r", "") == @"<test><nested><elements><here>
                Ƒϕϕ
            </here></elements></nested></test>".Replace("\r", "")
            );
        }

        [Fact]
        public void SaveUsingDeflateWithDictionaryWithoutDictionaryRoundTrips()
        {
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            AssertCompressionCompressesAndRoundTrips(docString, null);
        }

        static void AssertCompressionCompressesAndRoundTrips(string docString, byte[]? dictionary)
        {
            var doc = XDocument.Parse(docString);
            var compressedBytes = XmlCompression.ToCompressedUtf8(doc, dictionary);
            PAssert.That(() => compressedBytes.Length < UTF8.GetByteCount(docString));
            var decompressedDoc = XmlCompression.FromCompressedUtf8(compressedBytes, dictionary);
            PAssert.That(() => !ReferenceEquals(doc, decompressedDoc), "Using the same reference would be cheating!");
            PAssert.That(() => XNode.DeepEquals(doc, decompressedDoc));
            PAssert.That(() => decompressedDoc.ToString(SaveOptions.DisableFormatting) == docString);
        }

        [Fact]
        public void SaveUsingDeflateWithDictionaryRoundTrips()
        {
            var dictionary = UTF8.GetBytes("documentesthis");
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            AssertCompressionCompressesAndRoundTrips(docString, dictionary);
        }

        [Fact]
        public void SaveUsingDeflateWithDictionaryIsSmaller()
        {
            var dictionary = UTF8.GetBytes("documentesthis");
            var docString = "<test><this><document xmlns=\"bla\">Ƒоо</document></this></test>";
            var doc = XDocument.Parse(docString);
            var uncompressedBytes = UTF8.GetBytes(docString).Length;
            var withoutDictionary = XmlCompression.ToCompressedUtf8(doc, null).Length;
            var withDictionary = XmlCompression.ToCompressedUtf8(doc, dictionary).Length;
            PAssert.That(() => withDictionary < withoutDictionary && withoutDictionary < uncompressedBytes);
        }
    }
}
