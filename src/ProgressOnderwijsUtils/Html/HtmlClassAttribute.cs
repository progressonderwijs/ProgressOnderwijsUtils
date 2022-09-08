using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Html;

public static class HtmlClassAttribute
{
    public static THtmlTag _class<THtmlTag>(this THtmlTag htmlTagExpr, CssClass? cssClass)
        where THtmlTag : struct, IHtmlElement<THtmlTag>
        => htmlTagExpr.Attribute("class", cssClass?.ClassName);

    public static THtmlTag _class<THtmlTag>(this THtmlTag htmlTagExpr, params CssClass?[]? cssClasses)
        where THtmlTag : struct, IHtmlElement<THtmlTag>
    {
        if (cssClasses is not null) {
            foreach (var cssClass in cssClasses) {
                htmlTagExpr = htmlTagExpr._class(cssClass);
            }
        }
        return htmlTagExpr;
    }
}

public sealed record CssClass(string ClassName);
