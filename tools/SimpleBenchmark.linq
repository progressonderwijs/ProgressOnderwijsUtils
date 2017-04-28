<Query Kind="Statements">
  <Reference Relative="..\src\ProgressOnderwijsUtils\bin\Release\net46\ProgressOnderwijsUtils.dll">C:\VCS\external\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Release\net46\ProgressOnderwijsUtils.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.WebRequest.dll</Reference>
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference Prerelease="true">ExpressionToCodeLib</NuGetReference>
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
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Html</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Net.Cache</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var specUri = new Uri("https://en.wikipedia.org/wiki/HTML5");

var document = 
    new HttpClient(new WebRequestHandler {
        CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable),
    }).Using(client => new HtmlParser().Parse(client.GetStringAsync(specUri).Result));
var v0 = document.DocumentElement.OuterHtml;
HtmlFragment convert(INode node)
{
    if (node is IElement el)
        return new HtmlElement(
            el.TagName.ToLowerInvariant(), 
            el.Attributes.Select(attr => new HtmlAttribute { Name = attr.Name, Value = attr.Value}).ToArray(),
            el.ChildNodes.Select(convert).Where(kid=>!kid.IsEmpty).ToArray()
             );
    else if(node is IText text)
        return text.TextContent;
    else if(node.HasChildNodes)
        return node.ChildNodes.Select(convert).Where(kid=>!kid.IsEmpty).ToArray().WrapInHtmlFragment();
    else return HtmlFragment.Empty;            
}
var v1= convert(document).SerializeToStringWithoutDoctype();
//v1.Dump();
var treeWalker = document.CreateTreeWalker(document.DocumentElement);
var comments = MoreEnumerable
    .GenerateByIndex(_=> treeWalker.ToNext())
    .TakeWhile(n=>n!=null)
    .Where(n=>n is IComment)
    .ToArray();
foreach(var comment in comments)
{
    comment.Parent.RemoveChild(comment);
}
var v2 = convert(document).SerializeToStringWithoutDoctype();

(v1==v2).Dump();

var v3 = document.DocumentElement.OuterHtml;
(v1==v3).Dump();

File.WriteAllText("v0.html", v0);
File.WriteAllText("v1.html",v1);
File.WriteAllText("v2.html",v2);
File.WriteAllText("v3.html", v3);
File.WriteAllText("v1.cs", convert(document).ToCSharp());

double Time(Action action)
{
    var t = double.PositiveInfinity;
    for (int i = 0; i < 10; i++)
    {
        var sw = Stopwatch.StartNew();
        for (int j = 0; j < 1000; j++)
        {
            action();
        }
        var el =sw.Elapsed.TotalSeconds;
        t= Math.Min(t,el);
    }
    return t;
}
object.ReferenceEquals(document.DocumentElement.OuterHtml,document.DocumentElement.OuterHtml).Dump();
var hf = convert(document);

(int,int,int) GC()
=>     (System.GC.CollectionCount(0), System.GC.CollectionCount(1),System.GC.CollectionCount(2));

(int,int,int) delta( (int,int,int)x, (int,int,int) y) 
    =>(x.Item1-y.Item1,x.Item2-y.Item2,x.Item3-y.Item3);

var a= GC();
Time(() => hf.SerializeToStringWithoutDoctype()).Dump();
var b= GC();
//Time(() => { var x = document.DocumentElement.OuterHtml;}).Dump();
//var c= GC();

delta(b,a).Dump();
//delta(c,b).Dump();
 document.DocumentElement.OuterHtml.Length.Dump();
hf.SerializeToStringWithoutDoctype().Length.Dump();

