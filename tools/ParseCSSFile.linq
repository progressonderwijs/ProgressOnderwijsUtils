<Query Kind="Program">
  <Reference Relative="..\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll">C:\Users\PRGMA01\GitHub\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll</Reference>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>ProgressOnderwijsUtils.Html</Namespace>
  <Namespace>static ProgressOnderwijsUtils.Html.Tags</Namespace>
  <Namespace>static ProgressOnderwijsUtils.SafeSql</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

public Dictionary<string, HashSet<CssFile>> classes = new Dictionary<string, HashSet<CssFile>>();
public List<CssFile> CssFiles => new List<CssFile>(){
	new(@"src/Progress.ClientApp/dist/portal.css", @"src/Progress.Business/CssClasses/", "Progress.Businnes.CssClasses","PortalStyleClasses"),
	new(@"src/Progress.ClientApp/dist/catalogue.css", @"src/Progress.Business/CssClasses/", "Progress.Businnes.CssClasses","CatalogueStyleClasses"),
	new(@"src/Progress.ClientApp/dist/pnet.css", @"src/Progress.Business/CssClasses/", "Progress.Businnes.CssClasses","PNetStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/curio.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","CurioStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/demo.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","DemoStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/ehb.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","EhbStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/hanze.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","HanzeStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/homeinstead.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","HomeinsteadStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/hva.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","HvaStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/react-tabs.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","ReactTabsStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/site.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","SiteStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/tudelft.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","TudelftStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/umcg.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","UmcgStyleClasses"),
	new(@"src/ProgressElections/ProgressElections/ClientApp/css/uvh.css", @"src/ProgressElections/ProgressElections/ClientApp/css/", "ProgressElections.ClientApp.css","UvhTabsStyleClasses"),
};

void Main()
{
	CssFiles.OrderBy(x=> x.nSpace);
	foreach (var file in CssFiles)
	{
		var markup3 = StyleClassMarkUpFromFile(file).Dump();
	}
}

public static Regex DotRegex
	=> new Regex(@"(\.)(?<name>[^0-9][^\u0000-,./:-@\[\]^`{-\u009f]+)");
//More complex regex only '-' still works:
//	=> new Regex(@"(\.)(?!-?[0-9])(?<name>[^\u0000-,./:-@\[\\\]^`{-\u009f]+)");

public static string RemoveComments(string fileWithComments)
{
	var commentRegx = new Regex(@"(?<comment>(/\*)(.|\s)*?(\*/))");

	return commentRegx.Replace(fileWithComments, "");
}

public static string RemoveStrings(string fileWithStrings)
{
	var stringRegx = new Regex(@"(?<string>(\""[^\""]*\"")|((\')[^\']*(\')))");

	return stringRegx.Replace(fileWithStrings, "");
}

public static List<CssObjectContainer> GetAllClassObjects(string fileWithDotStartingWords)
{
	var matchCollection = DotRegex.Matches(fileWithDotStartingWords);
	var list = matchCollection.Cast<Match>().Select(m =>
	{
		var value = m.Groups["name"].Value;
		if (IsValidClassName(value))
		{
			return new CssObjectContainer(value);
		}
		return null;
	}).ToList();
	return list;
}

public static List<CssObjectContainer> ParseOriginalCssToClassList(string baseFile)
{
	baseFile = RemoveComments(baseFile);
	baseFile = RemoveStrings(baseFile);
	return GetAllClassObjects(baseFile);
}

public record CssObjectContainer(string className)
{
	public string objectName = CreateObjectName(className);

	static string CreateObjectName(string className)
	{
		var allSymbols = new Regex(@"[^a-zA-Z0-9_-]");
		var objectName = allSymbols.Replace(className, "");
		if (objectName.Length == 0)
		{
			throw new($@"Empty objectname for css class '{className}', all symbols except '-' and '_' are removed when generating objectnames. Make sure the css class name contains a non symbol character.");
		}
		var reformed = objectName.Split('-', System.StringSplitOptions.TrimEntries).ToList();
		return reformed.JoinStrings("_");
	}
}

public record CssFile(string inputFile, string outputFolder, string nSpace, string className)
{
	public string inputF = getCompleteFilePath(inputFile);
	public string outputF = getCompleteFilePath(outputFolder) + className + @".cs";

	static string getCompleteFilePath(string fP)
	{
		var folderpath = Util.CurrentQueryPath + @"..\..\..\..\progress\";
		//var folderpath = GetMyPath();
		return folderpath + fP;
	}
}

public static bool IsValidClassName(string className)
{
	if (className.Contains('\\'))
	{
		throw new($@"Css class '{className}' contains invalid symbol '\'.");
	}
	if (className.Equals('-'))
	{
		throw new($@"Invalid classname '{className}'.");
	}
	if (className.First().Equals('-') && char.IsDigit(className[1]))
	{
		throw new($@"Invalid classname '{className}', with pattern '-digit'.");
	}
	return true;
}

class DistinctICssClassComparer : IEqualityComparer<CssObjectContainer>
{
	public bool Equals(CssObjectContainer x, CssObjectContainer y)
	{
		if (x.objectName == y.objectName && x.className != y.className)
		{
			throw new($@"Duplicate object name '{x.objectName}' found from CSS classnames: '{y.className}' and '{x.className}'. Characters:'-', '_', '--' and '__' become '_' and special symbols get removed when generating the object names. Creating possible duplicates, use a different name for one of the two occurances.");
		}
		return x.objectName == y.objectName;
	}

	public int GetHashCode(CssObjectContainer obj)
	{
		return obj.objectName.GetHashCode();
	}
}

public string StyleClassMarkUpFromClassList(List<CssObjectContainer> cssClasses, CssFile file)
{
	var uniqueClassList = cssClasses.Distinct(new DistinctICssClassComparer());
	var fileContent = $@"namespace {file.nSpace}

public static class {file.className}
{{
";
	foreach (var classContainer in uniqueClassList)
	{
		//Duplicate objects in the same namespace will get a reference to the first object found in that namespace
		HashSet<CssFile> files;
		if (classes.TryGetValue(classContainer.className, out files) && files.Any(x=> x.nSpace == file.nSpace))
		{
			CssFile reference = classes[classContainer.className].Where(x => x.nSpace == file.nSpace).FirstOrDefault();
			fileContent += $@"	public static readonly CssClass {classContainer.objectName} = {reference.className}.{classContainer.objectName};
";
		}
		else
		{
			fileContent += $@"	public static readonly CssClass {classContainer.objectName} = new(""{classContainer.className}"");
";
			if (files == null)
			{
				classes.TryAdd(classContainer.className, new HashSet<CssFile>() { file });
			}
			else
			{
				classes[classContainer.className].Add(file);
			}
		}
	}
	fileContent += "}";

	return fileContent;
}

public string StyleClassMarkUpFromFile(CssFile file)
{
	var classList = ParseOriginalCssToClassList(System.IO.File.ReadAllText(file.inputF));
	return StyleClassMarkUpFromClassList(classList, file);
}

//testing vars
static string PreDefStringWithoutComments = @".umcg .institutionlogo__0 {background-image: url(""./ logo / umcg.png"");}.umcg.login-warning-NL::after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e - mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent.Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}.umcg.login-warning-EN::after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e-mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent. Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}";
static string PreDefStringWithoutStrings = @".umcg .institutionlogo__0 {background-image: url();}.umcg.login-warning-NL::after {content: ;white - space: pre - wrap;display: inline - block;}.umcg.login-warning-EN::after {content: ;white - space: pre - wrap;display: inline - block;}";
static List<string> PreDefDotStartingWordsList = new List<string> { "umcg", "institutionlogo__0", "umcg", "login-warning-NL", "umcg", "login-warning-EN", };
static List<CssObjectContainer> PreDefDotStartingWordsObjectList = new List<CssObjectContainer>() { new("umcg"), new("institutionlogo__0"), new("umcg"), new("login-warning-NL"), new("umcg"), new("login-warning-EN") };
static List<CssObjectContainer> PreDefDotStartingUniqueWordsObjectList = new List<CssObjectContainer>() { new("umcg"), new("institutionlogo__0"), new("login-warning-NL"), new("login-warning-EN") };

//only used for testing
public static List<string> GetAllDotStartingWords(string fileWithDotStartingWords)
{
	var matchCol = DotRegex.Matches(fileWithDotStartingWords);
	var list = matchCol.Cast<Match>().Select(match => match.Groups["name"].Value).ToList();
	return list;
}

//tests
public void TestRegexes(string testString)
{
	var noCommentTestString = RemoveComments(testString);
	if (noCommentTestString != PreDefStringWithoutComments)
	{
		noCommentTestString.Dump();
		PreDefStringWithoutComments.Dump();
		throw new("No comments not the same.");
	}
	var noStingAndCommentTestString = RemoveStrings(noCommentTestString);
	if (noStingAndCommentTestString != PreDefStringWithoutStrings)
	{
		noStingAndCommentTestString.Dump();
		PreDefStringWithoutStrings.Dump();
		throw new("No strings not the same.");
	}
	var dotStartingWordsList = GetAllDotStartingWords(noStingAndCommentTestString);
	if (!dotStartingWordsList.SequenceEqual(PreDefDotStartingWordsList))
	{
		dotStartingWordsList.Dump();
		PreDefDotStartingWordsList.Dump();
		throw new("Dot starting words not the same.");
	}
	var classObjects = GetAllClassObjects(noStingAndCommentTestString);
	if (!classObjects.SequenceEqual(PreDefDotStartingWordsObjectList))
	{
		classObjects.Dump();
		PreDefDotStartingWordsObjectList.Dump();
		throw new("Class objects not the same.");
	}
}