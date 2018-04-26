using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct CustomHtmlElement : IHtmlTagAllowingContent<CustomHtmlElement>
    {
        public CustomHtmlElement(string tagName, [CanBeNull] HtmlAttribute[] attributes, [CanBeNull] HtmlFragment[] childNodes)
            : this(tagName,
                attributes == null || attributes.Length == 0 ? HtmlAttributes.Empty : HtmlAttributes.FromArray(attributes),
                childNodes == null || childNodes.Length == 0 ? null : childNodes) { }

        internal CustomHtmlElement(string tagName, HtmlAttributes attributes, HtmlFragment[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            Contents = childNodes;
        }

        public CustomHtmlElement(string tagName)
        {
            TagName = tagName;
            Attributes = HtmlAttributes.Empty;
            Contents = null;
        }

        public string TagName { get; }
        public HtmlAttributes Attributes { get; }
        public HtmlFragment[] Contents { get; }

        [Pure]
        public HtmlFragment AsFragment() => this;

        [NotNull]
        string IHtmlTag.TagStart => "<" + TagName;

        [NotNull]
        string IHtmlTag.EndTag => Contents != null || !TagDescription.LookupTag(TagName).IsSelfClosing ? "</" + TagName + ">" : "";

        /// <summary>
        /// Returns the predefined implementation for non-custom html tags (e.g. HtmlTagKinds.TABLE for a custom-tag with name "table").
        /// </summary>
        public IHtmlTag Canonicalize()
        {
            var tagDescription = TagDescription.LookupTag(TagName);
            return tagDescription.EmptyValue == null
                ? this
                : HtmlTagAlterations.ReplaceAttributesAndContents(tagDescription.EmptyValue, Attributes, Contents);
        }

        [NotNull]
        IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>([NotNull] THtmlTagAlteration change) => change.ChangeWithContent(this);

        public static HtmlFragment operator +(CustomHtmlElement head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.HtmlElement(head), tail);
        public static HtmlFragment operator +(string head, CustomHtmlElement tail) => HtmlFragment.Fragment(head, HtmlFragment.HtmlElement(tail));
        CustomHtmlElement IHtmlTag<CustomHtmlElement>.WithAttributes(HtmlAttributes replacementAttributes) => new CustomHtmlElement(TagName, replacementAttributes, Contents);
        CustomHtmlElement IHtmlTagAllowingContent<CustomHtmlElement>.WithContents(HtmlFragment[] replacementContents) => new CustomHtmlElement(TagName, Attributes, replacementContents);
    }
}
