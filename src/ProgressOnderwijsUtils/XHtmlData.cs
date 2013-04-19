using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ProgressOnderwijsUtils
{
	public struct ToolTipSettings
	{
		public string Header;
		public int? Width;
	}

	public struct XhtmlData : IEnumerable<XNode>
	{
		readonly XNode[] nodes;
		public IEnumerable<XNode> Nodes { get { return nodes.EmptyIfNull(); } }

		public static XhtmlData Empty { get { return default(XhtmlData); } }

		public static XhtmlData Create(IEnumerable<XNode> nodes)
		{
			return new XhtmlData(nodes.ToArray());
		}

		public static XhtmlData Create(params object[] contents)
		{
			return Create(new XElement("x", contents).Nodes());
		}

		public static XhtmlData ParseAndSanitize(string s)
		{
			return XhtmlCleaner.HeuristicParse(s).Sanitize();
		}

		public static XhtmlData? TryParseAndSanitize(string s)
		{
			var xml = XhtmlCleaner.TryParse(s);
			if (xml == null)
				return null;

			return XhtmlCleaner.Sanitize(xml.Value);
		}


		public static XhtmlData GenerateToolTipHtmlFragment(IEnumerable<XNode> text, IEnumerable<XNode> tooltip, ToolTipSettings settings)
		{
			return
				Create(
				tooltip != null && tooltip.Any() ?
														new XNode[]{ 	
					                                        new XElement("span", new XAttribute("class","hastooltip"),
																settings.Width.HasValue? new XAttribute("data-tipwidth",settings.Width) : null,
						                                        new XElement("div", new XAttribute("class","tipContent"),
																	string.IsNullOrWhiteSpace(settings.Header) ? null : new XElement("div", new XAttribute("class", "header"), settings.Header),
							                                        new XElement("div", new XAttribute("class", "tipbody"),tooltip)
																),
						                                        text
															)
				                                        }
				: text);
		}

		XhtmlData(XNode[] nodes) { this.nodes = nodes; }

		public override string ToString()
		{
			return Nodes.Select(x => x.ToString(SaveOptions.DisableFormatting)).JoinStrings();
		}

		public string ToUiString()
		{
			using (StringWriter writer = new StringWriter())
			using (XmlWriter inner = XmlWriter.Create(writer, new XmlWriterSettings {
				ConformanceLevel = ConformanceLevel.Fragment,
			}))
			{
				foreach (var node in Nodes)
					node.WriteTo(inner);
				inner.Close();
				return writer.ToString();
			}
		}

		public XElement ToXHtmlDataElement()
		{
			// ReSharper disable CoVariantArrayConversion
			return new XElement("XHtmlData", nodes);
			// ReSharper restore CoVariantArrayConversion
		}

		public IEnumerator<XNode> GetEnumerator() { return Nodes.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
