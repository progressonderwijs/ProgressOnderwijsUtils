using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlAttribute
    {
        public string Name, Value;
    }

    public interface IHtmlTagName
    {
        string TagName { get; }
    }

    static class HtmlAttributeHelpers
    {
        public static readonly HtmlAttribute[] EmptyAttributes = new HtmlAttribute[0];

        public static HtmlAttribute[] appendAttr(this HtmlAttribute[] attributes, string attrName, string attrValue)
        {
            //performance assumption: the list of attributes is short.
            var retval = new HtmlAttribute[attributes.Length + 1];
            for (int i = 0; i < attributes.Length; i++) {
                retval[i] = attributes[i];
            }
            retval[attributes.Length] = new HtmlAttribute { Name = attrName, Value = attrValue };
            return retval;
        }

        public static IEnumerable<XAttribute> ToXAttributes(this HtmlAttribute[] htmlAttributes)
        {
            return htmlAttributes.Select(htmlAttr => new XAttribute(htmlAttr.Name, htmlAttr.Value));
        }
    }

    public struct HtmlElementOrTextContent
    {
        //This is a union type of EITHER a text content node OR an html element node.
        //When attributes == null: this  is a text content node
        //When attributes != null: this is an html element node
        readonly string tagNameOrTextContent;
        readonly HtmlAttribute[] attributesWhenTag;
        readonly HtmlElementOrTextContent[] childNodes;
        readonly XElement embeddedContent;

        public HtmlElementOrTextContent(string tagNameOrTextContent, HtmlAttribute[] attributesWhenTag, HtmlElementOrTextContent[] childNodes, XElement embeddedContent)
        {
            this.tagNameOrTextContent = tagNameOrTextContent;
            this.attributesWhenTag = attributesWhenTag;
            this.childNodes = childNodes;
            this.embeddedContent = embeddedContent;
        }

        public static HtmlElementOrTextContent TextContent(string textContent) => new HtmlElementOrTextContent(textContent, null, null, null);

        public static HtmlElementOrTextContent HtmlElement(HtmlElement element)
            => new HtmlElementOrTextContent(element.TagName, element.Attributes ?? HtmlAttributeHelpers.EmptyAttributes, element.ChildNodes, null);

        public static HtmlElementOrTextContent HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlElementOrTextContent[] childNodes)
            => new HtmlElementOrTextContent(tagName, attributes ?? HtmlAttributeHelpers.EmptyAttributes, childNodes, null);

        public static HtmlElementOrTextContent XmlElement(XElement xmlElement)
            => new HtmlElementOrTextContent(null, null, null, xmlElement);

        public static implicit operator HtmlElementOrTextContent(HtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlElementOrTextContent(string textContent) => TextContent(textContent);
        public bool IsTextContent => embeddedContent == null && attributesWhenTag == null;
        public bool IsHtmlElement => embeddedContent == null && attributesWhenTag != null;
        public bool IsXmlElement => embeddedContent != null;

        public string GetTextContent()
        {
            if (!IsTextContent) {
                throw new InvalidOperationException("Cannot convert html element node into text content node");
            }
            return tagNameOrTextContent;
        }

        public HtmlElement GetHtmlElement()
        {
            if (!IsHtmlElement) {
                throw new InvalidOperationException("Cannot convert text content node into html element node");
            }
            return new HtmlElement(tagNameOrTextContent, attributesWhenTag, childNodes);
        }

        public XNode ToXNode()
        {
            if (IsXmlElement) {
                return embeddedContent;
            }
            if (IsTextContent) {
                return new XText(tagNameOrTextContent);
            }
            Debug.Assert(IsHtmlElement);
            return new HtmlElement(tagNameOrTextContent, attributesWhenTag, childNodes).ToXElement();
        }
    }

    public struct HtmlElement
    {
        public readonly string TagName;
        public readonly HtmlAttribute[] Attributes;
        public readonly HtmlElementOrTextContent[] ChildNodes;

        public HtmlElement(string tagName, [NotNull] HtmlAttribute[] attributes, HtmlElementOrTextContent[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            ChildNodes = childNodes;
        }

        public XElement ToXElement() => new XElement(
            TagName,
            Attributes.ToXAttributes(),
            ChildNodes?.Select(childNode => childNode.ToXNode())
            );
    }

    public interface IFluentHtmlTagExpression<out TExpression>
        where TExpression : IFluentHtmlTagExpression<TExpression>
    {
        TExpression withAttribute(string attrName, string attrValue);
    }

    public static class HtmlTagHelpers
    {
        /// <summary>Creates an html data attribute.  E.g. setDataAttribute("foo", "bar") creates data-foo="bar". </summary>
        public static TExpression withDataAttribute<TExpression>(this TExpression htmlTagExpr, string dataAttrName, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.withAttribute("data-" + dataAttrName, attrValue);

        /// <summary>Creates an html data attribute.  E.g. setDataAttribute("foo", "bar") creates data-foo="bar". </summary>
        public static TExpression withAttributes<TExpression>(this TExpression htmlTagExpr, IEnumerable<HtmlAttribute> attributes)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.withAttribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        public static HtmlElementOrTextContent ToHtmlContent(this XElement xEl) => HtmlElementOrTextContent.XmlElement(xEl);
    }

    interface IHtmlStartTag
    {
        HtmlAttribute[] Attributes { get; }
        string TagName { get; }
    }

    public struct HtmlStartTag<TNamedTagType>
        : IHtmlStartTag, IFluentHtmlTagExpression<HtmlStartTag<TNamedTagType>>
        where TNamedTagType : struct, IHtmlTagName
    {
        public string TagName => default(TNamedTagType).TagName;
        public HtmlAttribute[] Attributes { get; }
        internal bool HasValue => Attributes != null;

        HtmlStartTag(HtmlAttribute[] attributes)
        {
            Attributes = attributes;
        }

        public HtmlStartTag<TNamedTagType> withAttribute(string attrName, string attrValue)
            => new HtmlStartTag<TNamedTagType>((Attributes ?? HtmlAttributeHelpers.EmptyAttributes).appendAttr(attrName, attrValue));

        public HtmlElement AddContent() => AddContent(null);
        public HtmlElement AddContent(params HtmlElementOrTextContent[] content) => new HtmlElement(TagName, Attributes, content);
        public HtmlElement this[params HtmlElementOrTextContent[] content] => new HtmlElement(TagName, Attributes, content);
        public static implicit operator HtmlElementOrTextContent(HtmlStartTag<TNamedTagType> startTag) => HtmlElementOrTextContent.HtmlElement(startTag.TagName, startTag.Attributes, null);
    }

    public struct GeneralStartTag
        : IHtmlStartTag
            , IFluentHtmlTagExpression<GeneralStartTag>
    {
        public string TagName { get; }
        public HtmlAttribute[] Attributes { get; }
        internal bool HasValue => Attributes != null;

        public GeneralStartTag(string tagName)
            : this(tagName, HtmlAttributeHelpers.EmptyAttributes) { }

        GeneralStartTag(string tagName, HtmlAttribute[] attributes)
        {
            TagName = tagName;
            Attributes = attributes ?? HtmlAttributeHelpers.EmptyAttributes;
        }

        public GeneralStartTag withAttribute(string attrName, string attrValue)
            => new GeneralStartTag(TagName, Attributes.appendAttr(attrName, attrValue));

        public HtmlElement AddContent() => AddContent(null);
        public HtmlElement AddContent(params HtmlElementOrTextContent[] content) => new HtmlElement(TagName, Attributes, content);
        public HtmlElement this[params HtmlElementOrTextContent[] content] => new HtmlElement(TagName, Attributes, content);
        public static implicit operator HtmlElementOrTextContent(GeneralStartTag startTag) => HtmlElementOrTextContent.HtmlElement(startTag.TagName, startTag.Attributes, null);
    }
}
