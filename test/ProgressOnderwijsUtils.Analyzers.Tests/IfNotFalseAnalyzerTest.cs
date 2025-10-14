using System.Linq;
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
}
