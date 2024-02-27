using System.Linq;
using System.Threading.Tasks;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests;

public sealed class RedundantAssertNotNullAnalyzerTest
{
    [Fact]
    public void OnField_Detected()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string test = "test";
                public static void Test()
                    => Console.WriteLine(test.AssertNotNull());
            }
            """;

        var diagnostics = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == RedundantAssertNotNullAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 8);
    }

    [Fact]
    public async Task OnField_Detected_Fixed()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string test = "test";
                public static void Test()
                    => Console.WriteLine(test.AssertNotNull());
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new RedundantAssertNotNullCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string test = "test";
                public static void Test()
                    => Console.WriteLine(test);
            }
            """,
            result.ToString()
        );
    }

    [Fact]
    public async Task OnField_Detected_Fixed_with_weird_comments()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string test = "test";
                public static void Test()
                    => Console.WriteLine(test
                       //comment 1
                       . /* comment 2 */AssertNotNull(// comment 3
                       ) /*comment */);
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new RedundantAssertNotNullCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string test = "test";
                public static void Test()
                    => Console.WriteLine(test
                       //comment 1
                        /* comment 2 */// comment 3
                        /*comment */);
            }
            """,
            result.ToString()
        );
    }

    [Fact]
    public async Task OnNestedField_Detected_Fixed()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly (string A, int B) test = ("test", 42);
                public static void Test()
                    => Console.WriteLine(test.A.AssertNotNull());
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new RedundantAssertNotNullCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly (string A, int B) test = ("test", 42);
                public static void Test()
                    => Console.WriteLine(test.A);
            }
            """,
            result.ToString()
        );
    }

    [Fact]
    public async Task OnFuncField_Detected_Fixed()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly Func<string> test = () => "test";
                public static void Test()
                    => Console.WriteLine(test.AssertNotNull()());
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new RedundantAssertNotNullCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly Func<string> test = () => "test";
                public static void Test()
                    => Console.WriteLine(test());
            }
            """,
            result.ToString()
        );
    }

    [Fact]
    public async Task OnFuncField_Detected_Fixed2()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly Func<string> test = () => "test";
                public static void Test()
                    => Console.WriteLine(test().AssertNotNull());
            }
            """;

        var workspace = DiagnosticHelper.CreateProjectWithTestFile(source);
        var diagnostic = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), workspace).Single();

        var fixesMade = await DiagnosticHelper.ApplyAllCodeFixes(workspace, diagnostic, new RedundantAssertNotNullCodeFix());
        PAssert.That(() => fixesMade == 1);
        var result = await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
        Assert.Equal(
            """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly Func<string> test = () => "test";
                public static void Test()
                    => Console.WriteLine(test());
            }
            """,
            result.ToString()
        );
    }

    [Fact]
    public void OnField_DifferentMethod_NotDetected()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            record Sneaky(string text) {
                public string AssertNotNull() => text;
            }

            static class C
            {
                static readonly Sneaky test = new("test");
                public static void Test() {
                    Console.WriteLine(test.AssertNotNull());
                    Console.WriteLine("test".PretendNullable().AssertNotNull());
                }
            }
            """;

        var diagnostics = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void OnNullableField_NotDetected()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static readonly string? test = "test";
                public static void Test()
                    => Console.WriteLine(test.AssertNotNull());
            }
            """;

        var diagnostics = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void OnGenericallyInferredNonNullability_Detected()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static T PassThrough<T>(T input) => input;
            
                public static void Test()
                    => Console.WriteLine(PassThrough("test").AssertNotNull());
            }
            """;

        var diagnostics = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == RedundantAssertNotNullAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 9);
    }

    [Fact]
    public void OnGenericallyInferredNullability_NotDetected()
    {
        var source = """
            #nullable enable
            using ProgressOnderwijsUtils;
            using System;

            static class C
            {
                static T PassThrough<T>(T input) => input;
            
                public static void Test()
                    => Console.WriteLine(PassThrough("test".PretendNullable()).AssertNotNull());
            }
            """;

        var diagnostics = DiagnosticHelper.GetDiagnostics(new RedundantAssertNotNullAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }
}
