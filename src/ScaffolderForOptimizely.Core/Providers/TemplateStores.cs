using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Core.Providers;

/// <summary>Looks up Scriban template text by profile and name.</summary>
public interface ITemplateStore
{
    /// <summary>Returns the template text and a display path, or null when it doesn't exist.</summary>
    (string Text, string Path)? TryGet(string profile, string templateName);
}

/// <summary>Profiles to try, newest first, e.g. cms13 → cms12.</summary>
internal static class ProfileChain
{
    public static IEnumerable<string> For(string profile)
    {
        yield return profile;
        if (profile != Profiles.Cms12)
        {
            yield return Profiles.Cms12;
        }
    }
}

/// <summary>Templates compiled into the extension (<c>Templates/{profile}/{name}</c>).</summary>
public sealed class EmbeddedTemplateStore : ITemplateStore
{
    private static readonly Assembly Assembly = typeof(EmbeddedTemplateStore).Assembly;
    private static readonly Lazy<Dictionary<string, string>> ResourceNames = new(() =>
        Assembly.GetManifestResourceNames().ToDictionary(n => n.Replace('\\', '/'), n => n, StringComparer.OrdinalIgnoreCase));

    public static EmbeddedTemplateStore Instance { get; } = new();

    public (string Text, string Path)? TryGet(string profile, string templateName)
    {
        foreach (var candidate in ProfileChain.For(profile))
        {
            var key = $"Templates/{candidate}/{templateName}";
            if (ResourceNames.Value.TryGetValue(key, out var resource))
            {
                using var stream = Assembly.GetManifestResourceStream(resource)!;
                using var reader = new StreamReader(stream);
                return (reader.ReadToEnd(), "embedded:" + key);
            }
        }

        return null;
    }

    /// <summary>All embedded template paths (for docs/tests).</summary>
    public static IEnumerable<string> List() => ResourceNames.Value.Keys.OrderBy(k => k, StringComparer.Ordinal);
}

/// <summary>Per-solution overrides in <c>.optimizely/templates/{profile}/{name}</c>.</summary>
public sealed class OverrideTemplateStore : ITemplateStore
{
    public const string RelativeFolder = ".optimizely/templates";

    private readonly string? _rootDirectory;

    public OverrideTemplateStore(string? rootDirectory)
    {
        _rootDirectory = rootDirectory;
    }

    public (string Text, string Path)? TryGet(string profile, string templateName)
    {
        if (string.IsNullOrEmpty(_rootDirectory))
        {
            return null;
        }

        foreach (var candidate in ProfileChain.For(profile))
        {
            var path = System.IO.Path.Combine(_rootDirectory!, ".optimizely", "templates", candidate, templateName);
            if (File.Exists(path))
            {
                return (File.ReadAllText(path), path);
            }
        }

        return null;
    }
}
