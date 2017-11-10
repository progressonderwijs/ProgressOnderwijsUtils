using System.Collections.Generic;
using System.Linq;
using ExpressionToCodeLib;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToCSharp
    {
        [NotNull]
        public static string ToCSharp(this HtmlFragment fragment)
        {
            var builder = FastShortStringBuilder.Create();
            AppendCSharpTo(ref builder, fragment, 0);
            return builder.FinishBuilding();
        }

        static void AppendCSharpTo(ref FastShortStringBuilder stringBuilder, HtmlFragment fragment, int indent)
        {
            if (fragment.Content is string stringContent) {
                stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(stringContent));
            } else if (fragment.Content is IHtmlTag htmlTag) {
                var description = TagDescription.LookupTag(htmlTag.TagName);
                if (description.FieldName != null) {
                    stringBuilder.AppendText(description.FieldName);
                } else {
                    stringBuilder.AppendText("new HtmlElement(");
                    stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(htmlTag.TagName));
                    stringBuilder.AppendText(")");
                }
                var wereAttributesRendered = AppendAttributesAsCSharp(ref stringBuilder, htmlTag.Attributes, description.AttributeMethodsByName, indent);
                if (htmlTag is IHtmlTagAllowingContent withContent && (withContent.Contents?.Length ?? 0) > 0) {
                    var subIndent = wereAttributesRendered ? indent + 8 : indent + 4;
                    if (wereAttributesRendered) {
                        AppendNewline(ref stringBuilder);
                        AppendIndent(ref stringBuilder, indent + 4);
                    }
                    stringBuilder.AppendText(".Content(");
                    AppendCommaSeparatedFragments(ref stringBuilder, withContent.Contents, subIndent);
                    AppendIndent(ref stringBuilder, indent);
                    stringBuilder.AppendText(")");
                }
            } else if (fragment.Content is HtmlFragment[] fragments) {
                stringBuilder.AppendText("HtmlFragment.Fragment(");
                AppendCommaSeparatedFragments(ref stringBuilder, fragments, indent + 4);
                AppendIndent(ref stringBuilder, indent);
                stringBuilder.AppendText(")");
            } else {
                stringBuilder.AppendText("HtmlFragment.Empty");
            }
        }

        static void AppendCommaSeparatedFragments(ref FastShortStringBuilder stringBuilder, [NotNull] HtmlFragment[] fragments, int subIndent)
        {
            var isSubsequent = false;
            foreach (var fragment in fragments) {
                if (isSubsequent) {
                    stringBuilder.AppendText(",");
                }
                AppendNewline(ref stringBuilder);
                AppendIndent(ref stringBuilder, subIndent);
                AppendCSharpTo(ref stringBuilder, fragment, subIndent);

                isSubsequent = true;
            }
            AppendNewline(ref stringBuilder);
        }

        static void AppendNewline(ref FastShortStringBuilder stringBuilder) => stringBuilder.AppendText("\n");
        static void AppendIndent(ref FastShortStringBuilder stringBuilder, int indent) => stringBuilder.AppendText(new string(' ', indent));

        static bool AppendAttributesAsCSharp(ref FastShortStringBuilder stringBuilder, HtmlAttributes htmlAttributes, IReadOnlyDictionary<string, string> attributeMethodsByName, int indent)
        {
            if (htmlAttributes.Count == 0) {
                return false;
            }
            var hasRenderedAttribute = false;

            foreach (var attr in htmlAttributes) {
                if (attr.Value == null) {
                    continue;
                }
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
                    stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(attr.Name));
                    stringBuilder.AppendText(",");
                }
                stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(attr.Value));
                stringBuilder.AppendText(")");
            }
            return hasRenderedAttribute;
        }
    }
}
