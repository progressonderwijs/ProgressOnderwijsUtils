using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ValueUtils;

namespace ProgressOnderwijsUtils
{
    public struct ToolTipSettings
    {
        public string Header;
        public int? Width;
    }

    public struct XhtmlData : IEnumerable<XNode>, IEquatable<XhtmlData>
    {
        readonly XNode[] nodes;
        public IEnumerable<XNode> Nodes => nodes.EmptyIfNull();
        public bool IsEmpty => nodes == null || nodes.Length == 0;
        public static XhtmlData Empty => default(XhtmlData);
        public static XhtmlData Create(IEnumerable<XNode> nodes) => new XhtmlData(nodes.ToArray());
        public static XhtmlData Create(params object[] contents) => Create(new XElement("x", contents).Nodes());
        public static XhtmlData ParseAndSanitize(string s) => XhtmlCleaner.HeuristicParse(s).Sanitize();

        public static XhtmlData? TryParseAndSanitize(string s)
        {
            var xml = XhtmlCleaner.TryParse(s);
            if (xml == null) {
                return null;
            }

            return xml.Value.Sanitize();
        }

        public static XhtmlData GenerateToolTipHtmlFragment(IEnumerable<XNode> text, IEnumerable<XNode> tooltip, ToolTipSettings settings)
        {
            //TODO:HTML-tooltip
            return
                Create(
                    tooltip != null && tooltip.Any()
                        ? new XNode[] {
                            new XElement(
                                "span",
                                new XAttribute("class", "hastooltip"),
                                settings.Width.HasValue ? new XAttribute("data-tipwidth", settings.Width) : null,
                                new XElement(
                                    "div",
                                    new XAttribute("class", "tipContent"),
                                    string.IsNullOrWhiteSpace(settings.Header) ? null : new XElement("div", new XAttribute("class", "header"), settings.Header),
                                    new XElement("div", new XAttribute("class", "tipbody"), tooltip)
                                    ),
                                text
                                )
                        }
                        : text);
        }

        XhtmlData(XNode[] nodes)
        {
            this.nodes = nodes;
        }

        public bool Equals(XhtmlData other) => FieldwiseEquality.AreEqual(this, other);
        public override bool Equals(object obj) => obj is XhtmlData && Equals((XhtmlData)obj);
        public override int GetHashCode() => FieldwiseHasher.Hash(this);
        public override string ToString() => Nodes.Select(x => x.ToString(SaveOptions.DisableFormatting)).JoinStrings();

        public string ToUiString()
        {
            using (var writer = new StringWriter())
            using (var inner = XmlWriter.Create(
                writer,
                new XmlWriterSettings {
                    ConformanceLevel = ConformanceLevel.Fragment,
                })) {
                foreach (var node in Nodes) {
                    node.WriteTo(inner);
                }
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

        public string TextContent => ToXHtmlDataElement().Value;
        public IEnumerator<XNode> GetEnumerator() => Nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
