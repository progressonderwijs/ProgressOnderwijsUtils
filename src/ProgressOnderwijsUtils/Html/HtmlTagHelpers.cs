using System;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TExpression Content<TExpression>(this TExpression htmlTagExpr, params HtmlFragment[] contents)
            where TExpression : struct, IHtmlTagAllowingContent
        {
            htmlTagExpr.Contents = htmlTagExpr.Contents.AppendArrays(contents);
            return htmlTagExpr;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HtmlFragment AsFragment(this IHtmlTag tag) => HtmlFragment.HtmlElement(tag);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute attribute)
            where TExpression : struct, IHtmlTag
            => htmlTagExpr.Attribute(attribute.Name, attribute.Value);

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute? attributeOrNull)
            where TExpression : struct, IHtmlTag
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        [Pure]
        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, string attrName, string attrValue)
            where THtmlTag : struct, IHtmlTag
        {
            if (attrValue == null)
            {
                return htmlTagExpr;
            }
            else
            {
                htmlTagExpr.Attributes = htmlTagExpr.Attributes.Add(attrName, attrValue);
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