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

    public static bool HasClass(HtmlAttributes atr, CssClass cssClass)
    {
        var classChars = cssClass.ClassName.AsSpan();
        foreach (var attr in atr) {
            if (attr.Name == "class") {
                var haystack = attr.Value.AsSpan();
                while (haystack.Length > 0) {
                    var endIdx = haystack.IndexOf(' ');

                    ReadOnlySpan<char> head;
                    if (endIdx == -1) {
                        head = haystack;
                        haystack = new();
                    } else {
                        head = haystack[..endIdx];
                        haystack = haystack[(endIdx + 1)..];
                    }
                    if (head.SequenceEqual(classChars) && head.Length > 0) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}

public sealed record CssClass(string? ClassName);
