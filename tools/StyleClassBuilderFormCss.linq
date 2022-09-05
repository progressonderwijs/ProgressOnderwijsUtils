<Query Kind="Program">
  <Reference>C:\Users\PRGMA01\GitHub\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll</Reference>
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>AngleSharp.Css</NuGetReference>
  <Namespace>AngleSharp</Namespace>
  <Namespace>AngleSharp.Css</Namespace>
  <Namespace>AngleSharp.Css.Parser</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>ProgressOnderwijsUtils.Html</Namespace>
  <Namespace>static ProgressOnderwijsUtils.Html.Tags</Namespace>
  <Namespace>static ProgressOnderwijsUtils.SafeSql</Namespace>
  <Namespace>AngleSharp.Css.Dom</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

void Main()
{
	var totalClasses = new HashSet<string>();
	var pathTo = @"C:/Users/PRGMA01/Github/"; //Change to the correct path to the folder
	var folderPath = @"progress/src/Progress.ClientApp/Progress.ClientApp/";
	var files = Directory.GetFiles(pathTo + folderPath, "*.css", SearchOption.AllDirectories);
	foreach (var filename in files)
	{
		string sheet = System.IO.File.ReadAllText(filename);

		var css = new AngleSharp.Css.Parser.CssParser().ParseStyleSheet(sheet);
		var fileClasses = new HashSet<string>();
		
		foreach (var rule in css.Rules.OfType<ICssStyleRule>())
		{
			if (rule.Selector != null)
			{
				var visitor = new Visitor();
				rule.Selector.Accept(visitor);
				foreach (var name in visitor.className)
				{
					if (name != null && !totalClasses.Contains(name))
					{
						totalClasses.Add(name);
						fileClasses.Add(name);
					}
				}
			}
		}

		if (fileClasses.Count() > 0) { 
			var comment = "\n//" + Path.GetFileName(filename);  
			comment.Dump();
		}

		foreach (string n in fileClasses)
		{
			if (n != string.Empty)
			{
				FormatCLine(n).Dump();
			}
		}

	}
}

public static string ReformatClassName(string name)
	{
		string[] chars = { "__", "--", "-", "_" };

		var reformed = name.Split(chars, System.StringSplitOptions.TrimEntries).ToList();
		reformed = reformed.ConvertAll(w => w.Substring(0, 1).ToUpper() + w.Substring(1));

		return reformed.JoinStrings("_");
	}

public static string FormatCLine(string name) {
	return "public static readonly CssClass " + ReformatClassName(name) + " = new(\"" + name + "\");";
}

class Visitor : ISelectorVisitor
{
	public List<string> className = new List<string>();
	
	public void Attribute(string name, string op, string value) { }

	public void Type(string name) { }

	public void Id(string value) { }

	public void Child(string name, int step, int offset, ISelector selector)
	{
		selector.Accept(this);
	}

	public void Class(string name)
	{
		this.className.Add(name);
	}

	public void PseudoClass(string name) { }

	public void PseudoElement(string name) { }

	public void List(IEnumerable<ISelector> selectors)
	{
		foreach (var selector in selectors)
		{
			selector.Accept(this);
		}
	}

	public void Combinator(IEnumerable<ISelector> selectors, IEnumerable<string> symbols)
	{
		foreach (var selector in selectors)
		{
			selector.Accept(this);
		}
	}

	public void Many(IEnumerable<ISelector> selectors)
	{
		foreach (var selector in selectors)
		{
			selector.Accept(this);
		}
	}
}

