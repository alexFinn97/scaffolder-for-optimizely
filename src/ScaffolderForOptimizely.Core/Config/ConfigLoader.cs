using System;
using System.IO;
using Newtonsoft.Json;

namespace ScaffolderForOptimizely.Core.Config;

/// <summary>The loaded config plus where it came from.</summary>
public sealed class LoadedConfig
{
    public LoadedConfig(ScaffolderConfig config, string? filePath, string? error)
    {
        Config = config;
        FilePath = filePath;
        Error = error;
    }

    public ScaffolderConfig Config { get; }

    /// <summary>Path of the config file, or null when none was found (defaults are used).</summary>
    public string? FilePath { get; }

    /// <summary>Parse error, if the file exists but couldn't be read. Defaults are used.</summary>
    public string? Error { get; }

    /// <summary>
    /// Directory that holds per-solution template overrides: the config file's directory, or the
    /// fallback directory passed to <see cref="ConfigLoader.Load"/>.
    /// </summary>
    public string? RootDirectory { get; set; }
}

public static class ConfigLoader
{
    /// <summary>
    /// Looks for <c>.optimizely-scaffolder.json</c> in <paramref name="startDirectory"/> and its parents,
    /// stopping at <paramref name="stopDirectory"/> (normally the solution folder) or a <c>.git</c> folder.
    /// </summary>
    public static LoadedConfig Load(string startDirectory, string? stopDirectory)
    {
        var path = Find(startDirectory, stopDirectory);
        if (path is null)
        {
            return new LoadedConfig(new ScaffolderConfig(), null, null)
            {
                RootDirectory = stopDirectory ?? startDirectory,
            };
        }

        try
        {
            var config = JsonConvert.DeserializeObject<ScaffolderConfig>(File.ReadAllText(path)) ?? new ScaffolderConfig();
            config.Folders ??= new FolderConfig();
            config.BaseClasses ??= new BaseClassConfig();
            config.Companions ??= new CompanionConfig();
            config.ViewPaths ??= new ViewPathConfig();
            return new LoadedConfig(config, path, null) { RootDirectory = Path.GetDirectoryName(path) };
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return new LoadedConfig(new ScaffolderConfig(), path, ex.Message) { RootDirectory = Path.GetDirectoryName(path) };
        }
    }

    private static string? Find(string startDirectory, string? stopDirectory)
    {
        var stop = stopDirectory is null ? null : Normalize(stopDirectory);
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, ScaffolderConfig.FileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            if ((stop is not null && string.Equals(Normalize(dir.FullName), stop, StringComparison.OrdinalIgnoreCase))
                || Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return null;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static string Normalize(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
