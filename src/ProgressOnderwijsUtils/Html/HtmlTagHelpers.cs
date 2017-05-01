using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlTagHelpers
    {
        [Pure]
        public static THtmlTag Attributes<THtmlTag>(this THtmlTag htmlTagExpr, IEnumerable<HtmlAttribute> attributes)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
        {
            foreach (var attribute in attributes) {
                htmlTagExpr = htmlTagExpr.Attribute(attribute.Name, attribute.Value);
            }
            return htmlTagExpr;
        }

        [Pure]
        public static THtmlTag Contents<THtmlTag, TContent>(this THtmlTag htmlTagExpr, IEnumerable<TContent> contents)
            where THtmlTag : struct, IHtmlTagAllowingContent<THtmlTag>
            where TContent : IConvertibleToFragment
            => htmlTagExpr.Content(contents.Select(el => el.AsFragment()).ToArray());

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Content<THtmlTag>(this THtmlTag htmlTagExpr, params HtmlFragment[] contents)
            where THtmlTag : struct, IHtmlTagAllowingContent<THtmlTag>
            => htmlTagExpr.WithContents(htmlTagExpr.Contents.AppendArrays(contents));

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HtmlFragment AsFragment(this IHtmlTag tag) => HtmlFragment.HtmlElement(tag);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, HtmlAttribute attribute)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, HtmlAttribute? attributeOrNull)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        [Pure]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, string attrName, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => attrValue == null ? htmlTagExpr : htmlTagExpr.WithAttributes(htmlTagExpr.Attributes.Add(attrName, attrValue));

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
