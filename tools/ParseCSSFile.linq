<Query Kind="Program">
  <Reference Relative="..\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll">C:\Users\PRGMA01\GitHub\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Debug\net6.0-windows\ProgressOnderwijsUtils.dll</Reference>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>ProgressOnderwijsUtils.Html</Namespace>
  <Namespace>static ProgressOnderwijsUtils.Html.Tags</Namespace>
  <Namespace>static ProgressOnderwijsUtils.SafeSql</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

void Main()
{
	var testString = @"/*Test*/	
.umcg .institutionlogo__0 {
    background-image: url(""./ logo / umcg.png"");
}
.umcg .login-warning-NL::after {
    content: ""Welkom bij de/*testing*/ medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen.	Je komt automatisch op het aanmeldscherm om je e - mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent.Zodra je stemt, is de link met de gegevens verbroken."";
	white - space: pre - wrap;
display: inline - block;
}
/*Test
more testesfsef 'dwadwadawdaw' "" dwadwadawd'wadawdawd' ""
fsfsf*/

'dwadwadawdaw' "" dwadwadawd'wadawdawd' ""
.umcg.login - warning - EN::after {
content: ""Welkom bij de medezeggenschapsverkiezingen UMCG.\a\aGebruik de onderstaande knop om in te loggen. Je komt automatisch op het aanmeldscherm om je e-mailadres in te typen.\a\aJe gegevens worden gebruikt om te checken of je een stemgerechtigde van het UMCG bent. Zodra je stemt, is de link met de gegevens verbroken."";
	white - space: pre - wrap;/*testing*/
display: inline - block;
}";
	testString.Dump();
	testString = removeComments(testString).Dump();
	testString = removestrings(testString).Dump();
}

public static string removeComments(string fileWithComments) 
{
	var commentRegx = new Regex(@"(?<comment>(/\*)(.|\s)*?(\*/))");
	
	return commentRegx.Replace(fileWithComments, "");
}

public static string removestrings(string fileWithStrings)
{
	var stringRegx = new Regex(@"(?<string>(\""[^\""]*\"")|((\')[^\']*(\')))");

	return stringRegx.Replace(fileWithStrings, "");
}