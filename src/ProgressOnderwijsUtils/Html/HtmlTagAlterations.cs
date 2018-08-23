using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlTagAlterations
    {
        public static IHtmlElement ReplaceAttributes([NotNull] IHtmlElement element, HtmlAttributes attributes)
            => element.ApplyChange(new AttributeAlteration(attributes));

        public static IHtmlElement ReplaceAttributes([NotNull] IHtmlElement element, [NotNull] IEnumerable<HtmlAttribute> attributes)
            => element.ApplyChange(new AttributeAlteration(attributes.ToHtmlAttributes()));

        public static IHtmlElementAllowingContent ReplaceContents([NotNull] this  IHtmlElementAllowingContent element, HtmlFragment children)
            => (IHtmlElementAllowingContent)element.ApplyChange(new ContentAlteration(children));

        public static IHtmlElement ReplaceAttributesAndContents([NotNull] IHtmlElement element, [NotNull] IEnumerable<HtmlAttribute> attributes, HtmlFragment children)
            => ReplaceAttributesAndContents(element, attributes.ToHtmlAttributes(), children);

        public static IHtmlElement ReplaceAttributesAndContents([NotNull] IHtmlElement element, HtmlAttributes attributes, HtmlFragment children)
        {
            if (!children.IsEmpty && !(element is IHtmlElementAllowingContent)) {
                throw new InvalidOperationException("Cannot insert content into empty tag");
            }
            return element.ApplyChange(new HtmlElementContentAndAttributeAlteration(attributes, children));
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

            public TSelf ChangeEmpty<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed.WithAttributes(newAttributes);

            public TSelf ChangeWithContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.WithAttributes(newAttributes).WithContents(newContents);
        }

        struct ContentAlteration : IHtmlElementAlteration
        {
            readonly HtmlFragment newContent;

            public ContentAlteration(HtmlFragment newContent)
                => this.newContent = newContent;

            public TSelf ChangeEmpty<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed;

            public TSelf ChangeWithContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.WithContents(newContent);
        }

        struct AttributeAlteration : IHtmlElementAlteration
        {
            readonly HtmlAttributes newAttributes;

            public AttributeAlteration(HtmlAttributes newAttributes)
            {
                this.newAttributes = newAttributes;
            }

            public TSelf ChangeEmpty<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElement<TSelf>
                => typed.WithAttributes(newAttributes);

            public TSelf ChangeWithContent<TSelf>(TSelf typed)
                where TSelf : struct, IHtmlElementAllowingContent<TSelf>
                => typed.WithAttributes(newAttributes);
        }
    }
}
