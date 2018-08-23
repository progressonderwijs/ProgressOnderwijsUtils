using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlHelpers
    {
        [Pure]
        public static THtmlTag Attributes<THtmlTag>(this THtmlTag htmlTagExpr, [NotNull] IEnumerable<HtmlAttribute> attributes)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.Attribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Content<THtmlTag>(this THtmlTag htmlTagExpr, params HtmlFragment[] contents)
            where THtmlTag : struct, IHtmlElementAllowingContent<THtmlTag>
            => htmlTagExpr.WithContents(HtmlFragment.Fragment(htmlTagExpr.Contents, HtmlFragment.Fragment(contents)));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HtmlFragment AsFragment(this IHtmlElement element)
            => HtmlFragment.Element(element);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, HtmlAttribute attribute)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, HtmlAttribute? attributeOrNull)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        [Pure]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, string attrName, [CanBeNull] string attrValue)
            where THtmlTag : struct, IHtmlElement<THtmlTag>
            => attrValue == null ? htmlTagExpr : htmlTagExpr.WithAttributes(htmlTagExpr.Attributes.Add(attrName, attrValue));

        [Pure]
        public static HtmlFragment WrapInHtmlFragment<T>([NotNull] this IEnumerable<T> htmlContents)
            where T : IConvertibleToFragment
            => HtmlFragment.Fragment(htmlContents.Select(el => el.AsFragment()).Where(frag => !frag.IsEmpty).ToArray());

        public static HtmlFragment EmptyIfNull<TContent>(this TContent? htmlFragmentOrNull)
            where TContent : struct, IConvertibleToFragment
            => htmlFragmentOrNull?.AsFragment() ?? HtmlFragment.Empty;

        public static HtmlFragment AsFragment([CanBeNull] this string textContent)
            => string.IsNullOrEmpty(textContent) ? HtmlFragment.Empty : HtmlFragment.TextContent(textContent);

        public static HtmlFragment Append<T>([CanBeNull] this T head, HtmlFragment tail)
            where T : IConvertibleToFragment
            => (head?.AsFragment() ?? HtmlFragment.Empty).Append(tail);

        public static HtmlFragment Append<T>([CanBeNull] this T head, HtmlFragment tail, params HtmlFragment[] longTail)
            where T : IConvertibleToFragment
            => (head?.AsFragment() ?? HtmlFragment.Empty).Append(tail, longTail);

        public static HtmlFragment Append([CanBeNull] this string head, HtmlFragment tail)
            => head.AsFragment().Append(tail);

        public static HtmlFragment Append([CanBeNull] this string head, HtmlFragment tail, params HtmlFragment[] longTail)
            => head.AsFragment().Append(tail, longTail);

        public static HtmlFragment JoinHtml<TFragments>([NotNull] [ItemNotNull] this IEnumerable<TFragments> htmlEls)
            where TFragments : IConvertibleToFragment
            => JoinHtml(htmlEls, HtmlFragment.Empty);

        public static HtmlFragment JoinHtml<TFragments>([NotNull] [ItemNotNull] this IEnumerable<TFragments> htmlEls, HtmlFragment joiner)
            where TFragments : IConvertibleToFragment
        {
            using (var enumerator = htmlEls.GetEnumerator()) {
                if (!enumerator.MoveNext()) {
                    return HtmlFragment.Empty;
                }
                var retval = new ArrayBuilder<HtmlFragment>();
                var joinerIsNonEmpty = !joiner.IsEmpty;
                // ReSharper disable once PossibleNullReferenceException
                var firstNode = enumerator.Current.AsFragment();
                retval.Add(firstNode);
                while (enumerator.MoveNext()) {
                    if (joinerIsNonEmpty) {
                        retval.Add(joiner);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    retval.Add(enumerator.Current.AsFragment());
                }
                return HtmlFragment.Fragment(retval.ToArray());
            }
        }

        public static HtmlFragment ContainedContent(this IHtmlElement element)
            => element is IHtmlElementAllowingContent elemWithContent ? elemWithContent.Contents : HtmlFragment.Empty;

        public static HtmlFragment[] ChildNodes(this IHtmlElement element)
            => element is IHtmlElementAllowingContent elemWithContent ? elemWithContent.ChildNodes() : HtmlFragment.EmptyNodes;

        public static HtmlFragment[] ChildNodes(this IHtmlElementAllowingContent elemWithContent)
            => elemWithContent.Contents.NodesOfFragment() ?? HtmlFragment.EmptyNodes;

        public static HtmlAttributes ToHtmlAttributes([NotNull] this IEnumerable<HtmlAttribute> attributes)
            => attributes as HtmlAttributes? ?? HtmlAttributes.FromArray(attributes as HtmlAttribute[] ?? attributes.ToArray());

        public static bool IsNamed([NotNull] this IHtmlElement element, string tagName)
            => element.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase); //IHtmlTag

        public static bool IsNamed<TTag>([NotNull] this IHtmlElement element, TTag tagName)
            where TTag : struct, IHtmlElement<TTag>
            => element.TagName.Equals(tagName.TagName, StringComparison.OrdinalIgnoreCase);
    }
}
