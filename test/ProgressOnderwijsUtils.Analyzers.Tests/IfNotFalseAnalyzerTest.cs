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
        var testCases = new[] {
            new {
                Source = """
                    class C
                    {
                        void M()
                        {
                            if (!false) { System.Console.WriteLine("Hello"); }
                        }
                    }
                    """,
                Expected = """
                    class C
                    {
                        void M()
                        {
                            System.Console.WriteLine("Hello");
                        }
                    }
                    """,
            },
            new {
                Source = """
                    class C
                    {
                        void M()
                        {
                            if (!false) { System.Console.WriteLine("A"); System.Console.WriteLine("B"); }
                        }
                    }
                    """,
                Expected = """
                    class C
                    {
                        void M()
                        {
                            System.Console.WriteLine("A"); System.Console.WriteLine("B");
                        }
                    }
                    """,
            },
            new {
                Source = """
                    class C
                    {
                        void M()
                        {
                            if (!false)
                            {
                                System.Console.WriteLine("Hello");
                            }
                        }
                    }
                    """,
                Expected = """
                    class C
                    {
                        void M()
                        {
                            System.Console.WriteLine("Hello");
                        }
                    }
                    """,
            },
            new {
                Source = """
                    class C
                    {
                        void M()
                        {
                            if (!false) { }
                        }
                    }
                    """,
                Expected = """
                    class C
                    {
                        void M()
                        {
                        }
                    }
                    """,
            },
            new {
                Source = """
                    class C
                    {
                        void M()
                        {
                            if (!false)
                            {
                                System.Console.WriteLine("A");
                                System.Console.WriteLine("B");
                            }
                        }
                    }
                    """,
                Expected = """
                    class C
                    {
                        void M()
                        {
                            System.Console.WriteLine("A");
                            System.Console.WriteLine("B");
                        }
                    }
                    """,
            },
        };

        foreach (var test in testCases) {
            var workspace = DiagnosticHelper.CreateProjectWithTestFile(test.Source);
            var diagnostic = DiagnosticHelper.GetDiagnostics(new IfNotFalseAnalyzer(), workspace).Single();
            var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new IfNotFalseCodeFix());
            PAssert.That(() => fixesMade == 1);

            var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
            var resultText = result.ToString().Replace("\r\n", "\n");
            Assert.Equal(test.Expected, resultText);
        }
    }
}
