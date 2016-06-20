using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //*
            if (usedNamespaces.Contains(XNamespace.Get("http://www.w3.org/2001/XMLSchema"))) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == "http://www.w3.org/2001/XMLSchema"))
                    attr.Remove();
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema");
            }
            if (usedNamespaces.Contains(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance"))) {
                foreach (var attr in doc.Root.Attributes().Where(attr => attr.IsNamespaceDeclaration && attr.Value == "http://www.w3.org/2001/XMLSchema-instance"))
                    attr.Remove();
                doc.Root.SetAttributeValue(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance");
            }
        }
    }
}
