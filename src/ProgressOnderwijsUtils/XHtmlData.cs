using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
	public struct XHtmlData
	{
		public XNode[] nodes;

		public static XHtmlData Create(params object[] contents)
		{
			return new XHtmlData
			{
				nodes = new XElement("x", contents).Nodes().ToArray()
			};
		}

		public static XHtmlData Parse(string s)
		{
			return new XHtmlData
			{
				nodes = HtmlTidyWrapper.HtmlSanitizer(s).Nodes().ToArray(),
			};
		}

		public string ToUiString()
		{
			using (StringWriter writer = new StringWriter())
			using (XmlWriter inner = XmlWriter.Create(writer, new XmlWriterSettings
				{
					ConformanceLevel = ConformanceLevel.Fragment,
				}))
			{
				foreach (var node in nodes.EmptyIfNull())
				{
					node.WriteTo(inner);
				}
				inner.Close();
				return writer.ToString();
			}
		}
	}
}
