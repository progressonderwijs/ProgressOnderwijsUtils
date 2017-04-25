using System;
using System.Collections.Generic;
using System.Linq;
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
        {
            if (attribute.Value == null) {
                return htmlTagExpr;
            } else {
                var attributes = htmlTagExpr.Attributes ?? Array.Empty<HtmlAttribute>();
                //performance assumption: the list of attributes is short.
                Array.Resize(ref attributes, attributes.Length + 1);
                attributes[attributes.Length - 1] = attribute;
                htmlTagExpr.Attributes = attributes;
                return htmlTagExpr;
            }
        }

        [Pure]
        public static TExpression Attribute<TExpression>(this TExpression htmlTagExpr, HtmlAttribute? attributeOrNull)
            where TExpression : struct, IHtmlTag
            => attributeOrNull == null ? htmlTagExpr : htmlTagExpr.Attribute(attributeOrNull.Value);

        public static THtmlTag Attribute<THtmlTag>(this THtmlTag htmlTagExpr, string attrName, string attrValue)
            where THtmlTag : struct, IHtmlTag
            => htmlTagExpr.Attribute(new HtmlAttribute { Name = attrName, Value = attrValue });

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