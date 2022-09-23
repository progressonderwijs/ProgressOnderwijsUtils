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
static List<string> PreDefDotStartingWordsList = new List<string>{"umcg","institutionlogo__0", "umcg","login-warning-NL", "umcg","login-warning-EN", };

void Main()
{
	var testString = @"/*Test*/.umcg .institutionlogo__0 {background-image: url(""./ logo / umcg.png"");}.umcg.login-warning-NL::after {content: ""Welkom bij de/*testing*/ medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e - mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent.Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;display: inline - block;}/*Test more testesfsef 'dwadwadawdaw' "" dwadwadawd'wadawdawd' ""fsfsf*/.umcg.login-warning-EN::/*esfsef*/after {content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e-mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent. Zodra je stemt, is de link met de gegevens verbroken."";white - space: pre - wrap;/*testing*//* with a / oh and a * an a /* */display: inline - block;}";
	TestRegexes(testString);
}

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
	var dotRegx = new Regex(@"(\.)(?<name>[^0-9][^\u0000-,./:-@\[\]^`{-\u009f]+)");
	//More complex regex only one '-' still works
	//var dotRegx = new Regex(@"(\.)(?!-?[0-9])(?<name>[^\u0000-,./:-@\[\\\]^`{-\u009f]+)");

	var matchCol = dotRegx.Matches(fileWithDotStartingWords);
	var list = matchCol.Cast<Match>().Select(match =>match.Groups["name"].Value).ToList();
	return list;
}

public void TestRegexes(string baseString)
{
	var noCommentTestString = removeComments(baseString);
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
	dotStartingWordsList.Dump();
}