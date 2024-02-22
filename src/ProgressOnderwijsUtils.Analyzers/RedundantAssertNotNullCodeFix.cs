using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ProgressOnderwijsUtils.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantAssertNotNullCodeFix))]
[Shared]
// ReSharper disable once UnusedType.Global - implicitly via host reflection
public sealed class RedundantAssertNotNullCodeFix : CodeFixProvider
{
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(RedundantAssertNotNullAnalyzer.Rule.Id);

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        const string action = "Remove redundant .AssertNotNull()";

        var diagnostic = context.Diagnostics.First();
        context.RegisterCodeFix(
            CodeAction.Create(
                action,
                async cancel => {
                    var document = context.Document;
                    var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);

                    if (root is null) {
                        return document;
                    }
                    // find the token at the additional location we reported in the analyzer
                    var syntaxNode = root.FindToken(diagnostic.Location.SourceSpan.End - 1).Parent ?? throw new("syntax node not found?");
                    var invocationExpression = (InvocationExpressionSyntax)syntaxNode.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().First().Ancestors().First();
                    var memberAccess = (MemberAccessExpressionSyntax)invocationExpression.Expression;
                    var memberExpressionWithTrivia = memberAccess.Expression
                        .WithTrailingTrivia(
                            new[] {
                                memberAccess.Expression.GetTrailingTrivia(),
                                memberAccess.OperatorToken.LeadingTrivia,
                                memberAccess.OperatorToken.TrailingTrivia,
                                memberAccess.Name.DescendantTrivia(),
                                memberAccess.GetTrailingTrivia(),
                                invocationExpression.ArgumentList.DescendantTrivia(),
                            }.SelectMany(x => x)
                        );

                    var newRoot = root.ReplaceNode(invocationExpression, memberExpressionWithTrivia);
                    return document.WithSyntaxRoot(newRoot);
                },
                action
            ),
            diagnostic
        );

        return Task.CompletedTask;
    }
}
