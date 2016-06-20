using System.Linq;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
    public static class XmlCompression
    {
        /// <summary>
        /// Removes unused namespaces and makes sure the XMLSchema and XMLSchema-instance namespaces are mapped to the conventional prefix.
        /// </summary>
        public static void CleanupNamespaces(XDocument doc)
        {
            var usedNamespaces = doc.Descendants().Select(el => el.Name.Namespace).ToSet();
            usedNamespaces.UnionWith(doc.Descendants().Attributes().Select(attr => attr.Name.Namespace));

            var unusedRootNamespaceDeclarations = doc.Descendants().Attributes()
                .Where(attr => attr.IsNamespaceDeclaration && !usedNamespaces.Contains(attr.Value))
                .ToArray();

            foreach (var unusedNamespaceDeclaration in unusedRootNamespaceDeclarations)
                unusedNamespaceDeclaration.Remove();

            if (usedNamespaces.Contains(xsdNamespace)) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == xsdNamespace.NamespaceName))
                    attr.Remove();
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsd", xsdNamespace.NamespaceName);
            }
            if (usedNamespaces.Contains(xsiNamespace)) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == xsiNamespace.NamespaceName))
                    attr.Remove();
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsi", xsiNamespace.NamespaceName);
            }
        }

        static readonly XNamespace xsdNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
        static readonly XNamespace xsiNamespace = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
    }
}
