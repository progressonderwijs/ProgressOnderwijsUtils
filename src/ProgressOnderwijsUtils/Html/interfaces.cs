using System.Linq;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public interface IConvertibleToFragment
    {
        [Pure]
        HtmlFragment AsFragment();
    }

    public interface IHtmlTag : IConvertibleToFragment
    {
        string TagName { get; }
        string TagStart { get; }
        string EndTag { get; }
        HtmlAttributes Attributes { get; }

        /// <summary>
        /// See HtmlTagAlterations for simple examples.  Upon using this method, the "change" parameter with receive a call to
        /// either ChangeWithContent or ChangeEmpty.
        /// </summary>
        IHtmlTag ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) where THtmlTagAlteration : IHtmlTagAlteration;
    }

    public interface IHtmlTagAlteration
    {
        TSelf ChangeEmpty<TSelf>(TSelf typed) where TSelf : struct, IHtmlTag<TSelf>;
        TSelf ChangeWithContent<TSelf>(TSelf typed) where TSelf : struct, IHtmlTagAllowingContent<TSelf>;
    }

    public interface IHtmlTagAllowingContent : IHtmlTag
    {
        HtmlFragment Contents { get; }
    }

    public interface IHtmlTag<out TSelf> : IHtmlTag
        where TSelf : struct, IHtmlTag<TSelf>
    {
        TSelf WithAttributes(HtmlAttributes replacementAttributes);
    }

    public interface IHtmlTagAllowingContent<out TSelf> : IHtmlTag<TSelf>, IHtmlTagAllowingContent
        where TSelf : struct, IHtmlTagAllowingContent<TSelf>
    {
        TSelf WithContents(HtmlFragment replacementContents);
    }
}
