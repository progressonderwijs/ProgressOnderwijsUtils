using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{
    struct TagDescription
    {
        public string TagName { get; private set; }
        public bool IsSelfClosing { get; private set; }
        public string? FieldName { get; private set; }

        public bool IsPredefined
            => FieldName != null;

        public IHtmlElement EmptyValue { get; private set; }
        public IReadOnlyDictionary<string, string> AttributeMethodsByName { get; private set; }

        static readonly IReadOnlyDictionary<string, TagDescription> ByTagName =
            typeof(Tags).GetTypeInfo()
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(field => new { FieldName = field.Name, field.FieldType, FieldValue = (IHtmlElement)field.GetValue(null)! })
                .ToDictionary(
                    field => field.FieldValue.TagName,
                    field => new TagDescription {
                        TagName = field.FieldValue.TagName,
                        EmptyValue = field.FieldValue,
                        FieldName = field.FieldName,
                        IsSelfClosing = !(field.FieldValue is IHtmlElementAllowingContent),
                        AttributeMethodsByName = AttributeLookup(field.FieldType, field.FieldValue)
                    },
                    StringComparer.OrdinalIgnoreCase
                );

        [NotNull]
        static Dictionary<string, string> AttributeLookup(Type tagType, IHtmlElement emptyValue)
            => typeof(AttributeConstructionMethods)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => {
                    var typeArgument = mi.GetGenericArguments().Single();
                    return typeArgument.GetGenericParameterConstraints()
                        .All(constraint =>
                            constraint.IsAssignableFrom(tagType)
                            || constraint == typeof(IHtmlElement<>).MakeGenericType(typeArgument) && typeof(IHtmlElement<>).MakeGenericType(tagType).IsAssignableFrom(tagType));
                })
                .ToDictionary(
                    method => ((IHtmlElement)method.MakeGenericMethod(tagType).Invoke(null, new[] { emptyValue, (object)"" })!).Attributes.Last().Name,
                    method => method.Name,
                    StringComparer.OrdinalIgnoreCase);

        public static TagDescription LookupTag([NotNull] string tagName)
            => ByTagName.TryGetValue(tagName, out var desc)
                ? desc
                : new TagDescription {
                    TagName = tagName,
                    FieldName = null,
                    IsSelfClosing = false,
                    AttributeMethodsByName = DefaultAttributes,
                };

        static readonly IReadOnlyDictionary<string, string> DefaultAttributes = AttributeLookup(typeof(CustomHtmlElement), new CustomHtmlElement("unknown", null, null));
    }
}
