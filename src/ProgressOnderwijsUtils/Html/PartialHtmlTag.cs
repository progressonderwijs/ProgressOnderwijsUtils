using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Html
{
    public interface IConvertibleToFragment
    {
        [Pure]
        HtmlFragment AsFragment();
    }

    public interface IHtmlTag : IConvertibleToFragment
    {
        string TagName { get; }
        string TagStart { get; }
        string EndTag { get; }
        HtmlAttribute[] Attributes { get; set; }
    }

    public interface IHtmlTagAllowingContent : IHtmlTag
    {
        HtmlFragment[] Contents { get; set; }
    }

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
        readonly object content; //null, string, HtmlFragment[] or IHtmlTag
        public bool IsTextContent => content is string;
        public bool IsHtmlElement => content is IHtmlTag;
        public bool IsCollectionOfFragments => content is HtmlFragment[] || IsEmpty;
        public bool IsEmpty => content == null;

        /// <summary>
        /// Only elements and fragments can have children; always empty for text nodes
        /// </summary>
        public IReadOnlyList<HtmlFragment> Children => content as HtmlFragment[] ?? (content as IHtmlTagAllowingContent).Contents ?? Array.Empty<HtmlFragment>();

        HtmlFragment(object content)
        {
            this.content = content;
            Debug.Assert((IsTextContent ? 1 : 0) + (IsHtmlElement ? 1 : 0) + (IsCollectionOfFragments ? 1 : 0) == 1);
        }

        [Pure]
        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent);

        [Pure]
        public static HtmlFragment HtmlElement(IHtmlTag element)
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
            if (content is string str) {
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
            if (content is string stringContent) {
                AppendEscapedText(ref stringBuilder, stringContent);
            } else if (content is HtmlFragment[] fragments) {
                for (var i = 0; i < fragments.Length; i++) {
                    fragments[i].AppendToBuilder(ref stringBuilder);
                }
            } else if (content is IHtmlTag htmlTag) {
                stringBuilder.AppendText(htmlTag.TagStart);
                if (htmlTag.Attributes != null) {
                    AppendAttributes(ref stringBuilder, htmlTag.Attributes);
                }
                stringBuilder.AppendText(">");

                if (htmlTag is IHtmlTagAllowingContent htmlTagAllowingContent) {
                    AppendTagContentAndEnd(ref stringBuilder, htmlTagAllowingContent);
                }
            } else {
                Debug.Assert(content == null);
            }
        }

        static void AppendTagContentAndEnd(ref FastShortStringBuilder stringBuilder, IHtmlTagAllowingContent htmlTagAllowingContent)
        {
            var contents = htmlTagAllowingContent.Contents ?? Array.Empty<HtmlFragment>();
            if (htmlTagAllowingContent.TagName == "script" || htmlTagAllowingContent.TagName == "style") {
                foreach (var childNode in contents) {
                    childNode.AppendAsRawTextToBuilder(ref stringBuilder);
                }
            } else {
                foreach (var childNode in contents) {
                    childNode.AppendToBuilder(ref stringBuilder);
                }
            }
            stringBuilder.AppendText(htmlTagAllowingContent.EndTag);
        }

        static void AppendAttributes(ref FastShortStringBuilder stringBuilder, HtmlAttribute[] attributes)
        {
            var className = default(string);
            foreach (var htmlAttribute in attributes) {
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

        void AppendAsRawTextToBuilder(ref FastShortStringBuilder stringBuilder)
        {
            if (content is HtmlFragment[] fragments) {
                Debug.Assert(IsCollectionOfFragments);
                foreach (var childNode in fragments) {
                    childNode.AppendAsRawTextToBuilder(ref stringBuilder);
                }
            } else if (content is string contentString) {
                Debug.Assert(IsTextContent);
                stringBuilder.AppendText(contentString);
            } else if (content != null) {
                Debug.Assert(IsHtmlElement);
                throw new InvalidOperationException("script and style tags cannot contain child elements");
            }
        }

        static void AppendEscapedText(ref FastShortStringBuilder stringBuilder, string stringContent)
        {
            var uptoIndex = 0;
            for (var textIndex = 0; textIndex < stringContent.Length; textIndex++) {
                var c = stringContent[textIndex];
                if (c <= '>') {
                    //https://html.spec.whatwg.org/#elements-2:normal-elements-4
                    //normal text must not contain < or & unescaped
                    if (c == '<') {
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&lt;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '&') {
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&amp;");
                        uptoIndex = textIndex + 1;
                    } else if (c == '>') {
                        //not strictly necessary
                        stringBuilder.AppendText(stringContent, uptoIndex, textIndex - uptoIndex);
                        stringBuilder.AppendText("&gt;");
                        uptoIndex = textIndex + 1;
                    }
                }
            }
            stringBuilder.AppendText(stringContent, uptoIndex, stringContent.Length - uptoIndex);
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
    }

    public struct HtmlElement : IHtmlTagAllowingContent
    {
        public HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
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

    public static class HtmlTagHelpers
    {
        [Pure]
        public static TExpression Attributes<TExpression>(this TExpression htmlTagExpr, IEnumerable<HtmlAttribute> attributes)
            where TExpression : struct, IHtmlTag
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.Attribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        [Pure]
        public static TExpression Contents<TExpression, TContent>(this TExpression htmlTagExpr, IEnumerable<TContent> contents)
            where TExpression : struct, IHtmlTagAllowingContent
            where TContent : IConvertibleToFragment
            => htmlTagExpr.Content(contents.Select(el => el.AsFragment()).ToArray());

        [Pure]
        public static TExpression Content<TExpression>(this TExpression htmlTagExpr, params HtmlFragment[] contents)
            where TExpression : struct, IHtmlTagAllowingContent
        {
            htmlTagExpr.Contents = htmlTagExpr.Contents.AppendArrays(contents);
            return htmlTagExpr;
        }

        [Pure]
        public static HtmlFragment AsFragment(this IHtmlTag tag) => HtmlFragment.HtmlElement(tag);

        [Pure]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute attribute)
            where TExpression : struct, IHtmlTag
            => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        [Pure]
        [UsefulToKeep("library method")]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute? attributeOrNull)
            where TExpression : struct, IHtmlTag
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

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

        public static HtmlFragment EmptyIfNull<TContent>(this TContent? htmlFragmentOrNull)
            where TContent : struct, IConvertibleToFragment
            => htmlFragmentOrNull?.AsFragment() ?? HtmlFragment.Empty;

        public static HtmlFragment JoinHtml<TFragments>(this IEnumerable<TFragments> htmlEls, HtmlFragment joiner)
            where TFragments : IConvertibleToFragment
        {
            using (var enumerator = htmlEls.GetEnumerator()) {
                if (!enumerator.MoveNext()) {
                    return HtmlFragment.Empty;
                }
                var retval = FastArrayBuilder<HtmlFragment>.Create();
                var joinerIsNonEmpty = !joiner.IsEmpty;
                var firstNode = enumerator.Current.AsFragment();
                retval.Add(firstNode);
                while (enumerator.MoveNext()) {
                    if (joinerIsNonEmpty) {
                        retval.Add(joiner);
                    }
                    retval.Add(enumerator.Current.AsFragment());
                }
                return HtmlFragment.Fragment(retval.ToArray());
            }
        }
    }
}
