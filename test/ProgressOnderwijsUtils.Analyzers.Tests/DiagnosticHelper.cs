using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ProgressOnderwijsUtils.Collections;

namespace ProgressOnderwijsUtils.Analyzers.Tests;

public static class DiagnosticHelper
{
    public static Diagnostic[] GetDiagnostics(DiagnosticAnalyzer analyzer, string source)
        => GetDiagnostics(analyzer, CreateProjectWithTestFile(source));

    public static Diagnostic[] GetDiagnostics(DiagnosticAnalyzer analyzer, AdhocWorkspace workspace)
    {
        var project = workspace.CurrentSolution.Projects.Single();
        var compilation = project.GetCompilationAsync().GetAwaiter().GetResult().AssertNotNull();
        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer,]);
        return [.. compilationWithAnalyzers.GetAllDiagnosticsAsync().GetAwaiter().GetResult(),];
    }

    public static async Task<int> ApplyAllCodeFixes(AdhocWorkspace workspace, Diagnostic diagnostic, CodeFixProvider codeFixProvider)
    {
        var document = workspace.CurrentSolution.GetDocument(diagnostic.Location.SourceTree) ?? throw new($"Could not resolve {diagnostic.Location}");
        var codeActions = new List<CodeAction>();

        var context = new CodeFixContext(document, diagnostic, (codeAction, _) => codeActions.Add(codeAction), CancellationToken.None);
        await codeFixProvider.RegisterCodeFixesAsync(context);
        var appliedCount = 0;
        foreach (var codeAction in codeActions) {
            var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
            if (operations.IsDefaultOrEmpty) {
                continue;
            }

            var changedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            if (!workspace.TryApplyChanges(changedSolution)) {
                throw new();
            }
            appliedCount++;
        }
        return appliedCount;
    }

    public static AdhocWorkspace CreateProjectWithTestFile(string source)
    {
        var assemblyPath = new Uri(typeof(object).Assembly.Location).Combine("./");

        var projectId = ProjectId.CreateNewId();
        var adhocWorkspace = new AdhocWorkspace();

        var solution = adhocWorkspace
            .CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(assemblyPath.Combine("System.Runtime.dll").LocalPath))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Console).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Maybe).Assembly.Location));
        var documentId = DocumentId.CreateNewId(projectId);
        if (!adhocWorkspace.TryApplyChanges(solution.AddDocument(documentId, "Test.cs", SourceText.From(source)))) {
            throw new();
        }
        return adhocWorkspace;
    }
}
