using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ProgressOnderwijsUtils
{
	/// <summary>
	/// Validates XHTML as XHTML 1.0 Transitional
	/// </summary>
	public sealed class XhtmlValidator
	{
		// XHTML validation requires namespaces to be set,
		// but doing so forces you to specify namespaces in Linq-to-XML queries (which is unwieldy),
		// so we just do it here temporarily.
		static readonly XNamespace NAMESPACE = SchemaSet.XHTML_NS;

		XDocument NamespacedCopy(XDocument document)
		{
			var copy = new XDocument(document);

			foreach (var element in copy.Descendants())
				element.Name = NAMESPACE + element.Name.LocalName;

			return copy;
		}

		XElement NamespacedCopy(XElement xhtml)
		{
			var copy = new XElement(xhtml);

			foreach (var element in copy.DescendantsAndSelf())
				element.Name = NAMESPACE + element.Name.LocalName;

			return copy;
		}

		public void Validate(XDocument document, ValidationEventHandler handler = null)
		{
			XDocument copy = NamespacedCopy(document);
			copy.Validate(handler);
		}

		public void Validate(XElement element, ValidationEventHandler handler = null)
		{
			XElement copy = NamespacedCopy(element);

			var elementName = new XmlQualifiedName(copy.Name.LocalName, copy.Name.NamespaceName);
			copy.Validate(SchemaSet.GetPartialValidationType(elementName), handler);
		}

	}
}
