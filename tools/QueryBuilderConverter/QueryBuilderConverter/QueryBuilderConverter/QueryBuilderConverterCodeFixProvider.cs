#nullable disable
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QueryBuilderConverter
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QueryBuilderConverterCodeFixProvider)), Shared]
    public class QueryBuilderConverterCodeFixProvider : CodeFixProvider
    {
        private const string title = "Replace with SQL($\"...\")";

        public override sealed ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(QueryBuilderConverterAnalyzer.DiagnosticId);

        public override sealed FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var cancellationToken = context.CancellationToken;
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var diagnosticSyntaxNode = root.FindNode(diagnosticSpan, false, true);


            if (QueryBuilderConverterAnalyzer.IsQueryBuilderCreate(diagnosticSyntaxNode, semanticModel,
                cancellationToken))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(title,
                        c =>
                            ConvertToSqlQueryInterpolation(document,
                                (InvocationExpressionSyntax) diagnosticSyntaxNode, c), title),
                    diagnostic);
            }
        }

        private async Task<Document> ConvertToSqlQueryInterpolation(Document document,
            InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            try
            {
                var queryBuilderCreateArgs = invocation.ArgumentList.Arguments;
                var replacementArguments = queryBuilderCreateArgs.Skip(1).Select(arg => arg.Expression).ToArray();
                var queryFormatLiteralSyntax = (LiteralExpressionSyntax) queryBuilderCreateArgs[0].Expression;
                var newInterpolatedString = ReplaceFormatStringWithInterpolation(queryFormatLiteralSyntax,
                    replacementArguments);

                var root = (CompilationUnitSyntax) await document.GetSyntaxRootAsync(cancellationToken);
                var newInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("SQL"),
                    SyntaxFactory.ArgumentList()
                        .AddArguments(SyntaxFactory.Argument(newInterpolatedString))
                    )
                    .WithLeadingTrivia(invocation.GetLeadingTrivia())
                    .WithTrailingTrivia(invocation.GetTrailingTrivia())
                    ;

                var rootWithSqlInterpolation = root.ReplaceNode(invocation, newInvocation);

                var hasUsingStaticProgressToolsSafeSql = rootWithSqlInterpolation.Usings.Any(ud =>
                    ud.StaticKeyword.Kind() != SyntaxKind.None
                    &&
                    ud.Name.ToString() == "ProgressOnderwijsUtils.SafeSql"
                    );

                var rootWithSqlInterpolationAndUsing =
                    hasUsingStaticProgressToolsSafeSql
                        ? rootWithSqlInterpolation
                        : rootWithSqlInterpolation.WithUsings(
                            rootWithSqlInterpolation.Usings.Add(SyntaxFactory.UsingDirective(
                                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                                null,
                                SyntaxFactory.ParseName("ProgressOnderwijsUtils.SafeSql")
                                )));

                return document.WithSyntaxRoot(rootWithSqlInterpolationAndUsing);
            }
            catch
            {
                return document;
            }
        }

        private static InterpolatedStringExpressionSyntax ReplaceFormatStringWithInterpolation(
            LiteralExpressionSyntax queryFormatLiteralSyntax, ExpressionSyntax[] replacementArguments)
        {
            var text = queryFormatLiteralSyntax.Token.ToString();

            //the hack here is that any string literal of a format string e.g. @"bla {2}" must be a valid string interpolation if prefixed by "$"
            var interpolatedString = (InterpolatedStringExpressionSyntax) SyntaxFactory.ParseExpression("$" + text);
            //in this new interpolation the literal 2 simply needs to be replaced by the argument at index 2.
            var newInterpolatedString = RewriteFakeFormatStringToInterpolation(interpolatedString, replacementArguments);
            return newInterpolatedString;
        }

        private static InterpolatedStringExpressionSyntax RewriteFakeFormatStringToInterpolation(
            InterpolatedStringExpressionSyntax interpolatedString,
            ExpressionSyntax[] replacementArguments)
            =>
                SyntaxFactory.InterpolatedStringExpression(
                    interpolatedString.StringStartToken,
                    SyntaxFactory.List(
                        interpolatedString.Contents
                            .Select(
                                iscs => ReplaceInterpolationArgument(iscs, replacementArguments))),
                    interpolatedString.StringEndToken
                    );

        private static InterpolatedStringContentSyntax ReplaceInterpolationArgument(
            InterpolatedStringContentSyntax iscs,
            ExpressionSyntax[] replacementArguments)
        {
            var interpolationSyntax = iscs as InterpolationSyntax;
            if (interpolationSyntax == null)
                return iscs;

            //must be a literal int since this was constructed from a format string
            var literalExpression = (LiteralExpressionSyntax) interpolationSyntax.Expression;
            var index = (int) literalExpression.Token.Value;

            return
                interpolationSyntax.WithExpression(
                    replacementArguments[index].WithoutLeadingTrivia().WithoutTrailingTrivia());
        }
    }
}
