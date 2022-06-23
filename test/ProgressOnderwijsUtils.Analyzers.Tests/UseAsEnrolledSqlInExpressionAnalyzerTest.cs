using System.Linq;
using ExpressionToCodeLib;
using Xunit;

namespace ProgressOnderwijsUtils.Analyzers.Tests;

public sealed class UseAsEnrolledSqlInExpressionAnalyzerTest
{
    [Fact]
    public void Use_AsEnrolledInExpression_for_literal_array()
    {
        var source = @"
                using static ProgressOnderwijsUtils.SafeSql;

                static class C
                {
                    public static void Test()
                        => _ = SQL($""select t.column from table t where t.column in {new[] {1, 2}}"");
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new UseAsEnrolledSqlInExpressionAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == UseAsEnrolledSqlInExpressionAnalyzer.Rule.Id);
    }

    [Fact]
    public void Do_not_use_AsEnrolledInExpression_for_enumerables()
    {
        var source = @"
                using System;
                using static ProgressOnderwijsUtils.SafeSql;

                enum E { One, Other, Another, };

                static class C
                {
                    public static void Test()
                        => _ = SQL($""select t.column from table t where t.column in {Enum.GetValues<E>()}"");
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new UseAsEnrolledSqlInExpressionAnalyzer(), source);
        PAssert.That(() => diagnostics.None());
    }
}
