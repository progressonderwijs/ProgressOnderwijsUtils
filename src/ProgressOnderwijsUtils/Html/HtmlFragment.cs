using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Html;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlFragment : IConvertibleToFragment
    {
        /// <summary>
        /// Either a string, an IHtmlTag, a non-empty HtmlFragment[], or null (the empty fragment).
        /// </summary>
        public readonly object Content;

        public bool IsTextContent => Content is string;
        public bool IsHtmlElement => Content is IHtmlTag;
        public bool IsCollectionOfFragments => Content is HtmlFragment[];
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
        public static HtmlFragment HtmlElement(IHtmlTag element)
            => new HtmlFragment(element);

        [Pure]
        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            => new HtmlFragment(new HtmlElement(tagName, attributes ?? Array.Empty<HtmlAttribute>(), childNodes));

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

        public override string ToString() => "HtmlFragment: " + this.SerializeToStringWithoutDoctype();
        public static HtmlFragment Empty => default(HtmlFragment);
        public static implicit operator HtmlFragment(HtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);

        [Pure]
        HtmlFragment IConvertibleToFragment.AsFragment() => this;
    }
}