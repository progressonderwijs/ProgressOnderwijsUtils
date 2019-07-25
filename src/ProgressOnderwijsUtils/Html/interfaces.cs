#nullable disable
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public interface IConvertibleToFragment
    {
        [Pure]
        HtmlFragment AsFragment();
    }

    public interface IHtmlElement : IConvertibleToFragment
    {
        string TagName { get; }
        string TagStart { get; }
        string EndTag { get; }
        HtmlAttributes Attributes { get; }

        /// <summary>
        /// See HtmlTagAlterations for simple examples.  Upon using this method, the "change" parameter with receive a call to
        /// either ChangeWithContent or ChangeEmpty.
        /// </summary>
        IHtmlElement ApplyChange<TAlteration>(TAlteration change)
            where TAlteration : IHtmlElementAlteration;
    }

    public interface IHtmlElementAlteration
    {
        TSelf ChangeEmpty<TSelf>(TSelf typed)
            where TSelf : struct, IHtmlElement<TSelf>;

        TSelf ChangeWithContent<TSelf>(TSelf typed)
            where TSelf : struct, IHtmlElementAllowingContent<TSelf>;
    }

    public interface IHtmlElementAllowingContent : IHtmlElement
    {
        HtmlFragment Contents();
    }

    public interface IHtmlElement<out TSelf> : IHtmlElement
        where TSelf : struct, IHtmlElement<TSelf>
    {
        TSelf WithAttributes(HtmlAttributes replacementAttributes);
    }

    public interface IHtmlElementAllowingContent<out TSelf> : IHtmlElement<TSelf>, IHtmlElementAllowingContent
        where TSelf : struct, IHtmlElementAllowingContent<TSelf>
    {
        TSelf WithContents(HtmlFragment replacementContents);
    }
}
