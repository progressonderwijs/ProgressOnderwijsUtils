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
        GeneratedOutputFilePath = LibHtmlDirectory.Combine("HtmlSpec.Generated.cs");

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
                .ToArray();

        string toClassName(string s)
            => s.Replace('-', '_');
        string[] splitList(string list)
            => list.Split(';').Select(s => s.Trim().TrimEnd('*')).Where(s => s != "").ToArray();

        var elements = tableOfElements.QuerySelectorAll("tbody tr")
            .SelectMany(
                tableRow => {
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
                    return elementNames.Select(
                        elementName => new {
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
            .Select(
                elGroup =>
                    new {
                        elementName = elGroup.Key,
                        csName = toClassName(elGroup.Key),
                        csUpperName = toClassName(elGroup.Key).ToUpperInvariant(),
                        elementMetaData =
                            new XElement(
                                "summary",
                                elGroup.SelectMany(
                                    el =>
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

        var globalAttributeExtensionMethods = globalAttributes
            .Select(
                attrName => $"""
                    {(attrName == "class" ? "\n    [Obsolete]" : "")}
                        public static THtmlTag _{toClassName(attrName)}<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
                            where THtmlTag : struct, IHtmlElement<THtmlTag>
                            => htmlTagExpr.Attribute("{attrName}", attrValue);
                    """
            );

        var specificAttributes = elements.SelectMany(el => el.attributes).ToDistinctArray();

        var elAttrInterfaces = specificAttributes
            .Select(
                attrName => $$"""

                        public interface IHasAttr_{{toClassName(attrName)}} { }
                    """
            );
        var elAttrExtensionMethods = specificAttributes
            .Select(
                attrName => $"""

                        public static THtmlTag _{toClassName(attrName)}<THtmlTag>(this THtmlTag htmlTagExpr, string? attrValue)
                            where THtmlTag : struct, IHasAttr_{toClassName(attrName)}, IHtmlElement<THtmlTag>
                            => htmlTagExpr.Attribute("{attrName}", attrValue);
                    """
            );

        var attrNamesClass = $$"""

            public static class AttributeNameInterfaces
            {{{elAttrInterfaces.JoinStrings("")}}
            }
            """;

        var attrExtensionMethodsClass = $$"""

            public static class AttributeConstructionMethods
            {{{globalAttributeExtensionMethods.JoinStrings("")}}{{elAttrExtensionMethods.JoinStrings("")}}
            }
            """;

        var elTagNameClasses = elements
            .Select(
                el =>
                    voidElements.Contains(el.elementName)
                        ? $$"""
                        
                                public struct {{el.csUpperName}} : IHtmlElement<{{el.csUpperName}}>{{el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}").JoinStrings("")}}
                                {
                                    public string TagName => "{{el.elementName}}";
                                    string IHtmlElement.TagStart => "<{{el.elementName}}";
                                    string IHtmlElement.EndTag => "";
                                    HtmlAttributes attrs;
                                    {{el.csUpperName}} IHtmlElement<{{el.csUpperName}}>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new {{el.csUpperName}} { attrs = replacementAttributes };
                                    HtmlAttributes IHtmlElement.Attributes => attrs;
                                    IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterEmptyElement(this);
                                    [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
                                    public static implicit operator HtmlFragment({{el.csUpperName}} tag) => tag.AsFragment();
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
                                    HtmlAttributes attrs;
                                    {{el.csUpperName}} IHtmlElement<{{el.csUpperName}}>.ReplaceAttributesWith(HtmlAttributes replacementAttributes) => new {{el.csUpperName}} { attrs = replacementAttributes, children = children };
                                    HtmlAttributes IHtmlElement.Attributes => attrs;
                                    HtmlFragment children;
                                    {{el.csUpperName}} IHtmlElementAllowingContent<{{el.csUpperName}}>.ReplaceContentWith(HtmlFragment replacementContents) => new {{el.csUpperName}} { attrs = attrs, children = replacementContents };
                                    HtmlFragment IHtmlElementAllowingContent.GetContent() => children;
                                    IHtmlElement IHtmlElement.ApplyAlteration<THtmlTagAlteration>(THtmlTagAlteration change) => change.AlterElementAllowingContent(this);
                                    [Pure] public HtmlFragment AsFragment() => HtmlFragment.Element(this);
                                    public static implicit operator HtmlFragment({{el.csUpperName}} tag) => tag.AsFragment();
                                    public static HtmlFragment operator +({{el.csUpperName}} head, HtmlFragment tail) => HtmlFragment.Fragment(HtmlFragment.Element(head), tail);
                                    public static HtmlFragment operator +(string head, {{el.csUpperName}} tail) => HtmlFragment.Fragment(head, HtmlFragment.Element(tail));
                                }
                            """
            );

        var tagNamesClass = $$"""

            public static class HtmlTagKinds
            {{{elTagNameClasses.JoinStrings("")}}
            }
            """;

        var elFields = elements
            .Select(
                el => $"""


                    {Regex.Replace(el.elementMetaData.ToString(SaveOptions.None), @"^|(?<=\n)", "    ///")}
                        public static readonly HtmlTagKinds.{el.csUpperName} _{el.csName} = new HtmlTagKinds.{el.csUpperName}();
                    """
            );
        var tagsClass = $$"""

            public static class Tags
            {{{elFields.JoinStrings("")}}
            }
            """;

        var generatedCSharpContent = $$"""
                #nullable enable
                using static ProgressOnderwijsUtils.Html.AttributeNameInterfaces;

                namespace ProgressOnderwijsUtils.Html;
                {{tagNamesClass}}
                {{tagsClass}}
                {{attrNamesClass}}
                {{attrExtensionMethodsClass}}

                """;

        output.WriteLine(generatedCSharpContent);
        AssertFileExistsAndApproveContent(GeneratedOutputFilePath, generatedCSharpContent);
    }

    static void AssertFileExistsAndApproveContent(Uri GeneratedOutputFilePath, string generatedCSharpContent)
    {
        if (!GeneratedOutputFilePath.RefersToExistingLocalFile()) {
            throw new($"Expected {GeneratedOutputFilePath.LocalPath} to already exist; has the repo-layout changed?");
        }

        ApprovalTest.CreateForApprovedPath(GeneratedOutputFilePath.LocalPath).AssertUnchangedAndSave(generatedCSharpContent);
    }
}
