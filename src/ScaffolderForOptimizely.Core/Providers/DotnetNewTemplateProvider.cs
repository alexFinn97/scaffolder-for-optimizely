using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Core.Providers;

/// <summary>
/// Produces outputs with Optimizely's own item templates (<c>dotnet new</c>, EPiServer.Templates).
/// Only used when the config's template source is <c>dotnet-new</c> and the output maps to one of
/// the pack's templates; everything else falls through to the embedded templates.
/// </summary>
public sealed class DotnetNewTemplateProvider : ITemplateProvider
{
    public const string PackageId = "EPiServer.Templates";
    public const string InstallArguments = "new install " + PackageId;

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
    private static readonly object CacheLock = new();
    private static Task<bool>? _installedCheck;

    private readonly IProcessRunner _runner;

    public DotnetNewTemplateProvider(IProcessRunner runner)
    {
        _runner = runner;
    }

    public string Source => TemplateSources.DotnetNew;

    /// <summary>Forgets the cached "is the pack installed" answer (call after installing it).</summary>
    public static void ResetInstalledCache()
    {
        lock (CacheLock)
        {
            _installedCheck = null;
        }
    }

    public async Task<bool> IsPackInstalledAsync(CancellationToken cancellationToken)
    {
        Task<bool> check;
        lock (CacheLock)
        {
            check = _installedCheck ??= CheckInstalledAsync();
        }

        return await check.ConfigureAwait(false);
    }

    private async Task<bool> CheckInstalledAsync()
    {
        try
        {
            var result = await _runner.RunAsync("dotnet", "new list epi-cms-contenttype", Path.GetTempPath(), Timeout, CancellationToken.None).ConfigureAwait(false);
            return result.ExitCode == 0 && result.Output.IndexOf("epi-cms-contenttype", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or TimeoutException or InvalidOperationException)
        {
            return false;
        }
    }

    public async Task<string?> TryRenderAsync(PlannedOutput output, GenerationRequest request, ProviderLog log, CancellationToken cancellationToken)
    {
        var mapping = output.Definition.DotnetNew;
        if (mapping is null
            || request.PreviewOnly
            || !string.Equals(request.Config.Config.TemplateSource, TemplateSources.DotnetNew, StringComparison.OrdinalIgnoreCase)
            || request.Context.CmsProfile != Profiles.Cms12)
        {
            return null;
        }

        foreach (var key in mapping.UnsupportedWhenOptionsSet)
        {
            if (request.Options.TryGetValue(key, out var value)
                && !string.IsNullOrWhiteSpace(value)
                && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        if (mapping.RendersView && output.Model.TryGetValue("view_path", out var viewPath) && viewPath is string path && path.Length > 0)
        {
            // Feature folders need an explicit view path, which Optimizely's template doesn't write.
            return null;
        }

        if (!await IsPackInstalledAsync(cancellationToken).ConfigureAwait(false))
        {
            log.DotnetTemplatePackMissing = true;
            return null;
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "ScaffolderForOptimizely", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var arguments = $"new {mapping.ShortName} -n {output.ClassName} -o \"{tempDirectory}\" --namespace {output.Namespace}";
            if (mapping.PassBaseType && output.Model.TryGetValue("base_class", out var baseClass) && baseClass is string b && b.Length > 0)
            {
                // EPiServer.Templates 2.x exposes this option in lower case.
                arguments += $" --basetype {b}";
            }

            if (mapping.PassContentType && output.Model.TryGetValue("model_class", out var model) && model is string m && m.Length > 0)
            {
                arguments += $" --contentType {m}";
            }

            foreach (var pair in mapping.BoolArguments)
            {
                var on = request.Options.TryGetValue(pair.Key, out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                arguments += $" {pair.Value} {(on ? "true" : "false")}";
            }

            var result = await _runner.RunAsync("dotnet", arguments, tempDirectory, Timeout, cancellationToken).ConfigureAwait(false);
            var file = Path.Combine(tempDirectory, output.ClassName + ".cs");
            if (result.ExitCode != 0 || !File.Exists(file))
            {
                log.Warnings.Add($"dotnet new {mapping.ShortName} failed (exit code {result.ExitCode}); used the built-in template instead. {result.Output.Trim()}");
                return null;
            }

            return File.ReadAllText(file);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or TimeoutException or IOException)
        {
            log.Warnings.Add($"dotnet new {mapping.ShortName} could not run ({ex.Message}); used the built-in template instead.");
            return null;
        }
        finally
        {
            TryDelete(tempDirectory);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Temp folder; not worth failing over.
        }
    }
}
