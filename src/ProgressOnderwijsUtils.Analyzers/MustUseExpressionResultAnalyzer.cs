using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ProgressOnderwijsUtils.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public sealed class MustUseExpressionResultAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new(
#pragma warning disable RS2008 // Enable analyzer release tracking - why would we care?
        "POU1001",
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Result must be used",
        "The result of the expression may not be ignored",
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
            SyntaxKind.ExpressionStatement,
            SyntaxKind.ArrowExpressionClause,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ParenthesizedLambdaExpression
        );
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ExpressionStatementSyntax expr) {
            AnalyzeExpressionStatement(context, expr);
        } else if (context.Node is ArrowExpressionClauseSyntax arrow) {
            AnalyzeArrowExpressionClause(context, arrow);
        } else if (context.Node is LambdaExpressionSyntax lamdba) {
            AnalyzeLambdaExpression(context, lamdba);
        }
    }

    static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context, ExpressionStatementSyntax expr)
    {
        if (expr.Expression.Kind() == SyntaxKind.SimpleAssignmentExpression) {
            return;
        }

        var exprType = context.SemanticModel.GetTypeInfo(expr.Expression).Type;
        if (!ExpressionTypeCanBeIgnored(exprType)) {
            context.ReportDiagnostic(Diagnostic.Create(Rule, expr.GetLocation(), exprType));
        }
    }

    static void AnalyzeArrowExpressionClause(SyntaxNodeAnalysisContext context, ArrowExpressionClauseSyntax arrow)
    {
        if (arrow.Expression.Kind() == SyntaxKind.SimpleAssignmentExpression) {
            return;
        }

        var exprType = context.SemanticModel.GetTypeInfo(arrow.Expression).Type;
        if (!ExpressionTypeCanBeIgnored(exprType)) {
            if (arrow.Parent is MethodDeclarationSyntax method) {
                var symbol = context.SemanticModel.GetDeclaredSymbol(method);
                if (symbol?.ReturnType.SpecialType == SpecialType.System_Void) {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, arrow.GetLocation(), exprType));
                }
            } else if (arrow.Parent is ConstructorDeclarationSyntax) {
                context.ReportDiagnostic(Diagnostic.Create(Rule, arrow.GetLocation(), exprType));
            }
        }
    }

    static void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context, LambdaExpressionSyntax lambda)
    {
        var convertedType = context.SemanticModel.GetTypeInfo(lambda).ConvertedType;

        if (!SymbolMatcher.IsActionDelegate.IsSymbolOfType(convertedType)) {
            return;
        }

        if (lambda.ExpressionBody != null) {
            var lambdaExprType = context.SemanticModel.GetTypeInfo(lambda.ExpressionBody).Type;
            if (!ExpressionTypeCanBeIgnored(lambdaExprType)) {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation(), lambdaExprType));
            }
        }
    }

    static bool ExpressionTypeCanBeIgnored(ITypeSymbol? exprType)
    {
        if (exprType == null) {
            return true;
        } else if (exprType.TypeKind == TypeKind.Error) {
            return true;
        } else if (exprType.SpecialType == SpecialType.System_Void) {
            return true;
        } else if (SymbolMatcher.IsUnit.IsSymbolOfType(exprType)) {
            return true;
        } else {
            // TODO: first start wilt the maybe's, then apply to other/all types
            return !SymbolMatcher.IsMaybe.IsSymbolOfType(exprType);
        }
    }
}