using System.Threading;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Core.Providers;

/// <summary>A source of file content. Providers are tried in order; the first that renders wins.</summary>
public interface ITemplateProvider
{
    /// <summary>Shown in the preview and Output pane, e.g. <c>dotnet-new</c>.</summary>
    string Source { get; }

    /// <summary>Renders the output, or returns null when this provider can't produce it.</summary>
    Task<string?> TryRenderAsync(PlannedOutput output, GenerationRequest request, ProviderLog log, CancellationToken cancellationToken);
}

/// <summary>Collects notes from providers during one generation.</summary>
public sealed class ProviderLog
{
    public System.Collections.Generic.List<string> Warnings { get; } = new();

    public bool DotnetTemplatePackMissing { get; set; }
}
