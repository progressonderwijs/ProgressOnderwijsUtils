namespace ProgressOnderwijsUtils.Html;

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
    IHtmlElement ApplyAlteration<TAlteration>(TAlteration change)
        where TAlteration : IHtmlElementAlteration;
}

public interface IHtmlElementAlteration
{
    TSelf AlterEmptyElement<TSelf>(TSelf typed)
        where TSelf : struct, IHtmlElement<TSelf>;

    TSelf AlterElementAllowingContent<TSelf>(TSelf typed)
        where TSelf : struct, IHtmlElementAllowingContent<TSelf>;
}

public interface IHtmlElementAllowingContent : IHtmlElement
{
    HtmlFragment GetContent();
}

public interface IHtmlElement<out TSelf> : IHtmlElement
    where TSelf : struct, IHtmlElement<TSelf>
{
    TSelf ReplaceAttributesWith(HtmlAttributes replacementAttributes);
}

public interface IHtmlElementAllowingContent<out TSelf> : IHtmlElement<TSelf>, IHtmlElementAllowingContent
    where TSelf : struct, IHtmlElementAllowingContent<TSelf>
{
    TSelf ReplaceContentWith(HtmlFragment replacementContents);
}
