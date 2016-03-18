<Query Kind="Statements">
  <Reference Relative="..\Build\bin\ExpressionToCodeLib.dll">C:\vcs\progress\Build\bin\ExpressionToCodeLib.dll</Reference>
  <Reference Relative="..\Build\bin\Newtonsoft.Json.dll">C:\vcs\progress\Build\bin\Newtonsoft.Json.dll</Reference>
  <Reference Relative="..\Build\bin\nunit.framework.dll">C:\vcs\progress\Build\bin\nunit.framework.dll</Reference>
  <Reference Relative="..\Build\bin\Packager.exe">C:\vcs\progress\Build\bin\Packager.exe</Reference>
  <Reference Relative="..\Build\bin\Progress.Business.dll">C:\vcs\progress\Build\bin\Progress.Business.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.Business.Test.dll">C:\vcs\progress\Build\bin\Progress.Business.Test.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.Gadgets.dll">C:\vcs\progress\Build\bin\Progress.Gadgets.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.Taken.exe">C:\vcs\progress\Build\bin\Progress.Taken.exe</Reference>
  <Reference Relative="..\Build\bin\Progress.Templates.dll">C:\vcs\progress\Build\bin\Progress.Templates.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.Test.dll">C:\vcs\progress\Build\bin\Progress.Test.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.Tools.dll">C:\vcs\progress\Build\bin\Progress.Tools.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.WebApp.dll">C:\vcs\progress\Build\bin\Progress.WebApp.dll</Reference>
  <Reference Relative="..\Build\bin\Progress.WebFramework.dll">C:\vcs\progress\Build\bin\Progress.WebFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.WebRequest.dll</Reference>
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference Prerelease="true">morelinq</NuGetReference>
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
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>NUnit.Framework</Namespace>
  <Namespace>Progress.Business</Namespace>
  <Namespace>Progress.Business.AppVersion</Namespace>
  <Namespace>Progress.Business.Data</Namespace>
  <Namespace>Progress.Business.DomainUnits</Namespace>
  <Namespace>Progress.Business.Test</Namespace>
  <Namespace>Progress.Packager</Namespace>
  <Namespace>Progress.Test.CodeStyle</Namespace>
  <Namespace>Progress.Test.GenericTests</Namespace>
  <Namespace>Progress.Tools</Namespace>
  <Namespace>Progress.WebApp</Namespace>
  <Namespace>Progress.WebApp.Base</Namespace>
  <Namespace>Progress.WebFramework</Namespace>
  <Namespace>static Progress.Tools.SafeSql</Namespace>
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
	var elementsSectionHeader = 
		document.QuerySelectorAll("h3")
			.Single(el => el.TextContent == "Elements");
	var tableOfElements = 
		MoreEnumerable.Generate(elementsSectionHeader, el => el.NextElementSibling)
			.First(el => "table".Equals(el.TagName, StringComparison.OrdinalIgnoreCase));
	
	var columns = tableOfElements.QuerySelectorAll("thead tr th").Select(el=>el.TextContent.Trim()).ToArray();
	
	var elementNameColumnIndex = columns.IndexOf("Element");
	var attributesColumnIndex = columns.IndexOf("Attributes");
	var descriptionColumnIndex = columns.IndexOf("Description");
	PAssert.That(()=>0 <= elementNameColumnIndex && elementNameColumnIndex < attributesColumnIndex);

	var globalAttributes =
		document.QuerySelector("#global-attributes ~ ul")
			.QuerySelectorAll("li")
			.Select(li => li.TextContent.Trim())
			.ToArray();

	var toClassName = Utils.F((string s) => s.Replace('-', '_'));

	var elements = tableOfElements.QuerySelectorAll("tbody tr")
		.SelectMany(tableRow =>
		{
			var tds = tableRow.QuerySelectorAll("th,td");
			var elementNames = tds[elementNameColumnIndex].TextContent.Split(',').Select(n=>n.Trim());
			var elementAnchor = tds[elementNameColumnIndex].QuerySelector("a")?.GetAttribute("href");
			var description = tds[descriptionColumnIndex].TextContent;
			var elementLink = new Uri(specUri, elementAnchor).ToString();
			var attributes = tds[attributesColumnIndex].TextContent
				.Split(';')
				.Select(s => s.Trim())
				.Where(s=>!string.IsNullOrEmpty(s) && s!="any*"&& s!="globals" && !s.Contains(" "))
				.Select(s=>s.TrimEnd('*'))
				.ToArray();
			return elementNames.Select(elementName=> new {
				elementName, 
				elementLink,
				description,
				attributes 
				}
			);
        })
		.GroupBy(el => el.elementName)
		.Select(elGroup =>
			 new { 
			 	elementName=elGroup.Key, 
				csName= toClassName(elGroup.Key), 
				elementMetaData=
					new XElement("summary",
					elGroup.SelectMany(el =>
						new object[]{
							el.description+". See: ",
							new XElement("a", new XAttribute("href", el.elementLink), el.elementLink),
							new XElement("br")
						}
					)
					), 
				attributes = elGroup.SelectMany(el=>el.attributes).Distinct()
			}
		).ToArray();











	var globalAttributeExtensionMethods = globalAttributes
		.Select(attrName => $@"
        public static TExpression _{toClassName(attrName)}<TExpression>(this TExpression htmlTagExpr, string attrValue)
            where TExpression : struct, IFluentHtmlTagExpression<TExpression>
            => htmlTagExpr.Attribute(""{attrName}"", attrValue);"
);


	var specificAttributes = elements.SelectMany(el => el.attributes).Distinct();
	
	var elAttrInterfaces = specificAttributes
		.Select(attrName => $@"
        public interface IHasAttr_{toClassName(attrName)} {{ }}"
);
	var elAttrExtensionMethods = specificAttributes
		.Select(attrName => $@"
        public static HtmlStartTag<TTagType> _{toClassName(attrName)}<TTagType>(this HtmlStartTag<TTagType> htmlTagExpr, string attrValue)
            where TTagType : struct, IHasAttr_{toClassName(attrName)}, IHtmlTagName
            => htmlTagExpr.Attribute(""{attrName}"", attrValue);"
);

	var attrNamesClass = $@"
    using AttributeNameInterfaces;
    namespace AttributeNameInterfaces
    {{{elAttrInterfaces.JoinStrings()}
    }}
";

	var attrExtensionMethodsClass = $@"
    public static class AttributeConstructionMethods
    {{{globalAttributeExtensionMethods.JoinStrings()}{elAttrExtensionMethods.JoinStrings()}
    }}
";

	var elTagNameClasses = elements
		.Select(el => $@"
        public struct {el.csName.ToUpper(CultureInfo.InvariantCulture)} : IHtmlTagName{
	el.attributes.Select(attrName => $", IHasAttr_{toClassName(attrName)}").JoinStrings()
			} {{ public string TagName => ""{el.elementName}""; }}"
);

	var tagNamesClass = $@"
    public static class TagNames
    {{{elTagNameClasses.JoinStrings()}
    }}
";

	var elFields = elements
		.Select(el => $@"

{Regex.Replace(el.elementMetaData.ToString(SaveOptions.None),@"^|(?<=\n)","        ///")}
		public static readonly HtmlStartTag<TagNames.{el.csName.ToUpper(CultureInfo.InvariantCulture)}> _{el.csName} = new HtmlStartTag<TagNames.{el.csName.ToUpper(CultureInfo.InvariantCulture}>();"
);
	var tagsClass = $@"
	public static class Tags
    {{{elFields.JoinStrings()}
    }}";

	$@"namespace Progress.Tools.Html
{{{(attrNamesClass + attrExtensionMethodsClass + tagNamesClass + tagsClass)}
}}
".Dump();
//	globalAttributes.Dump();
//	elements.Where(el => el.attributes.Any()).Dump();
}