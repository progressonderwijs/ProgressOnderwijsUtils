using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
	public struct XHtmlData
	{
		readonly XNode[] nodes;

		public static XHtmlData Create(params object[] contents)
		{
			return new XHtmlData(new XElement("x", contents).Nodes().ToArray());
		}

		public static XHtmlData Parse(string s)
		{
			return new XHtmlData(XhtmlCleaner.HtmlSanitizer(s).Nodes().ToArray());
		}

		XHtmlData(XNode[] nodes) { this.nodes = nodes; }

		public string ToUiString()
		{
			using (StringWriter writer = new StringWriter())
			using (XmlWriter inner = XmlWriter.Create(writer, new XmlWriterSettings {
				ConformanceLevel = ConformanceLevel.Fragment,
			}))
			{
				foreach (var node in nodes.EmptyIfNull())
					node.WriteTo(inner);
				inner.Close();
				return writer.ToString();
			}
		}
	}
}
