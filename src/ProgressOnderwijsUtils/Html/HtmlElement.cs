using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlElement : IHtmlTagAllowingContent<HtmlElement>
    {
        public HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
            : this(tagName, 
                  attributes == null || attributes.Length == 0 ? HtmlAttributes.Empty : HtmlAttributes.FromArray(attributes), 
                  childNodes == null || childNodes.Length == 0 ? null : childNodes) { }

        internal HtmlElement(string tagName, HtmlAttributes attributes, HtmlFragment[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            Contents = childNodes;
        }

        public HtmlElement(string tagName)
        {
            TagName = tagName;
            Attributes = HtmlAttributes.Empty;
            Contents = null;
        }


        public string TagName { get; }
        public HtmlAttributes Attributes { get; }
        public HtmlFragment[] Contents { get; }

        [Pure] public HtmlFragment AsFragment() => this;

        string IHtmlTag.TagStart => "<" + TagName;
        string IHtmlTag.EndTag => Contents != null || !TagDescription.LookupTag(TagName).IsSelfClosing ? "</" + TagName + ">" : "";


        IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
        HtmlElement IHtmlTag<HtmlElement>.WithAttributes(HtmlAttributes replacementAttributes) => new HtmlElement(TagName, replacementAttributes, Contents);
        HtmlElement IHtmlTagAllowingContent<HtmlElement>.WithContents(HtmlFragment[] replacementContents) => new HtmlElement(TagName, Attributes, replacementContents);
    }
}
