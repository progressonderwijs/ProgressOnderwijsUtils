using System.Linq;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests;

public sealed class IfNotFalseAnalyzerTests
{
    [Fact]
    public void FlagsIfNotFalse()
    {
        var source = """
            class C
            {
                void M()
                {
                    if (!false) { }
                }
            }
            """;
        var diagnostics = DiagnosticHelper.GetDiagnostics(new IfNotFalseAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == IfNotFalseAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 4);
    }

    [Fact]
    public void DoesNotFlagIfTrue()
    {
        var source = """
            class C
            {
                void M()
                {
                    if (true) { }
                }
            }
            """;
        var diagnostics = DiagnosticHelper.GetDiagnostics(new IfNotFalseAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public async Task CodeFix_RemovesIfNotFalse()
    {
        var source = """
            class C
            {
                void M()
                {
                    if (!false) { System.Console.WriteLine("Hello"); }
                }
            }
            """;
        var expected = """
            class C
            {
                void M()
                {
                    System.Console.WriteLine("Hello");
                }
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new IfNotFalseAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new IfNotFalseCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            expected,
            result.ToString()
        );
    }
}
