<Query Kind="Program">
  <Reference Relative="..\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll">C:\Users\PRGMA01\GitHub\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll</Reference>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>ProgressOnderwijsUtils.Html</Namespace>
  <Namespace>static ProgressOnderwijsUtils.Html.Tags</Namespace>
  <Namespace>static ProgressOnderwijsUtils.SafeSql</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

static string PreDefStringWithoutComments = @".umcg .institutionlogo__0 {background-image: url(""./ logo / umcg.png"");}.umcg.login-warning-NL::after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e - mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent.Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}.umcg.login-warning-EN::after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e-mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent. Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}";
static string PreDefStringWithoutStrings = @".umcg .institutionlogo__0 {background-image: url();}.umcg.login-warning-NL::after {content: ;white - space: pre - wrap;display: inline - block;}.umcg.login-warning-EN::after {content: ;white - space: pre - wrap;display: inline - block;}";
static List<string> PreDefDotStartingWordsList = new List<string> { "umcg", "institutionlogo__0", "umcg", "login-warning-NL", "umcg", "login-warning-EN", };
static List<CssObjectContainer> PreDefDotStartingWordsObjectList = new List<CssObjectContainer>() { new("umcg"), new("institutionlogo__0"), new("umcg"), new("login-warning-NL"), new("umcg"), new("login-warning-EN") };
static List<CssObjectContainer> PreDefDotStartingUniqueWordsObjectList = new List<CssObjectContainer>() { new("umcg"), new("institutionlogo__0"), new("login-warning-NL"), new("login-warning-EN") };

void Main()
{
	var testString = @"/*Test*/.umcg .institutionlogo__0 {background-image: url(""./ logo / umcg.png"");}.umcg.login-warning-NL::after {content: ""Welkom bij de/*testing*/ medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e - mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent.Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}/*Test more testesfsef 'dwadwadawdaw' "" dwadwadawd'wadawdawd' ""fsfsf*/.umcg.login-warning-EN::/*esfsef*/after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e-mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent. Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;/*testing*//* with a / oh and a * an a /* */display: inline - block;}";
	TestRegexes(testString);
	var classList = parseOriginalCssToClassList(testString);
	classList.SequenceEqual(PreDefDotStartingWordsObjectList).Dump();
	//execute this method after all the css classes are extracted to remove duplicates and different css classes with the same object name
	var uniqueClassList = classList.Distinct(new DistinctItemComparer()).Dump();
	uniqueClassList.SequenceEqual(PreDefDotStartingUniqueWordsObjectList).Dump();
}

public static Regex DotRegex
	=> new Regex(@"(\.)(?<name>[^0-9][^\u0000-,./:-@\[\]^`{-\u009f]+)");
//More complex regex only '-' still works:
//	=> new Regex(@"(\.)(?!-?[0-9])(?<name>[^\u0000-,./:-@\[\\\]^`{-\u009f]+)");

public static string removeComments(string fileWithComments)
{
	var commentRegx = new Regex(@"(?<comment>(/\*)(.|\s)*?(\*/))");

	return commentRegx.Replace(fileWithComments, "");
}

public static string removeStrings(string fileWithStrings)
{
	var stringRegx = new Regex(@"(?<string>(\""[^\""]*\"")|((\')[^\']*(\')))");

	return stringRegx.Replace(fileWithStrings, "");
}

public static List<string> getAllDotStartingWords(string fileWithDotStartingWords)
{
	var matchCol = DotRegex.Matches(fileWithDotStartingWords);
	var list = matchCol.Cast<Match>().Select(match => match.Groups["name"].Value).ToList();
	return list;
}

public static List<CssObjectContainer> getAllClassObjects(string fileWithDotStartingWords)
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

public static List<CssObjectContainer> parseOriginalCssToClassList(string baseFile)
{
	baseFile = removeComments(baseFile);
	baseFile = removeStrings(baseFile);
	return getAllClassObjects(baseFile);
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

public void TestRegexes(string testString)
{
	var noCommentTestString = removeComments(testString);
	if (noCommentTestString != PreDefStringWithoutComments)
	{
		noCommentTestString.Dump();
		PreDefStringWithoutComments.Dump();
		throw new("No comments not the same.");
	}
	var noStingAndCommentTestString = removeStrings(noCommentTestString);
	if (noStingAndCommentTestString != PreDefStringWithoutStrings)
	{
		noStingAndCommentTestString.Dump();
		PreDefStringWithoutStrings.Dump();
		throw new("No strings not the same.");
	}
	var dotStartingWordsList = getAllDotStartingWords(noStingAndCommentTestString);
	if (!dotStartingWordsList.SequenceEqual(PreDefDotStartingWordsList))
	{
		dotStartingWordsList.Dump();
		PreDefDotStartingWordsList.Dump();
		throw new("Dot starting words not the same.");
	}
	var classObjects = getAllClassObjects(noStingAndCommentTestString);
	if (!classObjects.SequenceEqual(PreDefDotStartingWordsObjectList))
	{
		classObjects.Dump();
		PreDefDotStartingWordsObjectList.Dump();
		throw new("Class objects not the same.");
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

class DistinctItemComparer : IEqualityComparer<CssObjectContainer>
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