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

        public HtmlAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
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

    public struct HtmlFragment
    {
        //This is a union type of EITHER a text content node OR an html element node.
        //When attributes == null: this  is a text content node
        //When attributes != null: this is an html element node
        readonly string tagNameOrTextContent;
        readonly HtmlAttribute[] attributesWhenTag;
        readonly HtmlFragment[] childNodes;
        readonly XElement embeddedContent;

        public HtmlFragment(string tagNameOrTextContent, HtmlAttribute[] attributesWhenTag, HtmlFragment[] childNodes, XElement embeddedContent)
        {
            this.tagNameOrTextContent = tagNameOrTextContent;
            this.attributesWhenTag = attributesWhenTag;
            this.childNodes = childNodes;
            this.embeddedContent = embeddedContent;
        }

        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent, null, null, null);

        public static HtmlFragment HtmlElement(HtmlElement element)
            => new HtmlFragment(element.TagName, element.Attributes ?? HtmlAttributeHelpers.EmptyAttributes, element.ChildNodes, null);

        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            => new HtmlFragment(tagName, attributes ?? HtmlAttributeHelpers.EmptyAttributes, childNodes, null);

        public static HtmlFragment XmlElement(XElement xmlElement)
            => new HtmlFragment(null, null, null, xmlElement);

        public static implicit operator HtmlFragment(HtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);
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
        public readonly HtmlFragment[] ChildNodes;

        public HtmlElement(string tagName, [NotNull] HtmlAttribute[] attributes, HtmlFragment[] childNodes)
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
        [Pure]
        TExpression Attribute(string attrName, string attrValue);

        [Pure]
        HtmlElement Content(params HtmlFragment[] content);
    }

    public static class HtmlTagHelpers
    {
        /// <summary>Creates an html data attribute.  E.g. setDataAttribute("foo", "bar") creates data-foo="bar". </summary>
        public static TExpression DataAttribute<TExpression>(this TExpression htmlTagExpr, string dataAttrName, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute("data-" + dataAttrName, attrValue);

        public static TExpression Attributes<TExpression>(this TExpression htmlTagExpr, IEnumerable<HtmlAttribute> attributes)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.Attribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute attribute)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute? attributeOrNull)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        public static HtmlFragment WrapInHtmlFragment(this XElement xEl) => HtmlFragment.XmlElement(xEl);
    }

    public struct HtmlStartTag<TNamedTagType>
        : IFluentHtmlTagExpression<HtmlStartTag<TNamedTagType>>
        where TNamedTagType : struct, IHtmlTagName
    {
        public string TagName => default(TNamedTagType).TagName;
        public HtmlAttribute[] Attributes { get; }

        HtmlStartTag(HtmlAttribute[] attributes)
        {
            Attributes = attributes;
        }

        [Pure]
        public HtmlStartTag<TNamedTagType> Attribute(string attrName, string attrValue)
            => attrValue == null ? this : new HtmlStartTag<TNamedTagType>((Attributes ?? HtmlAttributeHelpers.EmptyAttributes).appendAttr(attrName, attrValue));

        [Pure]
        public HtmlElement Content(params HtmlFragment[] content) => new HtmlElement(TagName, Attributes, content);

        [Pure]
        public HtmlElement Content() => Content(default(HtmlFragment[]));

        public static implicit operator HtmlFragment(HtmlStartTag<TNamedTagType> startTag) => HtmlFragment.HtmlElement(startTag.TagName, startTag.Attributes, null);

        [Pure]
        public HtmlElement Content(IEnumerable<HtmlElement> menuItemLiElements) => Content(menuItemLiElements.Select(el => (HtmlFragment)el).ToArray());
    }
}
