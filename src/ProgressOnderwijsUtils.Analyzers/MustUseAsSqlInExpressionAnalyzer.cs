using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class MustUseAsSqlInExpressionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable analyzer release tracking - why would we care?
        "POU1010",
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Use IEnumerable<>.AsSqlInExpression()",
        "Use the more efficient IEnumerable<>.AsSqlInExpression() for in expressions on sets.",
        "Functional",
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.ExpressionStatement
        );
    }

    static void Analyze(SyntaxNodeAnalysisContext context) { }
}
