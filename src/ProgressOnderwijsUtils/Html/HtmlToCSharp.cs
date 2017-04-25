using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpressionToCodeLib;

namespace ProgressOnderwijsUtils.Html
{
    public static class HtmlToCSharp
    {
        public static string ToCSharp(this HtmlFragment fragment)
        {
            var builder = FastShortStringBuilder.Create();
            AppendCSharpTo(ref builder, fragment, 0);
            return builder.Value;
        }

        class TagDescription
        {
            public string FieldName;
            public IReadOnlyDictionary<string, string> AttributeMethodsByName;

            public static readonly IReadOnlyDictionary<string, TagDescription> ByTagName =
                typeof(Tags).GetTypeInfo()
                    .GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Select(field => new { FieldName = field.Name, field.FieldType, FieldValue = (IHtmlTag)field.GetValue(null) })
                    .ToDictionary(
                        field => field.FieldValue.TagName,
                        field => new TagDescription {
                            FieldName = field.FieldName,
                            AttributeMethodsByName = AttributeLookup(field.FieldType, field.FieldValue)
                        },
                        StringComparer.OrdinalIgnoreCase
                    );

            static Dictionary<string, string> AttributeLookup(Type tagType, IHtmlTag emptyValue)
            {
                return typeof(AttributeConstructionMethods)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(mi => mi.GetGenericArguments().Single().GetGenericParameterConstraints().All(constraint => constraint.IsAssignableFrom(tagType)))
                    .ToDictionary(
                        method => ((IHtmlTag)method.MakeGenericMethod(tagType).Invoke(null, new[] { emptyValue, (object)"" })).Attributes.Last().Name,
                        method => method.Name,
                        StringComparer.OrdinalIgnoreCase);
            }

            public static readonly IReadOnlyDictionary<string, string> DefaultAttributes = AttributeLookup(typeof(HtmlElement), new HtmlElement("unknown", null, null));
        }

        static void AppendCSharpTo(ref FastShortStringBuilder stringBuilder, HtmlFragment fragment, int indent)
        {
            if (fragment.Content is string stringContent) {
                stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(stringContent));
            } else if (fragment.Content is IHtmlTag htmlTag) {
                bool wereAttributesRendered;
                if (TagDescription.ByTagName.TryGetValue(htmlTag.TagName, out var description)) {
                    stringBuilder.AppendText(description.FieldName);
                    wereAttributesRendered = AppendAttributesAsCSharp(ref stringBuilder, htmlTag.Attributes, description.AttributeMethodsByName, indent);
                } else {
                    stringBuilder.AppendText("new HtmlElement(");
                    stringBuilder.AppendText(ObjectToCode.PlainObjectToCode(htmlTag.TagName));
                    stringBuilder.AppendText(")");
                    wereAttributesRendered = AppendAttributesAsCSharp(ref stringBuilder, htmlTag.Attributes, TagDescription.DefaultAttributes, indent);
                }
                if (htmlTag is IHtmlTagAllowingContent withContent && (withContent.Contents?.Length ?? 0) > 0) {
                    var subIndent = wereAttributesRendered ? indent + 8 : indent + 4;
                    if (wereAttributesRendered) {
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

        static void AppendCommaSeparatedFragments(ref FastShortStringBuilder stringBuilder, HtmlFragment[] fragments, int subIndent)
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

        static bool AppendAttributesAsCSharp(ref FastShortStringBuilder stringBuilder, HtmlAttribute[] htmlAttributes, IReadOnlyDictionary<string, string> attributeMethodsByName, int indent)
        {
            if (htmlAttributes == null) {
                return false;
            }
            var hasRenderedAttribute = false;

            foreach (var attr in htmlAttributes) {
                if (attr.Value == null) {
                    continue;
                }
                if (hasRenderedAttribute) {
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
                AppendNewline(ref stringBuilder);
            }
            return hasRenderedAttribute;
        }
    }
}
