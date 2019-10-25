using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlElementAlterations
    {
        public static IHtmlElement ReplaceAttributesWith([NotNull] IHtmlElement element, HtmlAttributes attributes)
            => element.ApplyAlteration(new AttributeAlteration(attributes));

        public static IHtmlElementAllowingContent ReplaceContentWith([NotNull] this IHtmlElementAllowingContent element, HtmlFragment children)
            => (IHtmlElementAllowingContent)element.ApplyAlteration(new ContentAlteration(children));

        public static IHtmlElement ReplaceAttributesAndContents([NotNull] IHtmlElement element, HtmlAttributes attributes, HtmlFragment children)
        {
            if (!children.IsEmpty && !(element is IHtmlElementAllowingContent)) {
                throw new InvalidOperationException("Cannot insert content into empty tag");
            }
            return element.ApplyAlteration(new HtmlElementContentAndAttributeAlteration(attributes, children));
        }

        struct HtmlElementContentAndAttributeAlteration : IHtmlElementAlteration
        {
            readonly HtmlAttributes newAttributes;
            readonly HtmlFragment newContents;

            public HtmlElementContentAndAttributeAlteration(HtmlAttributes newAttributes, HtmlFragment newContents)
            {
                this.newContents = newContents;
                this.newAttributes = newAttributes;
            }

            public TSelf AlterEmptyElement<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed.ReplaceAttributesWith(newAttributes);

            public TSelf AlterElementAllowingContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.ReplaceAttributesWith(newAttributes).ReplaceContentWith(newContents);
        }

        struct ContentAlteration : IHtmlElementAlteration
        {
            readonly HtmlFragment newContent;

            public ContentAlteration(HtmlFragment newContent)
                => this.newContent = newContent;

            public TSelf AlterEmptyElement<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed;

            public TSelf AlterElementAllowingContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.ReplaceContentWith(newContent);
        }

        struct AttributeAlteration : IHtmlElementAlteration
        {
            readonly HtmlAttributes newAttributes;

            public AttributeAlteration(HtmlAttributes newAttributes)
            {
                this.newAttributes = newAttributes;
            }

            public TSelf AlterEmptyElement<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed.ReplaceAttributesWith(newAttributes);

            public TSelf AlterElementAllowingContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.ReplaceAttributesWith(newAttributes);
        }
    }
}
