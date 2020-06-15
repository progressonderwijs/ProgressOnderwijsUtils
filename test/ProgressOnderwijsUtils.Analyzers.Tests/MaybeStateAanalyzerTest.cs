using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests
{
    public sealed class MaybeStateAanalyzerTest
    {
        [Fact]
        public void Maybe_state_should_not_be_another_maybe()
        {
            var source = @"
                using ProgressOnderwijsUtils.Collections;
                
                static class C
                {
                    public static void Test()
                    {
                        Maybe.Ok().AsMaybeWithoutError<string>()
                            .WhenOk(m => Maybe.Error(""err"").AsMaybeWithoutValue<Unit>())
                                .AssertError();
                    }
                }
            ";

            var diagnostics = DiagnosticHelper.GetDiagnostics(new MaybeStateAnalyzer(), source);
            PAssert.That(() => diagnostics.All(diagnostic => diagnostic.Id == MaybeStateAnalyzer.Rule.Id));
            PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 7);
        }
    }
}
