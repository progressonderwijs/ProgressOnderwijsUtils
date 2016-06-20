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
    }
}
