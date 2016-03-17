﻿using System;
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
        readonly string tagNameOrTextContent; //iff text or element node
        readonly HtmlAttribute[] attributesWhenTag; // iff elementnode
        readonly HtmlFragment[] childNodes; //only if element node or collection; null means "empty".
        readonly XElement embeddedContent; //iff xml node

        //This is a union type of...
        // - An xml node
        //      (WITH embeddedContent, without tagNameOrTextContent, without attributesWhenTag, without childNodes)
        public bool IsXmlElement => embeddedContent != null && tagNameOrTextContent == null && attributesWhenTag == null && childNodes == null;
        // - A text content node:
        //      (without embeddedContent, WITH tagNameOrTextContent, without attributesWhenTag, without childNodes)
        public bool IsTextContent => tagNameOrTextContent != null && attributesWhenTag == null && embeddedContent == null && childNodes == null;
        // - A single element node    
        //      (without embeddedContent, WITH tagNameOrTextContent, WITH attributesWhenTag, ? childNodes)
        public bool IsHtmlElement => attributesWhenTag != null && embeddedContent == null && tagNameOrTextContent != null;
        // - A collection of fragments
        //      (without embeddedContent, without tagNameOrTextContent, without attributesWhenTag, ? childNodes)
        public bool IsCollectionOfFragments => embeddedContent == null && tagNameOrTextContent == null && attributesWhenTag == null;

        public bool IsEmpty => attributesWhenTag == null && embeddedContent == null && childNodes == null && string.IsNullOrEmpty(tagNameOrTextContent);

        public HtmlFragment(string tagNameOrTextContent, HtmlAttribute[] attributesWhenTag, HtmlFragment[] childNodes, XElement embeddedContent)
        {
            this.tagNameOrTextContent = tagNameOrTextContent;
            this.attributesWhenTag = attributesWhenTag;
            this.childNodes = childNodes;
            this.embeddedContent = embeddedContent;
            Debug.Assert((IsXmlElement ? 1 : 0) + (IsTextContent ? 1 : 0) + (IsHtmlElement ? 1 : 0) + (IsCollectionOfFragments ? 1 : 0) == 1);
        }

        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent, null, null, null);

        public static HtmlFragment HtmlElement(HtmlElement element)
            => new HtmlFragment(element.TagName, element.Attributes ?? HtmlAttributeHelpers.EmptyAttributes, element.ChildNodes, null);

        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            => new HtmlFragment(tagName, attributes ?? HtmlAttributeHelpers.EmptyAttributes, childNodes, null);

        public static HtmlFragment XmlElement(XElement xmlElement)
            => new HtmlFragment(null, null, null, xmlElement);

        public static HtmlFragment HtmlElements(HtmlFragment[] htmlEls)
            => new HtmlFragment(null, null, htmlEls, null);

        public static implicit operator HtmlFragment(HtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);

        [Pure]
        public object ToXDocumentFragment()
        {
            if (embeddedContent != null) {
                Debug.Assert(IsXmlElement);
                return embeddedContent;
            } else if (tagNameOrTextContent == null) {
                Debug.Assert(IsCollectionOfFragments);
                return childNodes?.ArraySelect(node => node.ToXDocumentFragment());
            } else if (attributesWhenTag == null) {
                Debug.Assert(IsTextContent);
                return new XText(tagNameOrTextContent);
            } else {
                Debug.Assert(IsHtmlElement);
                return new XElement(tagNameOrTextContent, attributesWhenTag.ToXAttributes(), childNodes?.ArraySelect(childNode => childNode.ToXDocumentFragment()));
            }
        }
    }

    public struct HtmlElement
    {
        public readonly string TagName;
        public readonly HtmlAttribute[] Attributes;
        public readonly HtmlFragment[] ChildNodes;

        [Pure]
        public HtmlElement MoreContent(params HtmlFragment[] content)
        {
            if (ChildNodes == null || content == null) {
                return new HtmlElement(TagName, Attributes, ChildNodes ?? content);
            }
            var newChildNodes = new HtmlFragment[ChildNodes.Length + content.Length];
            Array.Copy(ChildNodes, 0, newChildNodes, 0, ChildNodes.Length);
            Array.Copy(content, 0, newChildNodes, ChildNodes.Length, content.Length);
            return new HtmlElement(TagName, Attributes, newChildNodes);
        }

        public HtmlElement(string tagName, [NotNull] HtmlAttribute[] attributes, HtmlFragment[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            ChildNodes = childNodes;
        }

        [Pure]
        public HtmlFragment Finish() => HtmlFragment.HtmlElement(this);
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
        public static HtmlFragment WrapInHtmlFragment(this IEnumerable<HtmlElement> htmlEls) => HtmlFragment.HtmlElements(htmlEls.Select(el => (HtmlFragment)el).ToArray());
        public static HtmlFragment Finish<TExpression>(this TExpression htmlTagExpr)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Content().Finish();
    }

    public struct HtmlStartTag<TNamedTagType>
        : IFluentHtmlTagExpression<HtmlStartTag<TNamedTagType>>
        where TNamedTagType : struct, IHtmlTagName
    {
        string TagName => default(TNamedTagType).TagName;
        HtmlAttribute[] Attributes { get; }

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
