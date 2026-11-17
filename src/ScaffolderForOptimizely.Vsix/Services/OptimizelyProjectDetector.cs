using System;
using System.Collections.Concurrent;
using System.IO;

namespace ScaffolderForOptimizely.Services;

/// <summary>
/// Decides whether a project uses Optimizely, so the menu only shows where it's useful. Checks the
/// project file, then the restore output (which also covers packages that arrive transitively, e.g.
/// through a shared Models project). Results are cached until either file changes.
/// </summary>
internal static class OptimizelyProjectDetector
{
    private static readonly ConcurrentDictionary<string, (DateTime Stamp, bool Result)> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsOptimizelyProject(string projectFilePath)
    {
        if (string.IsNullOrEmpty(projectFilePath) || !File.Exists(projectFilePath))
        {
            return false;
        }

        var assetsPath = Path.Combine(Path.GetDirectoryName(projectFilePath)!, "obj", "project.assets.json");
        var stamp = Max(File.GetLastWriteTimeUtc(projectFilePath), File.Exists(assetsPath) ? File.GetLastWriteTimeUtc(assetsPath) : DateTime.MinValue);

        if (Cache.TryGetValue(projectFilePath, out var cached) && cached.Stamp == stamp)
        {
            return cached.Result;
        }

        var result = Contains(projectFilePath, "EPiServer.") || (File.Exists(assetsPath) && Contains(assetsPath, "\"EPiServer.CMS.Core/"));
        Cache[projectFilePath] = (stamp, result);
        return result;
    }

    private static bool Contains(string path, string value)
    {
        try
        {
            using var reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (line.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Treat unreadable files as "not Optimizely".
        }

        return false;
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
}
