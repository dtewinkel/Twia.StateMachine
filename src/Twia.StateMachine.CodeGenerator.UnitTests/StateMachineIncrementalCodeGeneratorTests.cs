using ktsu.Frontmatter;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Twia.StateMachine.CodeGenerator.UnitTests.Verifiers;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class StateMachineIncrementalCodeGeneratorTests
{
    private readonly TestContext _testContext;
    private readonly IncrementalGeneratorVerifier<StateMachineIncrementalCodeGenerator> _verifier = new(typeof(StateMachineIncrementalCodeGeneratorTests));

    public StateMachineIncrementalCodeGeneratorTests(TestContext testContext)
    {
        _testContext = testContext;
        _verifier.AddAdditionalFileReferences("Twia.StateMachine.dll");
    }

    private static readonly string _unitTestFilesDir = Path.Combine("TestFiles");

    public static IEnumerable<object[]> InitialStateTestCases()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, _unitTestFilesDir);
        if (!Directory.Exists(dir))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".e.cs", StringComparison.OrdinalIgnoreCase) 
                           && !f.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f))
        {
            var relative = Path.GetRelativePath(dir, file);
            yield return [relative];
        }
    }

    public static string InitialStateDisplayName(MethodInfo _, object[] data)
    {
        var relativeFileName = (string)data[0];
        var path = Path.Combine(AppContext.BaseDirectory, _unitTestFilesDir, relativeFileName);
        var subDir = Path.GetDirectoryName(relativeFileName)?.Replace($"{Path.DirectorySeparatorChar}", " - ") ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(relativeFileName);
        if (File.Exists(path))
        {
            var testFileInfo = ParseTestSource(relativeFileName, false);
            if (testFileInfo is not null)
            {
                name = testFileInfo.Name;
            }
        }
        return string.IsNullOrEmpty(subDir) ? name : $"{subDir}: {name}";
    }

        [TestMethod]
    [DynamicData(nameof(InitialStateTestCases), DynamicDataDisplayName = nameof(InitialStateDisplayName))]
    public async Task Generator_FromSourceFile_GeneratesCorrectResult(string relativeFileName)
    {
        var testInfo = ParseTestSource(relativeFileName, true)!;

        _verifier.OutputFile = testInfo.OutputFile;
        await _verifier.VerifyGeneratorAsync([testInfo.Code], [.. testInfo.Diagnostics], [.. testInfo.GeneratedSources], _testContext.CancellationToken);
    }

    private sealed record TestFileInfo(string Name, string Code, string? OutputFile, List<DiagnosticResult> Diagnostics, List<(string, string)> GeneratedSources);

    private enum OutputType
    {
        None = 1,
        Source
    }

    private static TestFileInfo? ParseTestSource(string relativeFileName, bool failOnError)
    {
        var path = Path.Combine(AppContext.BaseDirectory, _unitTestFilesDir, relativeFileName);

        var codeWithFrontMatter = File.ReadAllText(path);
        var frontmatter = Frontmatter.ExtractFrontmatter(codeWithFrontMatter);

        if (frontmatter is null)
        {
            if (failOnError)
            {
                Assert.Fail($"Test file '{relativeFileName}' is missing frontmatter.");
            }

            return null;
        }

        var code = Frontmatter.RemoveFrontmatter(codeWithFrontMatter);

        if (frontmatter["Name"] is not string name)
        {
            if (failOnError)
            {
                Assert.Fail(
                    $"Test file '{relativeFileName}' has no Name set. Expected 'Name: ' followed by a descriptive name.");
            }

            return null;
        }

        if (frontmatter["Output"] is not string outputText)
        {
            if (failOnError)
            {
                Assert.Fail(
                    $"Test file '{relativeFileName}' has no Output set, or output is invalid. Expected 'Output: ' followed by 'Source' or 'None'.");
            }

            return null;
        }

        if (!Enum.TryParse<OutputType>(outputText, ignoreCase: true, out var output))
        {
            if (failOnError)
            {
                Assert.Fail(
                    $"Test file '{relativeFileName}' has invalid Output '{outputText}'. Expected 'Source' or 'None'.");
            }

            return null;
        }

        var diagnostics = new List<DiagnosticResult>();
        if (frontmatter.TryGetValue("Diagnostics", out var value))
        {
            var diagnosticsMatter = (IEnumerable<object?>)value;

            var index = 1;
            foreach (var diagnostic in diagnosticsMatter)
            {
                if (diagnostic is not string diagnosticString)
                {
                    diagnostics.Add(DiagnosticResult
                        .CompilerError("<invalid>")
                        .WithLocation(index, 1)
                        .WithArguments(diagnostic?.GetType().FullName ?? "<null>"));
                    continue;
                }

                var parts = diagnosticString.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length < 3)
                {
                    diagnostics.Add(DiagnosticResult
                        .CompilerError("<invalid>")
                        .WithLocation(index, 2)
                        .WithArguments(diagnosticString));
                    continue;
                }

                var diagnosticId = new string([.. parts[0].Where(c => !char.IsWhiteSpace(c))]);
                var result = DiagnosticResult.CompilerError(diagnosticId);
                if (int.TryParse(parts[1], out var line) && int.TryParse(parts[2], out var column))
                {
                    result = result.WithLocation(line, column);
                }

                if (parts.Length > 3)
                {
                    result = result.WithArguments([.. parts[3..].Select(p => p.Trim('"'))]);
                }

                diagnostics.Add(result);
            }
        }

        List<(string, string)> generatedSources = [];
        if (output == OutputType.Source)
        {
            var expectedPath = Path.ChangeExtension(path, ".e.cs");
            if (!File.Exists(expectedPath))
            {
                if (failOnError)
                {
                    Assert.Fail($"Expected generated source file '{Path.GetFileName(expectedPath)}' is missing for test '{relativeFileName}'.");
                }
                return null;
            }

            var expectedCode = File.ReadAllText(expectedPath);
            generatedSources.Add(("*UnitTestStateMachine*", expectedCode));
        }

        var outputFile = Path.ChangeExtension(path, ".g.cs");
        Console.WriteLine($"Output in file: {outputFile}");
        Console.WriteLine();
        return new TestFileInfo(name, code, outputFile, [.. diagnostics], [.. generatedSources]);
    }
}
