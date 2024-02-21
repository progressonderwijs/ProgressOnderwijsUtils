using System;
using System.Linq;
using Microsoft.CodeAnalysis;
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
