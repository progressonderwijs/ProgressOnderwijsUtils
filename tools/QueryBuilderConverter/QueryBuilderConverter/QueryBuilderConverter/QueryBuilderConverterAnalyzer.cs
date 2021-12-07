#nullable disable
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace QueryBuilderConverter
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QueryBuilderConverterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "QueryBuilderConverter";
        private const string Category = "Api usage";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly string Title = "Avoid unsafe QueryBuilder.Create";
        private static readonly string MessageFormat = "Replace QueryBuilder.Create(...) with injection-safe safe SQL($\"...\")";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var syntaxNode = context.Node;

            if (IsQueryBuilderCreate(syntaxNode, context.SemanticModel, context.CancellationToken))
            {
                var diagnostic = Diagnostic.Create(Rule, syntaxNode.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }

        public static bool IsQueryBuilderCreate(SyntaxNode syntaxNode, SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var invocationExpr = syntaxNode as InvocationExpressionSyntax;
            if (invocationExpr == null)
                return false;

            var calledExpr = invocationExpr.Expression;

            var memberAccessExpression = calledExpr as MemberAccessExpressionSyntax;

            if (memberAccessExpression == null)
                return false;

            var calledMethodName = memberAccessExpression.Name;

            if (calledMethodName.Identifier.Text != "Create" && calledMethodName.Identifier.Text != "CreateDynamic")
                return false;

            var typeOfMemberExpr = memberAccessExpression.Expression as IdentifierNameSyntax;

            if (typeOfMemberExpr == null)
                return false;

            if (typeOfMemberExpr.Identifier.Text != "QueryBuilder")
                return false;

            //calling something like QueryBuilder.Create/CreateDynamic

            if (invocationExpr.ArgumentList.Arguments.Count < 1)
                return false;

            var firstArgument = invocationExpr.ArgumentList.Arguments[0];

            if (firstArgument.NameColon != null)
                return false;

            if (firstArgument.RefOrOutKeyword.Kind() != SyntaxKind.None)
                return false;

            var firstArgumentStringLiteralExpr = firstArgument.Expression as LiteralExpressionSyntax;

            if (firstArgumentStringLiteralExpr == null)
                return false;

            if (firstArgumentStringLiteralExpr.Kind() != SyntaxKind.StringLiteralExpression)
                return false;

            var symbolInfo = semanticModel.GetSymbolInfo(calledMethodName, cancellationToken);

            if (symbolInfo.Symbol == null)
                return false;

            if (symbolInfo.Symbol.ContainingNamespace.ToString() != "ProgressOnderwijsUtils")
                return false;

            var speculativeSqlQuerySymbols = semanticModel.GetSpeculativeSymbolInfo(invocationExpr.Span.Start,
                SyntaxFactory.IdentifierName("SQL"), SpeculativeBindingOption.BindAsExpression);
            if (speculativeSqlQuerySymbols.Symbol != null)
                if (speculativeSqlQuerySymbols.Symbol.ContainingType.Name != "SafeSql" ||
                    speculativeSqlQuerySymbols.Symbol.ContainingType.ContainingNamespace.ToString() != "ProgressOnderwijsUtils")
                    return false;

          return true;
        }
    }
}
