using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests
{
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
        public void Invocation_expression_van_explicitly_be_ignored()
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
    }
}
