using System.Collections.Generic;
using ScaffolderForOptimizely.Core.Catalog;

namespace ScaffolderForOptimizely.Core.Generation;

public sealed class GenerationResult
{
    public List<GeneratedFile> Files { get; } = new();

    /// <summary>Things the user should know (fallbacks, config errors). Shown in the dialog/Output pane.</summary>
    public List<string> Warnings { get; } = new();

    /// <summary>
    /// True when <c>dotnet new</c> was wanted but EPiServer.Templates isn't installed; the extension
    /// offers to install it.
    /// </summary>
    public bool DotnetTemplatePackMissing { get; set; }
}

public sealed class GeneratedFile
{
    public GeneratedFile(string outputId, OutputRole role, string fullPath, string relativePath, string content, string source)
    {
        OutputId = outputId;
        Role = role;
        FullPath = fullPath;
        RelativePath = relativePath;
        Content = content;
        Source = source;
    }

    public string OutputId { get; }

    public OutputRole Role { get; }

    public string FullPath { get; }

    /// <summary>Path relative to the project directory, with forward slashes.</summary>
    public string RelativePath { get; }

    public string Content { get; }

    /// <summary>Where the content came from: <c>override</c>, <c>dotnet-new</c> or <c>embedded</c>.</summary>
    public string Source { get; }

    public bool IsPrimary { get; set; }
}
