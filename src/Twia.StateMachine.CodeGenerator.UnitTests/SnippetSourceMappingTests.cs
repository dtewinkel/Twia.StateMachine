using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class SnippetSourceMappingTests
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);
    private static readonly MetadataReference[] References =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Append(typeof(StateMachineAttribute).Assembly.Location)
        .Distinct()
        .Select(path => MetadataReference.CreateFromFile(path))
        .ToArray();

    [TestMethod]
    [DataRow("OnEntry(\"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("OnExit(action: \"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("Transition(nameof(Fire), nameof(Ready), Action = \"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("TransitionAfter(\"00:00:01\", nameof(Ready), Action = \"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("TriggerlessTransition(nameof(Ready), Action = \"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("InternalTransition(action: \"MissingAction()\", trigger: nameof(Fire), Condition = \"MissingCondition\")")]
    [DataRow("InternalAfter(\"00:00:01\", \"MissingAction()\", Condition = \"MissingCondition\")")]
    [DataRow("InternalAfterEvery(\"00:00:01\", \"MissingAction()\", Condition = \"MissingCondition\")")]
    public void CompilerErrors_MapToAttributeArguments(string attribute)
    {
        var source = CreateSource(attribute);
        var path = "C:\\source\\Machine.cs";
        var (output, generated, generatorDiagnostics) = Generate(source, path);

        Assert.IsEmpty(generatorDiagnostics);
        var errors = output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(2, errors);
        Assert.IsTrue(errors.All(d => d.Id == "CS0103"));
        var expectedLine = source[..source.IndexOf("[" + attribute, StringComparison.Ordinal)].Count(c => c == '\n');
        foreach (var error in errors)
        {
            var span = error.Location.GetMappedLineSpan();
            Assert.AreEqual(path, span.Path);
            Assert.AreEqual(expectedLine, span.StartLinePosition.Line);
        }

        Assert.AreEqual(2, generated.Split("#line default", StringSplitOptions.None).Length - 1);
    }

    [TestMethod]
    public void ValidSnippets_KeepAccessToPrivateClassContext()
    {
        var source = CreateSource("OnEntry(\"Run()\", Condition = \"Enabled\")");
        var (output, _, generatorDiagnostics) = Generate(source, "Machine.cs");

        Assert.IsEmpty(generatorDiagnostics);
        Assert.IsEmpty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray());
    }

    [TestMethod]
    public void MultilineAttribute_MapsEachArgumentSeparately()
    {
        var source = CreateSource("OnEntry(\n        action: \"MissingAction();\\nMissingSecondAction()\",\n        Condition = \"42\")");
        var (output, _, generatorDiagnostics) = Generate(source, "Machine.cs");

        Assert.IsEmpty(generatorDiagnostics);
        var errors = output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.HasCount(3, errors);
        var actionLine = source[..source.IndexOf("action:", StringComparison.Ordinal)].Count(c => c == '\n');
        var conditionLine = source[..source.IndexOf("Condition =", StringComparison.Ordinal)].Count(c => c == '\n');
        var conditionError = errors.Single(d => d.Id == "CS0029");
        Assert.AreEqual(conditionLine, conditionError.Location.GetMappedLineSpan().StartLinePosition.Line);
        var actionErrors = errors.Where(d => d.Id == "CS0103")
            .OrderBy(d => d.Location.GetMappedLineSpan().StartLinePosition.Line).ToArray();
        Assert.HasCount(2, actionErrors);
        Assert.AreEqual(actionLine, actionErrors[0].Location.GetMappedLineSpan().StartLinePosition.Line);
        Assert.AreEqual(actionLine + 1, actionErrors[1].Location.GetMappedLineSpan().StartLinePosition.Line);
        Assert.IsTrue(errors.All(d => d.Location.GetMappedLineSpan().Path == "Machine.cs"));
    }

    [TestMethod]
    public void Mapping_RespectsExistingSourceDirectivesAndRestoresGeneratedLocation()
    {
        var source = CreateSource("OnEntry(\"MissingAction()\", Condition = \"MissingCondition\")")
            .Replace("    [OnEntry", "#line 100 \"Original.cs\"\n    [OnEntry", StringComparison.Ordinal)
            .Replace("    private partial void Ready();", "#line default\n    private partial void Ready();", StringComparison.Ordinal);
        var (output, generated, _) = Generate(source, "Machine.cs");

        foreach (var error in output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            var span = error.Location.GetMappedLineSpan();
            Assert.AreEqual("Original.cs", span.Path);
            Assert.AreEqual(99, span.StartLinePosition.Line);
        }

        var generatedTree = output.SyntaxTrees.Single(t => t.GetText().ToString() == generated);
        var generatedMethod = generatedTree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .Single(m => m.Identifier.ValueText == "Ready");
        var closeBraceLocation = generatedMethod.Body!.CloseBraceToken.GetLocation();
        Assert.AreEqual(closeBraceLocation.GetLineSpan(), closeBraceLocation.GetMappedLineSpan());
    }

    [TestMethod]
    public void Mapping_PreservesWindowsPathsWithSpaces()
    {
        var path = "C:\\source folder\\Machine.cs";
        var (output, _, generatorDiagnostics) = Generate(CreateSource("OnEntry(\"MissingAction()\")"), path);

        Assert.IsEmpty(generatorDiagnostics);
        var error = output.GetDiagnostics().Single(d => d.Severity == DiagnosticSeverity.Error);
        Assert.AreEqual("CS0103", error.Id);
        Assert.AreEqual(path, error.Location.GetMappedLineSpan().Path);
    }

    [TestMethod]
    public void Mapping_UpdatesWhenAttributeMoves()
    {
        var source = CreateSource("OnEntry(\"MissingAction()\")");
        var originalTree = CSharpSyntaxTree.ParseText(source, ParseOptions, "Machine.cs");
        var compilation = CreateCompilation(originalTree);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new StateMachineIncrementalCodeGenerator().AsSourceGenerator()], parseOptions: ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var movedTree = CSharpSyntaxTree.ParseText("\n\n" + source, ParseOptions, "Machine.cs");
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation.ReplaceSyntaxTree(originalTree, movedTree), out var output, out var diagnostics);

        Assert.IsEmpty(diagnostics);
        var error = output.GetDiagnostics().Single(d => d.Severity == DiagnosticSeverity.Error);
        var expectedLine = source[..source.IndexOf("[OnEntry", StringComparison.Ordinal)].Count(c => c == '\n') + 2;
        Assert.AreEqual(expectedLine, error.Location.GetMappedLineSpan().StartLinePosition.Line);
    }

    private static string CreateSource(string attribute) => $$"""
        using Twia.StateMachine;
        [StateMachine]
        public partial class Machine
        {
            private bool Enabled => true;
            private void Run() { }
            [InitialState]
            [{{attribute}}]
            private partial void Ready();
            [Trigger]
            public partial void Fire();
        }
        """;

    private static CSharpCompilation CreateCompilation(SyntaxTree tree) => CSharpCompilation.Create(
        "SnippetMappings", [tree], References,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    private static (Compilation Output, string Generated, IReadOnlyList<Diagnostic> Diagnostics) Generate(string source, string path)
    {
        var tree = CSharpSyntaxTree.ParseText(source, ParseOptions, path);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new StateMachineIncrementalCodeGenerator().AsSourceGenerator()], parseOptions: ParseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(CreateCompilation(tree), out var output, out var diagnostics);
        var generated = driver.GetRunResult().Results.Single().GeneratedSources.Single().SourceText.ToString();
        return (output, generated, diagnostics);
    }
}
