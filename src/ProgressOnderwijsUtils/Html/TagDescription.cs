namespace ProgressOnderwijsUtils.Html;

struct TagDescription
{
    public string TagName { get; private set; }
    public bool IsSelfClosing { get; private set; }
    public string? FieldName { get; private set; }

    public bool IsPredefined
        => FieldName != null;

    public IHtmlElement? EmptyValue { get; private set; }
    public IReadOnlyDictionary<string, string> AttributeMethodsByName { get; private set; }

    static readonly IReadOnlyDictionary<string, TagDescription> ByTagName =
        typeof(Tags).GetTypeInfo()
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(field => new { FieldName = field.Name, field.FieldType, FieldValue = (IHtmlElement)field.GetValue(null)!, })
            .ToDictionary(
                field => field.FieldValue.TagName.AssertNotNull(),
                field => new TagDescription {
                    TagName = field.FieldValue.TagName,
                    EmptyValue = field.FieldValue,
                    FieldName = field.FieldName,
                    IsSelfClosing = field.FieldValue is not IHtmlElementAllowingContent,
                    AttributeMethodsByName = AttributeLookup(field.FieldType, field.FieldValue),
                },
                StringComparer.OrdinalIgnoreCase
            );

    static Dictionary<string, string> AttributeLookup(Type tagType, IHtmlElement emptyValue)
        => typeof(AttributeConstructionMethods)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(
                mi => {
                    var typeArgument = mi.GetGenericArguments().Single();
                    return typeArgument.GetGenericParameterConstraints()
                        .All(
                            constraint =>
                                constraint.IsAssignableFrom(tagType)
                                || constraint == typeof(IHtmlElement<>).MakeGenericType(typeArgument) && typeof(IHtmlElement<>).MakeGenericType(tagType).IsAssignableFrom(tagType)
                        );
                }
            )
            .ToDictionary(
                method => ((IHtmlElement)method.MakeGenericMethod(tagType).Invoke(null, new[] { emptyValue, (object)"", })!).Attributes[^1].Name,
                method => method.Name,
                StringComparer.OrdinalIgnoreCase
            );

    public static TagDescription LookupTag(string tagName)
        => ByTagName.TryGetValue(tagName, out var desc)
            ? desc
            : new() {
                TagName = tagName,
                FieldName = null,
                IsSelfClosing = false,
                AttributeMethodsByName = DefaultAttributes,
            };

    static readonly IReadOnlyDictionary<string, string> DefaultAttributes = AttributeLookup(typeof(CustomHtmlElement), new CustomHtmlElement("unknown", null, null));
}
