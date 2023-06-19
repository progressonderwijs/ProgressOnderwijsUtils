namespace ProgressOnderwijsUtils.Html;

public static class HtmlToCSharp
{
    public static string ToCSharp(this HtmlFragment fragment)
    {
        var builder = MutableShortStringBuilder.Create();
        AppendCSharpTo(ref builder, fragment, 0);
        return builder.FinishBuilding();
    }

    static void AppendCSharpTo(ref MutableShortStringBuilder stringBuilder, HtmlFragment fragment, int indent)
    {
        if (fragment.Implementation is string stringContent) {
            stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(stringContent).AssertNotNull());
        } else if (fragment.Implementation is IHtmlElement htmlTag) {
            var description = TagDescription.LookupTag(htmlTag.TagName);
            if (description.FieldName != null) {
                stringBuilder.AppendText(description.FieldName);
            } else {
                stringBuilder.AppendText("new CustomHtmlElement(");
                stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(htmlTag.TagName).AssertNotNull());
                stringBuilder.AppendText(")");
            }
            var wereAttributesRendered = AppendAttributesAsCSharp(ref stringBuilder, htmlTag.Attributes, description.AttributeMethodsByName, indent);
            if (htmlTag is IHtmlElementAllowingContent withContent && !withContent.GetContent().IsEmpty) {
                var subIndent = wereAttributesRendered ? indent + 8 : indent + 4;
                if (wereAttributesRendered) {
                    AppendNewline(ref stringBuilder);
                    AppendIndent(ref stringBuilder, indent + 4);
                }
                stringBuilder.AppendText(".Content(");
                AppendPlusSeparatedFragments(ref stringBuilder, withContent.GetContent(), subIndent);
                AppendIndent(ref stringBuilder, indent);
                stringBuilder.AppendText(")");
            }
        } else if (fragment.Implementation is HtmlFragment[] fragments) {
            stringBuilder.AppendText("(");
            AppendPlusSeparatedFragments(ref stringBuilder, fragments, indent + 4);
            AppendIndent(ref stringBuilder, indent);
            stringBuilder.AppendText(")");
        } else {
            stringBuilder.AppendText("HtmlFragment.Empty");
        }
    }

    static void AppendPlusSeparatedFragments(ref MutableShortStringBuilder stringBuilder, HtmlFragment contents, int subIndent)
    {
        if (contents.Implementation is HtmlFragment[] fragments) {
            var isSubsequent = false;
            foreach (var fragment in fragments) {
                AppendNewline(ref stringBuilder);
                AppendIndent(ref stringBuilder, subIndent);
                if (isSubsequent) {
                    stringBuilder.AppendText("+ ");
                }
                AppendCSharpTo(ref stringBuilder, fragment, subIndent);

                isSubsequent = true;
            }
        } else if (contents.Implementation != null) {
            AppendNewline(ref stringBuilder);
            AppendIndent(ref stringBuilder, subIndent);
            AppendCSharpTo(ref stringBuilder, contents, subIndent);
        }
        AppendNewline(ref stringBuilder);
    }

    static void AppendNewline(ref MutableShortStringBuilder stringBuilder)
        => stringBuilder.AppendText("\n");

    static void AppendIndent(ref MutableShortStringBuilder stringBuilder, int indent)
        => stringBuilder.AppendText(new string(' ', indent));

    static bool AppendAttributesAsCSharp(ref MutableShortStringBuilder stringBuilder, HtmlAttributes htmlAttributes, IReadOnlyDictionary<string, string> attributeMethodsByName, int indent)
    {
        if (htmlAttributes.Count == 0) {
            return false;
        }
        var hasRenderedAttribute = false;

        foreach (var attr in htmlAttributes) {
            if (hasRenderedAttribute) {
                AppendNewline(ref stringBuilder);
                AppendIndent(ref stringBuilder, indent + 4);
            }
            hasRenderedAttribute = true;
            if (attributeMethodsByName.TryGetValue(attr.Name, out var methodName)) {
                stringBuilder.AppendText(".");
                stringBuilder.AppendText(methodName);
                stringBuilder.AppendText("(");
            } else {
                stringBuilder.AppendText(".Attribute(");
                stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(attr.Name).AssertNotNull());
                stringBuilder.AppendText(",");
            }
            stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(attr.Value).AssertNotNull());
            stringBuilder.AppendText(")");
        }
        return hasRenderedAttribute;
    }
}
