using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToStringExtensions
    {
        static readonly XslCompiledTransform toHtmlSyntax = MakeXhtmlToHtml();

        static XslCompiledTransform MakeXhtmlToHtml()
        {
            var transform = new XslCompiledTransform(false);
            var simpleTransform = @"<?xml version='1.0' encoding='UTF-8'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
  <xsl:output method='html' omit-xml-declaration='yes' indent='no'/>
  <xsl:template match='/html'>
    <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html&gt;</xsl:text>
    <xsl:copy-of select='.'/>
  </xsl:template>
</xsl:stylesheet>";
            using (var stringReader = new StringReader(simpleTransform))
            using (var xmlReader = XmlReader.Create(stringReader))
                transform.Load(xmlReader);
            return transform;
        }

        public static void SaveHtmlFragmentToStream(HtmlFragment rootElem, Stream outputStream, Encoding contentEncoding)
        {
            var xhtmlDoc = (XElement)rootElem.ToXDocumentFragment();
            var outputSettings = CreateXmlWriterSettings(contentEncoding);
            using (var xmlWriter = XmlWriter.Create(outputStream, outputSettings))
                toHtmlSyntax.Transform(xhtmlDoc.CreateReader(), null, xmlWriter);
        }

        static XmlWriterSettings CreateXmlWriterSettings(Encoding contentEncoding)
        {
            var outputSettings = toHtmlSyntax.OutputSettings.Clone();
            outputSettings.CheckCharacters = false;
            outputSettings.NewLineChars = "\n";
            outputSettings.Encoding = contentEncoding;
            return outputSettings;
        }

        public static string SerializeToString(this HtmlFragment rootElem)
        {
            var xhtmlDoc = (XElement)rootElem.ToXDocumentFragment();
            var outputSettings = CreateXmlWriterSettings(Encoding.UTF8);
            using (var stringWriter = new StringWriter()) {
                using (var xmlWriter = XmlWriter.Create(stringWriter, outputSettings))
                    toHtmlSyntax.Transform(xhtmlDoc.CreateReader(), null, xmlWriter);
                return stringWriter.ToString().Replace(@"<META http-equiv=""Content-Type"" content=""text/html; charset=utf-16"">","");
            }
        }
    }
}