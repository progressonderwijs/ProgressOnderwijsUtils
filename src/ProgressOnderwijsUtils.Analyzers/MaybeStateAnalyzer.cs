using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public sealed class MaybeStateAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "POU2001",
            "Maybe may not contain Maybe state",
            "The maybe-error state may not be ignored.",
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
            var type = context.SemanticModel.GetTypeInfo(context.Node).Type;
            if (type is INamedTypeSymbol named
                && named.IsSymbolOfType(TypeKind.Struct, "Maybe", "Collections", "ProgressOnderwijsUtils")
                && named.TypeArguments[0].IsSymbolOfType(TypeKind.Struct, "Maybe", "Collections", "ProgressOnderwijsUtils")) {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), type));
            }
        }
    }
}
