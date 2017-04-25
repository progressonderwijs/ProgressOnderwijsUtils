using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct HtmlElement : IHtmlTagAllowingContent
    {
        public HtmlElement(string tagName, HtmlAttribute[] attributes, HtmlFragment[] childNodes)
        {
            TagName = tagName;
            Attributes = attributes;
            Contents = childNodes;
        }

        public HtmlElement(string tagName)
        {
            TagName = tagName;
            Attributes = null;
            Contents = null;
        }

        [Pure]
        public HtmlFragment AsFragment() => this;

        public string TagName { get; }
        string IHtmlTag.TagStart => "<" + TagName;
        string IHtmlTag.EndTag => "</" + TagName + ">";
        public HtmlAttribute[] Attributes { get; set; }
        public HtmlFragment[] Contents { get; set; }
    }
}