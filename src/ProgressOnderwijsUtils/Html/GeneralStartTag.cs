
#if false

    //probably want this after the html conversion.
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public struct GeneralStartTag
        : IFluentHtmlTagExpression<GeneralStartTag>
    {
        public string TagName { get; }
        public HtmlAttribute[] Attributes { get; }

        public GeneralStartTag(string tagName)
            : this(tagName, HtmlAttributeHelpers.EmptyAttributes) { }

        GeneralStartTag(string tagName, HtmlAttribute[] attributes)
        {
            TagName = tagName;
            Attributes = attributes ?? HtmlAttributeHelpers.EmptyAttributes;
        }

        [Pure]
        public GeneralStartTag Attribute(string attrName, string attrValue)
            => new GeneralStartTag(TagName, Attributes.appendAttr(attrName, attrValue));

        [Pure]
        public HtmlElement Content() => Content(null);

        [Pure]
        public HtmlElement Content(params HtmlFragment[] content) => new HtmlElement(TagName, Attributes, content);

        public static implicit operator HtmlFragment(GeneralStartTag startTag) => HtmlFragment.HtmlElement(startTag.TagName, startTag.Attributes, null);
    }
}
#endif
