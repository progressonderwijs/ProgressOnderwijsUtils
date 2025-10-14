using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IfNotFalseAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable analyzer release tracking - why would we care?
        "POU1004",
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Redundant if (!false) statement",
        "The if statement condition is always true (if (!false))",
        "CodeQuality",
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;
        if (ifStatement.Condition is PrefixUnaryExpressionSyntax prefix &&
            prefix.IsKind(SyntaxKind.LogicalNotExpression) &&
            prefix.Operand is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.FalseLiteralExpression)) {
            context.ReportDiagnostic(Diagnostic.Create(Rule, ifStatement.GetLocation()));
        }
    }
}
