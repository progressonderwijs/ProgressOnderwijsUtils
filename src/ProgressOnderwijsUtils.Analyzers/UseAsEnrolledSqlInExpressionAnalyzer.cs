using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class UseAsEnrolledSqlInExpressionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable analyzer release tracking - why would we care?
        "POU1010",
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Use IEnumerable<>.AsEnrolledSqlInExpression()",
        "Use the more efficient IEnumerable<>.AsEnrolledSqlInExpression() for in expressions on small sets of constant size.",
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
            SyntaxKind.InvocationExpression
        );
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is InvocationExpressionSyntax expr
            && context.SemanticModel.GetSymbolInfo(expr).Symbol is IMethodSymbol { Name: "SQL" } method
            && method.ContainingType.Name == "SafeSql"
            && method.ContainingNamespace.Name == "ProgressOnderwijsUtils"
            && expr.ArgumentList.Arguments[0].Expression is InterpolatedStringExpressionSyntax arg) {
            AnalyzeSqlInvocation(context, arg);
        }
    }

    static void AnalyzeSqlInvocation(SyntaxNodeAnalysisContext context, InterpolatedStringExpressionSyntax expr)
    {
        for (var i = 0; i < expr.Contents.Count; i++) {
            if (expr.Contents[i] is InterpolationSyntax ip
                && i > 0 && expr.Contents[i - 1] is InterpolatedStringTextSyntax str
                && str.TextToken.Text.TrimEnd().EndsWith(" in", System.StringComparison.OrdinalIgnoreCase)) {
                AnalyzeInExpression(context, ip);
            }
        }
    }

    static void AnalyzeInExpression(SyntaxNodeAnalysisContext context, InterpolationSyntax ip)
    {
        if (ip.Expression is ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax) {
            context.ReportDiagnostic(Diagnostic.Create(Rule, ip.GetLocation()));
        } else if (context.SemanticModel.GetTypeInfo(ip.Expression).Type is { } type
                   && type.AllInterfaces.Any(i => i.ContainingNamespace.Name == "Generic" && i.Name == "IEnumerable" && i.TypeParameters.Length == 1)) {
            // TODO: I cannot seem to determine easily for which type the enumerable is instantiated with
        }
    }
}
