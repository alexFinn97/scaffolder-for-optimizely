using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ScaffolderForOptimizely.Compile.Tests;

/// <summary>Compiles C# in memory against everything this test host has loaded, Optimizely included.</summary>
internal static class TestCompiler
{
    private static readonly Lazy<IReadOnlyList<MetadataReference>> References = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList());

    public static CSharpCompilation Create(IEnumerable<(string Path, string Source)> files, bool nullable)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);
        var trees = files.Select(f => CSharpSyntaxTree.ParseText(f.Source, parseOptions, f.Path));
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithNullableContextOptions(nullable ? NullableContextOptions.Enable : NullableContextOptions.Disable)
            // Same as the SDK's default NoWarn: assembly version unification notices.
            .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
            {
                ["CS1701"] = ReportDiagnostic.Suppress,
                ["CS1702"] = ReportDiagnostic.Suppress,
            });

        var compilation = CSharpCompilation.Create("Contoso.Web", trees, References.Value, options);

        if (compilation.GetTypeByMetadataName("EPiServer.Core.PageData") is null)
        {
            throw new InvalidOperationException("EPiServer assemblies aren't visible to the test compiler.");
        }

        return compilation;
    }

    /// <summary>Errors and warnings, formatted with the offending file so failures are readable.</summary>
    public static IReadOnlyList<string> Problems(Compilation compilation) =>
        compilation.GetDiagnostics()
            .Where(d => d.Severity >= DiagnosticSeverity.Warning)
            .Select(d => $"{d.Location.SourceTree?.FilePath}({d.Location.GetLineSpan().StartLinePosition.Line + 1}): {d.Id} {d.GetMessage()}")
            .ToList();
}
