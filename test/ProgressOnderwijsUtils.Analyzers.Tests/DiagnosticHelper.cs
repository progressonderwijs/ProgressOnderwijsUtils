using System;
using System.Collections.Immutable;
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
    {
        var project = CreateProject(source);
        var compilation = project.GetCompilationAsync().Result.AssertNotNull();
        var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        return compilationWithAnalyzers.GetAllDiagnosticsAsync().Result.ToArray();
    }

    static Project CreateProject(string source)
    {
        var assemblyPath = new Uri(typeof(object).Assembly.Location).Combine("./");

        var projectId = ProjectId.CreateNewId();
        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(assemblyPath.Combine("System.Runtime.dll").LocalPath))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(Maybe).Assembly.Location));
        var documentId = DocumentId.CreateNewId(projectId);
        solution = solution.AddDocument(documentId, "Test.cs", SourceText.From(source));
        return solution.GetProject(projectId).AssertNotNull();
    }
}