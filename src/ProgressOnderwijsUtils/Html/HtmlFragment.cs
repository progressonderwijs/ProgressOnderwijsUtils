using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        HtmlFragment(object content) => Content = content;

        [Pure]
        public static HtmlFragment TextContent(string textContent) => new HtmlFragment(textContent);

        [Pure]
        public static HtmlFragment HtmlElement(IHtmlTag element) => new HtmlFragment(element);

        [Pure]
        public static HtmlFragment HtmlElement(CustomHtmlElement element) => new HtmlFragment(element.Canonicalize());

        [Pure]
        public static HtmlFragment HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes) => HtmlElement(new CustomHtmlElement(tagName, attributes, childNodes));

        [Pure]
        public static HtmlFragment Fragment([CanBeNull] params HtmlFragment[] htmlEls)
            => htmlEls == null || htmlEls.Length == 0
                ? Empty
                : htmlEls.Length == 1
                    ? htmlEls[0]
                    : new HtmlFragment(htmlEls);

        [Pure]
        public static HtmlFragment Fragment<T>([NotNull] IEnumerable<T> htmlEls)
            where T : IConvertibleToFragment
            => Fragment(htmlEls.Select(el => el.AsFragment()).Where(f => !f.IsEmpty).ToArray());

        public override string ToString() => "HtmlFragment: " + this.SerializeToStringWithoutDoctype();
        public static HtmlFragment Empty => default(HtmlFragment);
        public static implicit operator HtmlFragment(CustomHtmlElement element) => HtmlElement(element);
        public static implicit operator HtmlFragment(string textContent) => TextContent(textContent);

        [Pure]
        HtmlFragment IConvertibleToFragment.AsFragment() => this;
    }
}
