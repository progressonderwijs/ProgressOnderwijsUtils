using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ZlibWithDictionary;

namespace ProgressOnderwijsUtils
{
    public static class XmlCompression
    {
        /// <summary>
        /// Removes unused namespaces and makes sure the XMLSchema and XMLSchema-instance namespaces are mapped to the conventional prefix.
        /// </summary>
        public static void CleanupNamespaces(XDocument doc)
        {
            if (doc.Root == null) {
                throw new InvalidOperationException("empty documents not supported");
            }
            var usedNamespaces = doc.Descendants().Select(el => el.Name.Namespace).ToHashSet();
            usedNamespaces.UnionWith(doc.Descendants().Attributes().Select(attr => attr.Name.Namespace));

            var unusedRootNamespaceDeclarations = doc.Descendants().Attributes()
                .Where(attr => attr.IsNamespaceDeclaration && !usedNamespaces.Contains(attr.Value))
                .ToArray();

            foreach (var unusedNamespaceDeclaration in unusedRootNamespaceDeclarations) {
                unusedNamespaceDeclaration.Remove();
            }

            if (usedNamespaces.Contains(xsdNamespace)) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == xsdNamespace.NamespaceName)) {
                    attr.Remove();
                }
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsd", xsdNamespace.NamespaceName);
            }
            if (usedNamespaces.Contains(xsiNamespace)) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == xsiNamespace.NamespaceName)) {
                    attr.Remove();
                }
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsi", xsiNamespace.NamespaceName);
            }
        }

        public static void RemoveComments(XDocument doc)
        {
            foreach (var comment in doc.DescendantNodes().OfType<XComment>().ToArray()) {
                comment.Remove();
            }
        }

        static readonly XNamespace xsdNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
        static readonly XNamespace xsiNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

        static readonly XmlWriterSettings xmlWriterSettings = new XmlWriterSettings {
            Encoding = Encoding.UTF8,
            Indent = false,
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            OmitXmlDeclaration = true,
        };

        public static byte[] ToUtf8(XDocument doc)
        {
            var sb = new StringBuilder();
            using (var xw = XmlWriter.Create(sb, xmlWriterSettings)) {
                doc.Save(xw);
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public static XDocument FromUtf8(byte[] utf8EncodedXml)
            => XDocument.Parse(Encoding.UTF8.GetString(utf8EncodedXml));

        /// <summary>
        /// Saves the xml document.  The document is minified (redundant namespaces and indenting omitted), serialized to utf8, and then zlib compressed with an (optional) dictionary.
        /// You must provide this identical dictionary to be able to decompress the document.
        /// </summary>
        /// <param name="doc">The document to compress.</param>
        /// <param name="dictionary">
        /// The dictionary to use during compression.
        /// A good dictionary shared as many substrings that are as long as possible with the input data (e.g. document with the same schema).
        /// A null dictionary is permitted, which means "compress without dictionary".
        /// </param>
        /// <returns>The deflate-compressed document.</returns>
        public static byte[] ToCompressedUtf8(XDocument doc, byte[]? dictionary)
        {
            CleanupNamespaces(doc);
            RemoveComments(doc);
            var uncompressedBytes = ToUtf8(doc);
            var compressedBytes = DeflateCompression.ZlibCompressWithDictionary(uncompressedBytes, dictionary, Ionic.Zlib.CompressionLevel.BestCompression);
            return compressedBytes;
        }

        /// <summary>
        /// Loads an XDocument that was saved with 'SaveUsingDeflateWithDictionary'.  You must provide the same dictionary used during compression.
        /// </summary>
        public static XDocument FromCompressedUtf8(byte[] compressedBytes, byte[]? dictionary)
        {
            var bytes = DeflateCompression.ZlibDecompressWithDictionary(compressedBytes, dictionary);
            var xmlString = Encoding.UTF8.GetString(bytes);
            return XDocument.Parse(xmlString);
        }
    }
}
