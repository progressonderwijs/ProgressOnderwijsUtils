using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Html;
public static class HtmlClassAttribute
{
    public static THtmlTag _classFromObject<THtmlTag>(this THtmlTag htmlTagExpr, CssClass cssClass) where THtmlTag : struct, IHtmlElement<THtmlTag> => htmlTagExpr.Attribute("class", cssClass.ClassName);

    public static THtmlTag _classFromObjects<THtmlTag>(this THtmlTag htmlTagExpr, CssClass[] cssClasses)
        where THtmlTag : struct, IHtmlElement<THtmlTag>
    {
        var joinedClassList = cssClasses.Select(c => c.ClassName).ToArray().JoinStrings(" ");
        return htmlTagExpr.Attribute("class", joinedClassList);
    }
}

public record CssClass(string ClassName)
{
    public readonly string ClassName = ClassName;
}
