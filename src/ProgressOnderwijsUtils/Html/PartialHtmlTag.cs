using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    static class HtmlAttributeHelpers
    {
        public static readonly HtmlAttribute[] EmptyAttributes = new HtmlAttribute[0];

        [Pure]
        public static HtmlAttribute[] appendAttr(this HtmlAttribute[] attributes, string attrName, string attrValue)
        {
            //performance assumption: the list of attributes is short.
            Array.Resize(ref attributes, attributes.Length + 1);
            attributes[attributes.Length - 1] = new HtmlAttribute { Name = attrName, Value = attrValue };
            return attributes;
        }
    }

    public struct HtmlFragment : IConvertibleToFragment
    {
        object Content; //null, string, HtmlFragment[] or IHtmlTag
        public bool IsTextContent => Content is string;
        // - A single element node    
        //      (without embeddedContent, WITH tagNameOrTextContent, WITH attributesWhenTag, ? childNodes)
        public bool IsHtmlElement => Content is IHtmlTag;
        // - A collection of fragments
        //      (without embeddedContent, without tagNameOrTextContent, without attributesWhenTag, ? childNodes)
        public bool IsCollectionOfFragments => Content is IHtmlTag || IsEmpty;
        public bool IsEmpty => Content == null;

        /// <summary>
        /// Only elements and fragments can have children; always empty for text nodes
        /// </summary>
        public IReadOnlyList<HtmlFragment> Children => Content as HtmlFragment[] ?? (Content as IHtmlTagAllowingContent).Contents ?? Array.Empty<HtmlFragment>();

        HtmlFragment(object content)
        {
            Content = content;
            Debug.Assert((IsTextContent ? 1 : 0) + (IsHtmlElement ? 1 : 0) + (IsCollectionOfFragments ? 1 : 0) == 1);
        }

        [Pure]
        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent);

        [Pure]
        public static HtmlFragment HtmlElement(HtmlElement element)
            => new HtmlFragment(element);

        [Pure]
        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            => new HtmlFragment(new HtmlElement(tagName, attributes ?? HtmlAttributeHelpers.EmptyAttributes, childNodes));

        [Pure]
        public static HtmlFragment Fragment(params HtmlFragment[] htmlEls)
            => htmlEls == null || htmlEls.Length == 0
                ? Empty
                : htmlEls.Length == 1
                    ? htmlEls[0]
                    : new HtmlFragment(htmlEls);

        [Pure]
        public static HtmlFragment Fragment<T>(IEnumerable<T> htmlEls)
            where T : IConvertibleToFragment
            => Fragment(htmlEls.Select(el => el.AsFragment()).ToArray());

        public static HtmlFragment Empty => default(HtmlFragment);
        public static implicit operator HtmlFragment(HtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);

        public string TextContent()
        {
            var fastStringBuilder = FastShortStringBuilder.Create();
            AppendTextContent(ref fastStringBuilder);
            return fastStringBuilder.Value;
        }

        void AppendTextContent(ref FastShortStringBuilder fastStringBuilder)
        {
            if (Content is string str) {
                fastStringBuilder.AppendText(str);
            } else {
                Debug.Assert(IsHtmlElement || IsCollectionOfFragments);
                foreach (var child in Children) {
                    child.AppendTextContent(ref fastStringBuilder);
                }
            }
        }

        public override string ToString() => "HtmlFragment: " + this.SerializeToString();

        [Pure]
        HtmlFragment IConvertibleToFragment.AsFragment() => this;

        internal void AppendToBuilder(ref FastShortStringBuilder stringBuilder)
        {
            if (Content is HtmlFragment[] fragments) {
                Debug.Assert(IsCollectionOfFragments);
                AppendChildrenToBuilder(ref stringBuilder, fragments);
            } else if (attributesWhenTag == null) {
                Debug.Assert(IsTextContent);
                AppendEscapedText(ref stringBuilder);
            } else {
                Debug.Assert(IsHtmlElement);
                stringBuilder.AppendText("<");
                stringBuilder.AppendText(tagNameOrTextContent);
                AppendAttributes(ref stringBuilder);

                stringBuilder.AppendText(">");

                if (tagNameOrTextContent != "area"
                    && tagNameOrTextContent != "base"
                    && tagNameOrTextContent != "br"
                    && tagNameOrTextContent != "col"
                    && tagNameOrTextContent != "embed"
                    && tagNameOrTextContent != "hr"
                    && tagNameOrTextContent != "img"
                    && tagNameOrTextContent != "input"
                    && tagNameOrTextContent != "keygen"
                    && tagNameOrTextContent != "link"
                    && tagNameOrTextContent != "menuitem"
                    && tagNameOrTextContent != "meta"
                    && tagNameOrTextContent != "param"
                    && tagNameOrTextContent != "source"
                    && tagNameOrTextContent != "track"
                    && tagNameOrTextContent != "wbr") {
                    if (tagNameOrTextContent == "script" || tagNameOrTextContent == "style") {
                        AppendChildrenAsRawText(ref stringBuilder);
                    } else {
                        AppendChildrenToBuilder(ref stringBuilder);
                    }
                    stringBuilder.AppendText("</");
                    stringBuilder.AppendText(tagNameOrTextContent);
                    stringBuilder.AppendText(">");
                } else {
                    Debug.Assert(childNodes == null || childNodes.Length == 0);
                }
            }
        }

        void AppendAttributes(ref FastShortStringBuilder stringBuilder)
        {
            var className = default(string);
            foreach (var htmlAttribute in attributesWhenTag) {
                if (htmlAttribute.Name == "class") {
                    className = className == null ? htmlAttribute.Value : className + " " + htmlAttribute.Value;
                } else {
                    AppendAttribute(ref stringBuilder, htmlAttribute);
                }
            }
            if (className != null) {
                AppendAttribute(ref stringBuilder, new HtmlAttribute("class", className));
            }
        }

        static void AppendAttribute(ref FastShortStringBuilder stringBuilder, HtmlAttribute htmlAttribute)
        {
            stringBuilder.AppendText(" ");
            stringBuilder.AppendText(htmlAttribute.Name);
            if (htmlAttribute.Value != "") {
                stringBuilder.AppendText("=\"");
                AppendEscapedAttributeValue(ref stringBuilder, htmlAttribute.Value);
                stringBuilder.AppendText("\"");
            }
        }

        void AppendChildrenAsRawText(ref FastShortStringBuilder stringBuilder)
        {
            if (childNodes != null) {
                foreach (var childNode in childNodes) {
                    childNode.AppendAsRawTextToBuilder(ref stringBuilder);
                }
            }
        }

        void AppendAsRawTextToBuilder(ref FastShortStringBuilder stringBuilder)
        {
            if (tagNameOrTextContent == null) {
                Debug.Assert(IsCollectionOfFragments);
                AppendChildrenAsRawText(ref stringBuilder);
            } else if (attributesWhenTag == null) {
                Debug.Assert(IsTextContent);
                stringBuilder.AppendText(tagNameOrTextContent);
            } else {
                Debug.Assert(IsHtmlElement);
                throw new InvalidOperationException("script and style tags cannot contain child elements");
            }
        }

        void AppendEscapedText(ref FastShortStringBuilder stringBuilder)
        {
            var uptoIndex = 0;
            for (var textIndex = 0; textIndex < tagNameOrTextContent.Length; textIndex++) {
                var c = tagNameOrTextContent[textIndex];
                if (c <= '>') {
                    //https://html.spec.whatwg.org/#elements-2:normal-elements-4
                    //normal text must not contain < or & unescaped
                    if (c == '<') {
                        stringBuilder.AppendText(tagNameOrTextContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&lt;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '&') {
                        stringBuilder.AppendText(tagNameOrTextContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&amp;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '>') {
                        //not strictly necessary
                        stringBuilder.AppendText(tagNameOrTextContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&gt;");
                        uptoIndex = textIndex + 1;
                    }
                }
            }
            stringBuilder.AppendText(tagNameOrTextContent, uptoIndex, tagNameOrTextContent.Length - uptoIndex);
        }

        static void AppendEscapedAttributeValue(ref FastShortStringBuilder stringBuilder, string attrValue)
        {
            var uptoIndex = 0;
            for (var textIndex = 0; textIndex < attrValue.Length; textIndex++) {
                var c = attrValue[textIndex];
                if (c <= '&') {
                    //https://html.spec.whatwg.org/#attributes-2
                    //quoted attribute values must not contain " or & unescaped
                    if (c == '&') {
                        stringBuilder.AppendText(attrValue, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&amp;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '"') {
                        stringBuilder.AppendText(attrValue, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&quot;");
                        uptoIndex = textIndex + 1;
                    }
                }
            }
            stringBuilder.AppendText(attrValue, uptoIndex, attrValue.Length - uptoIndex);
        }

        void AppendChildrenToBuilder(ref FastShortStringBuilder stringBuilder, HtmlFragment[] fragments)
        {
            if (childNodes != null) {
                for (var i = 0; i < childNodes.Length; i++) {
                    childNodes[i].AppendToBuilder(ref stringBuilder);
                }
            }
        }
    }

    public struct HtmlElement : IHtmlTag, IHtmlTagAllowingContent
    {
        public HtmlElement(string tagName, [NotNull] HtmlAttribute[] attributes, HtmlFragment[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            Contents = childNodes;
        }

        [Pure]
        public HtmlFragment AsFragment() => this;

        public string TagName { get; }
        string IHtmlTag.TagStart => "<" + TagName;
        string IHtmlTag.EndTag => "</" + TagName + ">";
        public HtmlAttribute[] Attributes { get; set; }
        public HtmlFragment[] Contents { get; set; }
    }

    public interface IFluentHtmlTagExpression<out TExpression> : IConvertibleToFragment
        where TExpression : IFluentHtmlTagExpression<TExpression>
    {
        [Pure]
        TExpression Attribute(string attrName, string attrValue);

        [Pure]
        TExpression Content(params HtmlFragment[] content);
    }

    public static class HtmlTagHelpers
    {
        [Pure]
        public static TExpression Attributes<TExpression>(this TExpression htmlTagExpr, IEnumerable<HtmlAttribute> attributes)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.Attribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        [Pure]
        public static TExpression Contents<TExpression, TContent>(this TExpression htmlTagExpr, IEnumerable<TContent> items)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            where TContent : IConvertibleToFragment
            => htmlTagExpr.Content(items.Select(el => el.AsFragment()).ToArray());

        [Pure]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute attribute)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression> => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        [Pure]
        [UsefulToKeep("library method")]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute? attributeOrNull)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression> => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, string attrName, string attrValue)
            where THtmlTag : struct, IHtmlTag
        {
            if (attrValue == null) {
                return htmlTagExpr;
            } else {
                htmlTagExpr.Attributes = (htmlTagExpr.Attributes ?? HtmlAttributeHelpers.EmptyAttributes).appendAttr(attrName, attrValue);
                return htmlTagExpr;
            }
        }

        [Pure]
        public static HtmlFragment WrapInHtmlFragment<T>(this IEnumerable<T> htmlEls)
            where T : IConvertibleToFragment
            => HtmlFragment.Fragment(htmlEls.Select(el => el.AsFragment()).ToArray());

        public static HtmlFragment EmptyIfNull<T>(this T? htmlFragmentOrNull)
            where T : struct, IConvertibleToFragment
            => htmlFragmentOrNull?.AsFragment() ?? HtmlFragment.Empty;

        public static HtmlFragment JoinHtml<T>(this IEnumerable<T> htmlEls, HtmlFragment joiner)
            where T : IConvertibleToFragment
        {
            if (joiner.IsEmpty) {
                var retval = new List<HtmlFragment>();
                foreach (var item in htmlEls) {
                    retval.Add(item.AsFragment());
                }
                return HtmlFragment.Fragment(retval.ToArray());
            }
            using (var enumerator = htmlEls.GetEnumerator()) {
                if (!enumerator.MoveNext()) {
                    return HtmlFragment.Empty;
                }
                var firstNode = enumerator.Current.AsFragment();

                var retval = new List<HtmlFragment> { firstNode };

                while (enumerator.MoveNext()) {
                    retval.Add(joiner);
                    retval.Add(enumerator.Current.AsFragment());
                }
                return HtmlFragment.Fragment(retval.ToArray());
            }
        }
    }

    public interface IConvertibleToFragment
    {
        [Pure]
        HtmlFragment AsFragment();
    }

    public struct HtmlTag<TName>
        : IFluentHtmlTagExpression<HtmlTag<TName>>
        where TName : struct, IHtmlTagName
    {
        readonly HtmlAttribute[] Attributes;
        readonly HtmlFragment[] childNodes;

        HtmlTag(HtmlAttribute[] attributes, HtmlFragment[] childNodes)
        {
            Attributes = attributes;
            this.childNodes = childNodes;
        }

        [Pure]
        public HtmlTag<TName> Attribute(string attrName, string attrValue)
            => attrValue == null ? this : new HtmlTag<TName>((Attributes ?? HtmlAttributeHelpers.EmptyAttributes).appendAttr(attrName, attrValue), childNodes);

        [Pure]
        public HtmlTag<TName> Content(params HtmlFragment[] content) => new HtmlTag<TName>(Attributes, ArrayExtensions.AppendArrays(childNodes, content));

        public static implicit operator HtmlFragment(HtmlTag<TName> tag) => HtmlFragment.HtmlElement(default(TName).TagName, tag.Attributes, tag.childNodes);
        public HtmlFragment AsFragment() => this;
    }

    public interface IHtmlTag
    {
        string TagName { get; }
        string TagStart { get; }
        string EndTag { get; }
        HtmlAttribute[] Attributes { get; set; }
    }

    public interface IHtmlTagAllowingContent
    {
        HtmlFragment[] Contents { get; set; }
    }
}
