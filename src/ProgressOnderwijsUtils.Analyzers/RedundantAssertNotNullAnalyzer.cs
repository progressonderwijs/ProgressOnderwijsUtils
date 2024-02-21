using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class RedundantAssertNotNullAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable analyzer release tracking - why would we care?
        "POU1002",
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Redundant .AssertNotNull()",
        "The call to AssertNotNull() is redundant because the target is known to be non-null",
        "Functional",
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    static readonly NameSyntax
        AssertNotNull_Name = SyntaxFactory.ParseName("AssertNotNull");

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var token = context.CancellationToken;

        if (context.Node is InvocationExpressionSyntax {
                ArgumentList.Arguments.Count: 0,
                Expression: MemberAccessExpressionSyntax memberAccess,
            }
            && memberAccess.Name.IsEquivalentTo(AssertNotNull_Name)
            && context.SemanticModel.GetTypeInfo(memberAccess.Expression, token) is { Nullability.FlowState: NullableFlowState.NotNull, } type
            && context.SemanticModel.GetSymbolInfo(memberAccess.Name, token) is {
                Symbol: {
                    ContainingNamespace: { Name: "ProgressOnderwijsUtils", ContainingNamespace.IsGlobalNamespace: true, },
                    ContainingType.Name : "NullableReferenceTypesHelpers",
                },
            }) {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Rule,
                    memberAccess.GetLocation(),
                    type,
                    new[] {
                        memberAccess.OperatorToken.GetLocation(), memberAccess.Name.GetLocation(),
                    }
                )
            );
        }
    }
}
