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
                        => _ = SQL($""select t.column from table t where t.column in {new[] {1, 2, }}"");
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new UseAsEnrolledSqlInExpressionAnalyzer(), source);
        PAssert.That(() => diagnostics.Single().Id == UseAsEnrolledSqlInExpressionAnalyzer.Rule.Id);
        PAssert.That(() => diagnostics.Single().Location.GetLineSpan().StartLinePosition.Line == 6);
    }

    [Fact]
    public void Use_multiple_AsEnrolledInExpression_for_literal_arrays()
    {
        var source = @"
                using static ProgressOnderwijsUtils.SafeSql;

                static class C
                {
                    public static void Test()
                        => _ = SQL($@""
                            select t.column
                            from table t
                            where 1=1
                                and t.column in {new[] {1, 2, }}
                                and t.other in {new long[] {3, 4, }}
                        "");
                }
            ";

        var diagnostics = DiagnosticHelper.GetDiagnostics(new UseAsEnrolledSqlInExpressionAnalyzer(), source);
        PAssert.That(() => diagnostics.All(d => d.Id == UseAsEnrolledSqlInExpressionAnalyzer.Rule.Id));
        PAssert.That(() => diagnostics.Select(d => d.Location.GetLineSpan().StartLinePosition.Line).SequenceEqual(new[] { 10, 11, }));
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
