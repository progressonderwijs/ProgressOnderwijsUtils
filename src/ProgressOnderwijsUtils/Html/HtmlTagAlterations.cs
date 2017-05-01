using System;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlTagAlterations
    {
        public static IHtmlTag ReplaceAttributes(IHtmlTag tag, HtmlAttributes attributes) => tag.ApplyChange(new HtmlTagAtributeAlteration(attributes));
        public static IHtmlTagAllowingContent ReplaceContents(IHtmlTagAllowingContent tag, HtmlFragment[] children) => (IHtmlTagAllowingContent)tag.ApplyChange(new HtmlTagContentAlteration(children));

        public static IHtmlTag ReplaceAttributesAndContents(IHtmlTag tag, HtmlAttributes attributes, HtmlFragment[] children)
        {
            if (children != null && children.Length > 0 && !(tag is IHtmlTagAllowingContent)) {
                throw new InvalidOperationException("Cannot insert content into empty tag");
            }
            return tag.ApplyChange(new HtmlTagContentAndAttributeAlteration(attributes, children));
        }

        struct HtmlTagContentAndAttributeAlteration : IHtmlTagAlteration
        {
            readonly HtmlAttributes newAttributes;
            readonly HtmlFragment[] newContents;

            public HtmlTagContentAndAttributeAlteration(HtmlAttributes newAttributes, HtmlFragment[] newContents)
            {
                this.newContents = newContents;
                this.newAttributes = newAttributes;
            }

            public TSelf ChangeEmpty<TSelf>(TSelf typed) where TSelf : struct, IHtmlTag<TSelf>
                => typed.WithAttributes(newAttributes);

            public TSelf ChangeWithContent<TSelf>(TSelf typed) where TSelf : struct, IHtmlTagAllowingContent<TSelf>
                => typed.WithAttributes(newAttributes).WithContents(newContents);
        }

        struct HtmlTagContentAlteration : IHtmlTagAlteration
        {
            readonly HtmlFragment[] newContents;

            public HtmlTagContentAlteration(HtmlFragment[] newContents)
            {
                this.newContents = newContents;
            }

            public TSelf ChangeEmpty<TSelf>(TSelf typed) where TSelf : struct, IHtmlTag<TSelf>
                => typed;

            public TSelf ChangeWithContent<TSelf>(TSelf typed) where TSelf : struct, IHtmlTagAllowingContent<TSelf>
                => typed.WithContents(newContents);
        }

        struct HtmlTagAtributeAlteration : IHtmlTagAlteration
        {
            readonly HtmlAttributes newAttributes;

            public HtmlTagAtributeAlteration(HtmlAttributes newAttributes)
            {
                this.newAttributes = newAttributes;
            }

            public TSelf ChangeEmpty<TSelf>(TSelf typed) where TSelf : struct, IHtmlTag<TSelf>
                => typed.WithAttributes(newAttributes);

            public TSelf ChangeWithContent<TSelf>(TSelf typed) where TSelf : struct, IHtmlTagAllowingContent<TSelf>
                => typed.WithAttributes(newAttributes);
        }
    }
}
