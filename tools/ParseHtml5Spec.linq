<Query Kind="Statements">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.WebRequest.dll</Reference>
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>ExpressionToCodeLib</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <Namespace>AngleSharp</Namespace>
  <Namespace>AngleSharp.Attributes</Namespace>
  <Namespace>AngleSharp.Css</Namespace>
  <Namespace>AngleSharp.Css.Values</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>AngleSharp.Dom.Css</Namespace>
  <Namespace>AngleSharp.Dom.Html</Namespace>
  <Namespace>AngleSharp.Html</Namespace>
  <Namespace>AngleSharp.Parser.Html</Namespace>
  <Namespace>ExpressionToCodeLib</Namespace>
  <Namespace>MoreLinq</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Net.Cache</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var specUri = new Uri("https://html.spec.whatwg.org/");

using (var client = new HttpClient(new WebRequestHandler {
    CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable),
})) {
    var content = await client.GetStringAsync(specUri);
    var document = new HtmlParser().Parse(content);
    var tableOfElements = document.QuerySelectorAll("h3#elements-3 + p + table").Single();
    
    var columns = tableOfElements.QuerySelectorAll("thead tr th").Select(el=>el.TextContent.Trim()).ToArray();
    
    var elementNameColumnIndex = Array.IndexOf(columns, "Element");
    var attributesColumnIndex = Array.IndexOf(columns, "Attributes");
    var descriptionColumnIndex = Array.IndexOf(columns, "Description");
    var categoriesColumnIndex = Array.IndexOf(columns, "Categories");
    var parentsColumnIndex = columns.TakeWhile(s => !s.StartsWith("Parents")).Count();
    var childrenColumnIndex = Array.IndexOf(columns, "Children");

    PAssert.That(()=>0 <= elementNameColumnIndex && elementNameColumnIndex < attributesColumnIndex);

    var tableOfAttributes = document.QuerySelectorAll("h3#attributes-3 + p + table").Single();

    var globalAttributes =
        tableOfAttributes.QuerySelectorAll("tbody tr")
            .Where(tr=>tr.QuerySelector("td").TextContent.Trim() == "HTML elements")
            .Select(tr=>tr.QuerySelector("th").TextContent.Trim())
            .ToArray();

    string toClassName(string s) => s.Replace('-', '_');
    string[] splitList(string list) => list.Split(';').Select(s=>s.Trim().TrimEnd('*')).Where(s=>s!="").ToArray();

    var elements = tableOfElements.QuerySelectorAll("tbody tr")
        .SelectMany(tableRow =>
        {
            var tds = tableRow.QuerySelectorAll("th,td");
            var elementNames = tds[elementNameColumnIndex].TextContent.Split(',').Select(n=>n.Trim());
            var elementAnchor = tds[elementNameColumnIndex].QuerySelector("a")?.GetAttribute("href");
            var description = tds[descriptionColumnIndex].TextContent;
            var categories = splitList(tds[categoriesColumnIndex].TextContent);
            var parents = splitList(tds[parentsColumnIndex].TextContent);
            var children = splitList(tds[childrenColumnIndex].TextContent).Where(s=>s!="empty").ToArray();
            var elementLink = new Uri(specUri, elementAnchor).ToString();
            var attributes = splitList(tds[attributesColumnIndex].TextContent)
                .Where(s=> s!="any" && s!= "globals" && !s.Contains(" ")).ToArray();
            return elementNames.Select(elementName=> new {
                elementName, 
                elementLink,
                description,
                attributes,
                categories,
                parents,
                children,
                }
            );
        })
        .Where(el=>!el.elementName.Contains(" "))
        .GroupBy(el => el.elementName)
        .Select(elGroup =>
            new
            {
                elementName = elGroup.Key,
                csName = toClassName(elGroup.Key),
                csUpperName = toClassName(elGroup.Key).ToUpperInvariant(),
                elementMetaData =
                new XElement("summary",
                    elGroup.SelectMany(el =>
                        new object[]{
                            el.description+". See: ",
                            new XElement("a", new XAttribute("href", el.elementLink), el.elementLink),
                            new XElement("br")
                        }
                    )
                ),
                attributes = elGroup.SelectMany(el => el.attributes).Distinct().ToArray(),
                categories = elGroup.SelectMany(el => el.categories).Distinct().ToArray(),
                parents = elGroup.SelectMany(el => el.parents).Distinct().ToArray(),
                children = elGroup.SelectMany(el => el.children).Distinct().ToArray(),
            }
        ).ToArray();

    var globalAttributeExtensionMethods = globalAttributes
        .Select(attrName => $@"
        public static THtmlTag _{toClassName(attrName)}<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute(""{attrName}"", attrValue);"
);


    var specificAttributes = elements.SelectMany(el => el.attributes).Distinct();
    
    var elAttrInterfaces = specificAttributes
        .Select(attrName => $@"
        public interface IHasAttr_{toClassName(attrName)} {{ }}"
);
    var elAttrExtensionMethods = specificAttributes
        .Select(attrName => $@"
        public static THtmlTag _{toClassName(attrName)}<THtmlTag>(this THtmlTag htmlTagExpr, string attrValue)
            where THtmlTag : struct, IHasAttr_{toClassName(attrName)}, IHtmlTag<THtmlTag>
            => htmlTagExpr.Attribute(""{attrName}"", attrValue);"
);

    var attrNamesClass = $@"
    namespace AttributeNameInterfaces
    {{{string.Join("",elAttrInterfaces)}
    }}";

    var attrExtensionMethodsClass = $@"
    public static class AttributeConstructionMethods
    {{{string.Join("",globalAttributeExtensionMethods)}{string.Join("",elAttrExtensionMethods)}
    }}";

    var elTagNameClasses = elements
        .Select(el =>
        el.children.Length == 0 
        ? $@"
        public struct {el.csUpperName} : IHtmlTag<{el.csUpperName}>{
            string.Join("", el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}"))
            }
        {{
            public string TagName => ""{el.elementName}"";
            string IHtmlTag.TagStart => ""<{el.elementName}"";
            string IHtmlTag.EndTag => """";
            HtmlAttributes attrs;
            {el.csUpperName} IHtmlTag<{el.csUpperName}>.WithAttributes(HtmlAttributes replacementAttributes) => new {el.csUpperName} {{ attrs = replacementAttributes }};
            HtmlAttributes IHtmlTag.Attributes => attrs;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeEmpty(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment({el.csUpperName} tag) => tag.AsFragment();
        }}"

        : $@"
        public struct {el.csUpperName} : IHtmlTagAllowingContent<{el.csUpperName}>{
            string.Join("", el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}"))
            }
        {{
            public string TagName => ""{el.elementName}"";
            string IHtmlTag.TagStart => ""<{el.elementName}"";
            string IHtmlTag.EndTag => ""</{el.elementName}>"";
            HtmlAttributes attrs;
            {el.csUpperName} IHtmlTag<{el.csUpperName}>.WithAttributes(HtmlAttributes replacementAttributes) => new {el.csUpperName} {{ attrs = replacementAttributes, children = children }};
            HtmlAttributes IHtmlTag.Attributes => attrs;
            HtmlFragment[] children;
            {el.csUpperName} IHtmlTagAllowingContent<{el.csUpperName}>.WithContents(HtmlFragment[] replacementContents) => new {el.csUpperName} {{ attrs = attrs, children = replacementContents }};
            HtmlFragment[] IHtmlTagAllowingContent.Contents => children;
            IHtmlTag IHtmlTag.ApplyChange<THtmlTagAlteration>(THtmlTagAlteration change) => change.ChangeWithContent(this);
            [Pure] public HtmlFragment AsFragment() => HtmlFragment.HtmlElement(this);
            public static implicit operator HtmlFragment({el.csUpperName} tag) => tag.AsFragment();
        }}"
);

    var tagNamesClass = $@"
    public static class HtmlTagKinds
    {{{string.Join("",elTagNameClasses)}
    }}";

    var elFields = elements
        .Select(el => $@"

{Regex.Replace(el.elementMetaData.ToString(SaveOptions.None),@"^|(?<=\n)","        ///")}
        public static readonly HtmlTagKinds.{el.csUpperName} _{el.csName} = new HtmlTagKinds.{el.csUpperName}();"
);
    var tagsClass = $@"
    public static class Tags
    {{{string.Join("",elFields)}
    }}";

    $@"using JetBrains.Annotations;

namespace ProgressOnderwijsUtils.Html
{{
    using AttributeNameInterfaces;
{tagNamesClass}
{tagsClass}
{attrNamesClass}
{attrExtensionMethodsClass}
}}".Dump();
}