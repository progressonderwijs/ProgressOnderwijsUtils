﻿using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct CustomHtmlElement : IHtmlElementAllowingContent<CustomHtmlElement>
    {
        readonly HtmlFragment contents;

        public CustomHtmlElement(string tagName, HtmlAttribute[]? attributes, HtmlFragment[]? childNodes)
            : this(tagName,
                attributes == null || attributes.Length == 0 ? HtmlAttributes.Empty : HtmlAttributes.FromArray(attributes),
                childNodes == null || childNodes.Length == 0 ? null : childNodes) { }

        internal CustomHtmlElement(string tagName, HtmlAttributes attributes, HtmlFragment childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            contents = childNodes;
        }

        public CustomHtmlElement(string tagName)
        {
            TagName = tagName;
            Attributes = HtmlAttributes.Empty;
            contents = HtmlFragment.Empty;
        }

        public string TagName { get; }
        public HtmlAttributes Attributes { get; }

        public HtmlFragment GetContent()
            => contents;

        [Pure]
        public HtmlFragment AsFragment()
            => this;

        [NotNull]
        string IHtmlElement.TagStart
            => "<" + TagName;

        [NotNull]
        string IHtmlElement.EndTag
            => !GetContent().IsEmpty || !TagDescription.LookupTag(TagName).IsSelfClosing ? "</" + TagName + ">" : "";

        /// <summary>
        /// Returns the predefined implementation for non-custom html tags (e.g. HtmlTagKinds.TABLE for a custom-tag with name "table").
        /// </summary>
        public IHtmlElement Canonicalize()
        {
            var tagDescription = TagDescription.LookupTag(TagName);
            return tagDescription.EmptyValue == null
                ? this
                : HtmlElementAlterations.ReplaceAttributesAndContents(tagDescription.EmptyValue, Attributes, GetContent());
        }

        [NotNull]
        IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>([NotNull] THtmlTagAlteration change)
            => change.AlterElementAllowingContent(this);

        public static HtmlFragment operator +(CustomHtmlElement head, HtmlFragment tail)
            => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);

        public static HtmlFragment operator +(string head, CustomHtmlElement tail)
            => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));

        CustomHtmlElement IHtmlElement<CustomHtmlElement>.ReplaceAttributesWith(HtmlAttributes replacementAttributes)
            => new CustomHtmlElement(TagName, replacementAttributes, GetContent());

        CustomHtmlElement IHtmlElementAllowingContent<CustomHtmlElement>.ReplaceContentWith(HtmlFragment replacementContents)
            => new CustomHtmlElement(TagName, Attributes, replacementContents);
    }
}
