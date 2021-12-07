using System;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlElementAlterations
    {
        public static IHtmlElement ReplaceAttributesWith(this IHtmlElement element, HtmlAttributes attributes)
            => element.ApplyAlteration(new AttributeAlteration(attributes));

        public static IHtmlElementAllowingContent ReplaceContentWith(this IHtmlElementAllowingContent element, HtmlFragment children)
            => (IHtmlElementAllowingContent)element.ApplyAlteration(new ContentAlteration(children));

        public static IHtmlElement ReplaceAttributesAndContents(this IHtmlElement element, HtmlAttributes attributes, HtmlFragment children)
            => element.ApplyAlteration(new ContentAlteration(children)).ReplaceAttributesWith(attributes);

        struct ContentAlteration : IHtmlElementAlteration
        {
            readonly HtmlFragment newContent;

            public ContentAlteration(HtmlFragment newContent)
                => this.newContent = newContent;

            public TSelf AlterEmptyElement<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => newContent.IsEmpty ? typed : throw new InvalidOperationException("Cannot insert content into empty tag");

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
