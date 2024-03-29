using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests;

public sealed class MustUseExpressionResultAnalyzerTest
{
    [Fact]
    public void Expression_result_cannot_be_ignored_from_simple_expression()
    {
        var source = @"
                using ProgressOnderwijsUtils.Collections;

                static class C
                {
                    public static void Test()
                    {
                        Maybe.Ok().AsMaybeWithoutError<string>();
                    }
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new MustUseExpressionResultAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == MustUseExpressionResultAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 7);
    }

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
}
