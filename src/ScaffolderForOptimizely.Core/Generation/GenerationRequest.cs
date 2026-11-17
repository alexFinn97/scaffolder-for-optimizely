using System.Collections.Generic;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>Everything needed to scaffold one item.</summary>
public sealed class GenerationRequest
{
    public GenerationRequest(
        ItemDefinition item,
        string name,
        string targetDirectory,
        IDictionary<string, string> options,
        ProjectContext context,
        LoadedConfig config)
    {
        Item = item;
        Name = name;
        TargetDirectory = targetDirectory;
        Options = options;
        Context = context;
        Config = config;
    }

    public ItemDefinition Item { get; }

    /// <summary>Class name as typed; the generator adds a missing suffix.</summary>
    public string Name { get; }

    /// <summary>Folder that was right-clicked (or the project folder).</summary>
    public string TargetDirectory { get; }

    /// <summary>Option values keyed by <see cref="OptionKeys"/>.</summary>
    public IDictionary<string, string> Options { get; }

    public ProjectContext Context { get; }

    public LoadedConfig Config { get; }

    /// <summary>
    /// When true, never run <c>dotnet new</c> (used for the live preview, so typing stays fast).
    /// </summary>
    public bool PreviewOnly { get; set; }
}
