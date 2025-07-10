using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Xunit.Abstractions;

namespace ProgressOnderwijsUtils.Tests;

public sealed class HtmlDslGenerator
{
    static readonly HttpClient client = new();
    static readonly SourceLocation here = SourceLocation.Here();

    static readonly Uri
        specUri = new("https://html.spec.whatwg.org/"),
        currentFilePath = new(here.FilePath),
        localCache = currentFilePath.Combine("html.spec.whatwg.org.cached"),
        LibHtmlDirectory = currentFilePath.Combine("../../src/ProgressOnderwijsUtils/Html/"),
        HtmlTagKinds_GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.HtmlTagKinds.Generated.cs"),
        HtmlTags_GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.HtmlTags.Generated.cs"),
        AttributeNameInterfaces_GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.AttributeNameInterfaces.Generated.cs"),
        AttributeConstructionMethods_GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.AttributeConstructionMethods.Generated.cs"),
        AttributeLookupTable_GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.AttributeLookupTable.Generated.cs");

    readonly ITestOutputHelper output;

    public HtmlDslGenerator(ITestOutputHelper output)
        => this.output = output;

    [SkippableFact]
    public async Task RegenerateHtmlTagCSharp()
    {
        if (Environment.GetEnvironmentVariable("APPVEYOR") != null) {
            throw new SkipException("For manual regeneration; won't work on the CI");
        }
        if (!localCache.RefersToExistingLocalFile() || new FileInfo(localCache.LocalPath).LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)) {
            output.WriteLine("Cache out of date; refreshing");
            var freshContent = await client.GetStringAsync(specUri);
            await File.WriteAllTextAsync(localCache.LocalPath, freshContent);
        }
        var content = await File.ReadAllTextAsync(localCache.LocalPath);
        var document = new HtmlParser().ParseDocument(content);

        var voidElements = (document.GetElementById("void-elements")?.ParentElement?.NextElementSibling?.TextContent.Split(',').Select(s => s.Trim()).ToArray()).AssertNotNull();
        var tableOfElements = document.QuerySelectorAll("h3#elements-3 + p + table").Single();

        var columns = tableOfElements.QuerySelectorAll("thead tr th").Select(el => el.TextContent.Trim()).ToArray();

        var elementNameColumnIndex = Array.IndexOf(columns, "Element");
        var attributesColumnIndex = Array.IndexOf(columns, "Attributes");
        var descriptionColumnIndex = Array.IndexOf(columns, "Description");
        var categoriesColumnIndex = Array.IndexOf(columns, "Categories");
        var parentsColumnIndex = columns.TakeWhile(s => !s.StartsWith("Parents", StringComparison.Ordinal)).Count();
        var childrenColumnIndex = Array.IndexOf(columns, "Children");

        PAssert.That(() => 0 <= elementNameColumnIndex && elementNameColumnIndex < attributesColumnIndex);

        var tableOfAttributes = document.QuerySelectorAll("h3#attributes-3 + p + table").Single();

        var globalAttributes =
            tableOfAttributes.QuerySelectorAll("tbody tr")
                .Where(tr => tr.QuerySelector("td")?.TextContent.Trim() == "HTML elements")
                .Select(tr => tr.QuerySelector("th").AssertNotNull().TextContent.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var booleanAttributes =
            tableOfAttributes.QuerySelectorAll("tbody tr").GroupBy(
                tr => tr.QuerySelector("th").AssertNotNull().TextContent.Trim(),
                tr => tr.QuerySelector("td > a[href='#boolean-attribute']")?.TextContent.Trim() == "Boolean attribute",
                (key, isBoolean) => (key, isBoolean: isBoolean.Distinct().ToArray() is [var unique,] ? unique : default(bool?))
            ).ToDictionary(o => o.key, o => o.isBoolean, StringComparer.OrdinalIgnoreCase);

        static string toClassName(string s)
            => s.Replace('-', '_');
        static string[] splitList(string list)
            => list.Split(';').Select(s => s.Trim().TrimEnd('*')).Where(s => s != "").ToArray();

        var elements = tableOfElements.QuerySelectorAll("tbody tr")
            .SelectMany(tableRow => {
                    var tds = tableRow.QuerySelectorAll("th,td");
                    var elementNames = tds[elementNameColumnIndex].TextContent.Split(',').Select(n => n.Trim());
                    var elementAnchor = tds[elementNameColumnIndex].QuerySelector("a")?.GetAttribute("href");
                    var description = tds[descriptionColumnIndex].TextContent;
                    var categories = splitList(tds[categoriesColumnIndex].TextContent);
                    var parents = splitList(tds[parentsColumnIndex].TextContent);
                    var children = splitList(tds[childrenColumnIndex].TextContent).Where(s => s != "empty").ToArray();
                    var elementLink = new Uri(specUri, elementAnchor).ToString();
                    var attributes = splitList(tds[attributesColumnIndex].TextContent)
                        .Where(s => s != "any" && s != "globals" && !s.Contains(" ")).ToArray();
                    return elementNames.Select(elementName => new {
                            elementName,
                            elementLink,
                            description,
                            attributes,
                            categories,
                            parents,
                            children,
                        }
                    );
                }
            )
            .Where(el => !el.elementName.Contains(" "))
            .GroupBy(el => el.elementName)
            .Select(elGroup =>
                new {
                    elementName = elGroup.Key,
                    csName = toClassName(elGroup.Key),
                    csUpperName = toClassName(elGroup.Key).ToUpperInvariant(),
                    elementMetaData =
                        new XElement(
                            "summary",
                            elGroup.SelectMany(el =>
                                new object[] {
                                    el.description + ". See: ",
                                    new XElement("a", new XAttribute("href", el.elementLink), el.elementLink),
                                    new XElement("br")
                                }
                            )
                        ),
                    attributes = elGroup.SelectMany(el => el.attributes).ToDistinctArray(),
                    categories = elGroup.SelectMany(el => el.categories).ToDistinctArray(),
                    parents = elGroup.SelectMany(el => el.parents).ToDistinctArray(),
                    children = elGroup.SelectMany(el => el.children).ToDistinctArray(),
                }
            ).ToArray();

        var specificAttributes = elements.SelectMany(el => el.attributes).ToDistinctArray();

        var elAttrInterfaces = specificAttributes
            .Select(attrName => $$"""
                public interface IHasAttr_{{toClassName(attrName)}} { }

                """
            );
        var elAttrExtensionMethods = globalAttributes.Concat(specificAttributes)
            .Select(AttrHelper).JoinStrings("");

        var elTagNameClasses = elements
            .Select(el =>
                voidElements.Contains(el.elementName)
                    ? $$"""

                        public struct {{el.csUpperName}} : IHtmlElement<{{el.csUpperName}}>{{el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}").JoinStrings("")}}
                        {
                            public string TagName => "{{el.elementName}}";
                            string IHtmlElement.TagStart => "<{{el.elementName}}";
                            string IHtmlElement.EndTag => "";
                            ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<{{el.elementName}}"u8;
                            ReadOnlySpan<byte> IHtmlElement.EndTagUtf8 => ""u8;
                            public bool ContainsUnescapedText => {{(el.elementName is "script" or "style" ? "true" : "false")}};
                            HtmlAttributes attrs;
                            {{el.csUpperName}} IHtmlElement<{{el.csUpperName}}>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new {{el.csUpperName}} { attrs = replacementAttributes };
                            HtmlAttributes IHtmlElement.Attributes => attrs;
                            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
                            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
                            public static implicit operator HtmlFragment({{el.csUpperName}} tag) => tag.AsFragment();
                            public static HtmlFragment operator +({{el.csUpperName}} unary) => unary;
                            public static HtmlFragment operator +({{el.csUpperName}} head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
                            public static HtmlFragment operator +(string head, {{el.csUpperName}} tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
                        }
                    """
                    : $$"""

                        public struct {{el.csUpperName}} : IHtmlElementAllowingContent<{{el.csUpperName}}>{{el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}").JoinStrings("")}}
                        {
                            public string TagName => "{{el.elementName}}";
                            string IHtmlElement.TagStart => "<{{el.elementName}}";
                            string IHtmlElement.EndTag => "</{{el.elementName}}>";
                            ReadOnlySpan<byte> IHtmlElement.TagStartUtf8 => "<{{el.elementName}}"u8;
                            ReadOnlySpan<byte> IHtmlElement.EndTagUtf8 => "</{{el.elementName}}>"u8;
                            public bool ContainsUnescapedText => {{(el.elementName is "script" or "style" ? "true" : "false")}};
                            HtmlAttributes attrs;
                            {{el.csUpperName}} IHtmlElement<{{el.csUpperName}}>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new {{el.csUpperName}} { attrs = replacementAttributes, children = children };
                            HtmlAttributes IHtmlElement.Attributes => attrs;
                            HtmlFragment children;
                            {{el.csUpperName}} IHtmlElementAllowingContent<{{el.csUpperName}}>.ReplaceContentWith(HtmlFragment replacementContents) => new {{el.csUpperName}} { attrs = attrs, children = replacementContents };
                            HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
                            IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
                            [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
                            public static implicit operator HtmlFragment({{el.csUpperName}} tag) => tag.AsFragment();
                            public static HtmlFragment operator +({{el.csUpperName}} unary) => unary;
                            public static HtmlFragment operator +({{el.csUpperName}} head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
                            public static HtmlFragment operator +(string head, {{el.csUpperName}} tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
                        }
                    """
            );

        var elFields = elements
            .Select(el => $"""

                {Regex.Replace(el.elementMetaData.ToString(SaveOptions.None), @"^|(?<=\n)", "    ///")}
                    public static readonly HtmlTagKinds.{el.csUpperName} _{el.csName} = new HtmlTagKinds.{el.csUpperName}();

                """
            );

        var diff = new[] {
            AssertFileExistsAndApproveContent(
                HtmlTagKinds_GeneratedOutputFilePath,
                $$"""
                #nullable enable
                using ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

                namespace ProgressOnderwijsUtils.Html;

                public static class HtmlTagKinds
                {{{elTagNameClasses.JoinStrings("")}}
                }

                """
            ),

            AssertFileExistsAndApproveContent(
                HtmlTags_GeneratedOutputFilePath,
                $$"""
                #nullable enable
                namespace ProgressOnderwijsUtils.Html;

                public static class Tags
                {{{elFields.JoinStrings("")}}}

                """
            ),

            AssertFileExistsAndApproveContent(
                AttributeNameInterfaces_GeneratedOutputFilePath,
                $$"""
                #nullable enable
                namespace ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

                {{elAttrInterfaces.JoinStrings("")}}
                """
            ),

            AssertFileExistsAndApproveContent(
                AttributeConstructionMethods_GeneratedOutputFilePath,
                $$"""
                #nullable enable
                using ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

                namespace ProgressOnderwijsUtils.Html;

                public static class AttributeConstructionMethods
                {{{elAttrExtensionMethods}}
                }

                """
            ),

            AssertFileExistsAndApproveContent(AttributeLookupTable_GeneratedOutputFilePath, GenerateAttributeLookupTable(elements.Select(el => (el.elementName, el.attributes)).ToArray())),
        }.WhereNotNull().ToArray();

        PAssert.That(() => diff.None());
        return;

        string GenerateAttributeLookupTable((string elementName, DistinctArray<string> attributes)[] elementsWithAttributes)
        {
            // Generate lookup entries for each element
            var elementLookups = elementsWithAttributes.Select(el => {
                    var elementAttributes = globalAttributes.Concat(el.attributes).ToDistinctArray();
                    var attributeEntries = elementAttributes.Select(attrName => {
                            var methodName = $"_{toClassName(attrName)}";
                            return $"[\"{attrName}\"] = \"{methodName}\"";
                        }
                    ).JoinStrings(",\n                ");

                    return "[\"" + el.elementName + "\"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {\n" +
                        $"                {attributeEntries}\n" +
                        "            },";
                }
            ).JoinStrings("\n            ");

            // Generate default attributes for unknown elements (global attributes only)
            var defaultAttributeEntries = globalAttributes.Select(attrName => {
                    var methodName = $"_{toClassName(attrName)}";
                    return $"[\"{attrName}\"] = \"{methodName}\"";
                }
            ).JoinStrings(",\n                ");

            return $$"""
                #nullable enable
                namespace ProgressOnderwijsUtils.Html;

                public static class AttributeLookupTable
                {
                    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ByTagName =
                        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase) {
                {{elementLookups}}
                        };

                    public static readonly IReadOnlyDictionary<string, string> DefaultAttributes =
                        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                            {{defaultAttributeEntries}}
                        };
                }

                """;
        }

        string AttrHelper(string attrName)
        {
            var obsoleteAttribute = attrName == "class" ? "\n    [Obsolete]" : "";
            var applicabilityTypeContraint = globalAttributes.Contains(attrName) switch {
                true => "",
                false => $", IHasAttr_{toClassName(attrName)}",
            };
            var isBoolean = booleanAttributes.GetValueOrDefault(attrName) ?? (attrName.StartsWith("on", StringComparison.OrdinalIgnoreCase) ? false : null);
            return isBoolean switch {
                null => throw new(attrName + " could not be determined to be a boolean attribute or not"),
                false => AttrExtensionMethod(attrName, applicabilityTypeContraint, obsoleteAttribute, ", string? attrValue", ", attrValue"),
                _ => AttrExtensionMethod(attrName, applicabilityTypeContraint, obsoleteAttribute, ", bool attrValue", ", attrValue ? \"\" : null")
                    + AttrExtensionMethod(attrName, applicabilityTypeContraint, obsoleteAttribute, "", ", \"\"")
                    + AttrExtensionMethod(attrName, applicabilityTypeContraint, obsoleteAttribute, ", string? attrValue", ", attrValue"),
            };
        }

        static string AttrExtensionMethod(string attrName, string applicabilityTypeContraint, string obsoleteAttribute, string attrValueParam, string attrValueExpr)
            => $$"""
                {{obsoleteAttribute}}
                    public static THtmlTag _{{toClassName(attrName)}}<THtmlTag>(this THtmlTag htmlTagExpr{{attrValueParam}})
                        where THtmlTag : struct{{applicabilityTypeContraint}}, IHtmlElement<THtmlTag>
                        => htmlTagExpr.Attribute("{{attrName}}"{{attrValueExpr}});
                """;
    }

    static string? AssertFileExistsAndApproveContent(Uri GeneratedOutputFilePath, string generatedCSharpContent)
    {
        if (!GeneratedOutputFilePath.RefersToExistingLocalFile()) {
            throw new($"Expected {GeneratedOutputFilePath.LocalPath} to already exist; has the repo-layout changed?");
        }

        return ApprovalTest.CreateForApprovedPath(GeneratedOutputFilePath.LocalPath).UpdateIfChangedFrom(generatedCSharpContent, out var diff)
            ? diff
            : null;
    }
}
