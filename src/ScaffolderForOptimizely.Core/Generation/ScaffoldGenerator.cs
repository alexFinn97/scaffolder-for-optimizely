using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Providers;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>
/// Produces the files for a request. Doesn't touch the project: writing files and adding them to
/// the project is the extension's job.
/// </summary>
public sealed class ScaffoldGenerator
{
    private readonly IProcessRunner _processRunner;

    public ScaffoldGenerator(IProcessRunner? processRunner = null)
    {
        _processRunner = processRunner ?? ProcessRunner.Instance;
    }

    /// <summary>
    /// Providers in priority order: per-solution override, then <c>dotnet new</c>, then embedded.
    /// </summary>
    public IReadOnlyList<ITemplateProvider> ProvidersFor(GenerationRequest request) => new ITemplateProvider[]
    {
        new ScribanTemplateProvider(new OverrideTemplateStore(request.Config.RootDirectory), "override"),
        new DotnetNewTemplateProvider(_processRunner),
        new ScribanTemplateProvider(EmbeddedTemplateStore.Instance, TemplateSources.Embedded),
    };

    public async Task<GenerationResult> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var result = new GenerationResult();
        if (request.Config.Error is not null)
        {
            result.Warnings.Add($"Couldn't read {request.Config.FilePath}: {request.Config.Error}. Using defaults.");
        }

        var log = new ProviderLog();
        var providers = ProvidersFor(request);

        foreach (var output in OutputPlanner.Plan(request))
        {
            string? content = null;
            string source = string.Empty;
            foreach (var provider in providers)
            {
                content = await provider.TryRenderAsync(output, request, log, cancellationToken).ConfigureAwait(false);
                if (content is not null)
                {
                    source = provider.Source;
                    break;
                }
            }

            if (content is null)
            {
                result.Warnings.Add($"No template found for '{output.Definition.TemplateName}' ({request.Context.CmsProfile}).");
                continue;
            }

            content = output.IsCSharp
                ? CSharpPostProcessor.Process(content, output.RequiredUsings, FileScoped(request))
                : CSharpPostProcessor.NormalizeText(content);

            result.Files.Add(new GeneratedFile(output.Definition.Id, output.Definition.Role, output.FullPath, output.RelativePath, content, source)
            {
                IsPrimary = output.IsPrimary,
            });
        }

        result.Warnings.AddRange(log.Warnings);
        result.DotnetTemplatePackMissing = log.DotnetTemplatePackMissing;
        if (log.DotnetTemplatePackMissing)
        {
            result.Warnings.Add($"{DotnetNewTemplateProvider.PackageId} isn't installed, so the built-in templates were used.");
        }

        return result;
    }

    private static bool FileScoped(GenerationRequest request) =>
        request.Config.Config.FileScopedNamespaces ?? request.Context.FileScopedNamespaces;
}
