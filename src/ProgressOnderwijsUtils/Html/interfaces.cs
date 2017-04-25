using System;
using System.Collections.Generic;
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
        HtmlAttribute[] Attributes { get; set; }
    }

    public interface IHtmlTagAllowingContent : IHtmlTag
    {
        HtmlFragment[] Contents { get; set; }
    }
}
