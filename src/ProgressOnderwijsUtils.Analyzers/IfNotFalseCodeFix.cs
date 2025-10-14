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
public sealed class IfNotFalseCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => [IfNotFalseAnalyzer.Rule.Id,];

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        const string title = "Remove redundant if statement";
        var diagnostic = context.Diagnostics.First();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) {
            return;
        }

        var ifStatement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<IfStatementSyntax>();
        if (ifStatement is null) {
            return;
        }

        var newRoot = ifStatement.Statement is BlockSyntax { Statements.Count: > 0, } block
            ? ReplaceWithBlockStatements(root, ifStatement, block)
            : root.RemoveNode(ifStatement, SyntaxRemoveOptions.KeepNoTrivia);

        if (newRoot is null) {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(newRoot)),
                title
            ),
            diagnostic
        );
    }

    private static SyntaxNode ReplaceWithBlockStatements(SyntaxNode root, IfStatementSyntax ifStatement, BlockSyntax block)
    {
        var statements = block.Statements;
        var firstStatement = statements[0]
            .WithLeadingTrivia(ifStatement.GetLeadingTrivia())
            .WithTrailingTrivia(block.GetTrailingTrivia());

        var newStatements = statements.Replace(statements[0], firstStatement);

        return root.ReplaceNode(ifStatement, newStatements);
    }
}
