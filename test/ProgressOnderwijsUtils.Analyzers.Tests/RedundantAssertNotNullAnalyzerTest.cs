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

    /*
    [Fact]
    public void Void_expression_is_not_ignored_by_definition()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    public static void Test()
                    {
                        Maybe.Ok().AsMaybeWithoutError<string>().If(_ => { }, err => { });
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void Unit_expression_may_be_ignored()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    public static void Test()
                    {
                        Maybe.Ok().AsMaybeWithoutError<string>().AssertOk();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void Invocation_expression_result_cannot_be_ignored()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    static Maybe<Unit, string> Foo()
                    {
                        return Maybe.Ok();
                    }

                    public static void Test()
                    {
                        Foo();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == MustUseExpressionResultAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 12);
    }

    [Fact]
    public void Invocation_expression_can_explicitly_be_ignored()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    static Maybe<Unit, string> Foo()
                    {
                        return Maybe.Ok();
                    }

                    public static void Test()
                    {
                        _ = Foo();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void Invocation_expression_result_cannot_be_ignored_in_expression_body()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    static Maybe<Unit, string> Foo()
                    {
                        return Maybe.Ok();
                    }

                    public static void Test()
                        => Foo();
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == MustUseExpressionResultAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 11);
    }

    [Fact]
    public void Assignment_expression_result_maybe_ignored_in_expression_body()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    static Maybe<Unit, string> Foo;

                    public static void Test()
                        => Foo = Maybe.Ok().AsMaybeWithoutError<string>();
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void Invocation_expression_result_cannot_be_ignored_in_constructor_lambda_expression()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                sealed class Test
                {
                    static Maybe<Unit, string> Foo()
                    {
                        return Maybe.Ok();
                    }

                    public Test()
                        => Foo();
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == MustUseExpressionResultAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 11);
    }

    [Fact]
    public void Local_function_result_cannot_be_ignored()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                sealed class Test
                {
                    public Test()
                    {
                        Foo();

                        static Maybe<Unit, string> Foo()
                            => Maybe.Ok();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == MustUseExpressionResultAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 7);
    }

    [Fact]
    public void All_kinds_of_assignments_are_allowed()
    {
        var source = @"
                using System.Collections.Generic;
                using ProgressOnderwijsUtils.Collections;

                sealed class C
                {
                    static readonly Maybe<Unit, string> staticField = Maybe.Ok().AsMaybeWithoutError<string>();
                    readonly Maybe<Unit, string> instanceField;

                    C()
                    {
                        instanceField = Maybe.Ok().AsMaybeWithoutError<string>();
                    }

                    Maybe<Unit, string> Test()
                    {
                        var variable = Maybe.Ok().AsMaybeWithoutError<string>();
                        variable = Maybe.Ok().AsMaybeWithoutError<string>();

                        return Maybe.Ok().AsMaybeWithoutError<string>();
                    }

                    IEnumerable<Maybe<Unit, string>> Yields()
                    {
                        yield return Maybe.Ok().AsMaybeWithoutError<string>();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }

    [Fact]
    public void Func_cannot_be_assigned_to_action()
    {
        var source = @"
                using System;
                using System.Collections.Generic;
                using ProgressOnderwijsUtils.Collections;

                sealed class C
                {
                    static readonly Action staticField = () => Maybe.Ok().AsMaybeWithoutError<string>();
                    readonly Action instanceField;

                    C()
                    {
                        instanceField = () => Maybe.Ok().AsMaybeWithoutError<string>();
                    }

                    Action<int> Test()
                    {
                        Action<int> variable = i => Maybe.Ok().AsMaybeWithoutError<string>();
                        variable = _ => Maybe.Ok().AsMaybeWithoutError<string>();

                        return _ => Maybe.Ok().AsMaybeWithoutError<string>();
                    }

                    IEnumerable<Action> Yields()
                    {
                        yield return () => Maybe.Ok().AsMaybeWithoutError<string>();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.All(diagnostic => diagnostic.Id == MustUseExpressionResultAnalyzer.Rule.Id));
        PAssert.That(() => diagnostics.ArraySelect(diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line).SetEqual(new[] { 7, 12, 17, 18, 20, 25, }));
    }

    [Fact]
    public void Func_invocation_cannot_be_ignored()
    {
        var source = @"
                using System;
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    static readonly Func<Maybe<Unit, string>> func = () => Maybe.Ok().AsMaybeWithoutError<string>();

                    static void TestStatementBody()
                    {
                        func();
                    }

                    static void TestExpressionBody()
                        => func();
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.All(diagnostic => diagnostic.Id == MustUseExpressionResultAnalyzer.Rule.Id));
        PAssert.That(() => diagnostics.ArraySelect(diagnostic => diagnostic.Location.GetLineSpan().StartLinePosition.Line).SetEqual(new[] { 10, 14, }));
    }
    */
}
