using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
		const string SCHEMA_LOCATION = "http://www.w3.org/2002/08/xhtml/xhtml1-transitional.xsd";

		static readonly Lazy<byte[]> SCHEMA_DATA = new Lazy<byte[]>(delegate {

			using (var web = new WebClient())
			{
				return web.DownloadData(SCHEMA_LOCATION);
			}

		});

		readonly XmlSchemaSet schemas = new XmlSchemaSet();

		readonly XmlSchema schema;

		public XhtmlValidator()
		{
			using (var stream = new MemoryStream(SCHEMA_DATA.Value, writable: false))
			{
				schema = XmlSchema.Read(stream, null);
				schemas.Add(schema);
			}
		}

		// XHTML validation requires namespaces to be set,
		// but doing so forces you to specify namespaces in Linq-to-XML queries (which is unwieldy),
		// so we just do it here temporarily.
		static readonly XNamespace NAMESPACE = "http://www.w3.org/1999/xhtml";

		XDocument NamespacedCopy(XDocument document)
		{
			XDocument copy = new XDocument(document);

			foreach (var element in copy.Descendants())
			{
				element.Name = NAMESPACE + element.Name.LocalName;
			}

			return copy;
		}

		XElement NamespacedCopy(XElement xhtml)
		{
			XElement copy = new XElement(xhtml);

			foreach (var element in copy.DescendantsAndSelf())
			{
				element.Name = NAMESPACE + element.Name.LocalName;
			}

			return copy;
		}

		public void Validate(XDocument document, ValidationEventHandler handler = null)
		{
			XDocument copy = NamespacedCopy(document);
			copy.Validate(schemas, handler);
		}

		public void Validate(XElement element, ValidationEventHandler handler = null)
		{
			XElement copy = NamespacedCopy(element);

			var elementName = new XmlQualifiedName(copy.Name.LocalName, copy.Name.NamespaceName);
			copy.Validate(schema.Elements[elementName], schemas, handler);
		}

	}
}
