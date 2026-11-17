using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ScaffolderForOptimizely.Core.Context;

/// <summary>
/// File-system checks that don't need Roslyn: the root namespace from the project file and
/// whether views live under <c>Views/</c> or next to their classes (feature folders).
/// </summary>
public static class ProjectFileScanner
{
    private static readonly HashSet<string> IgnoredFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", "wwwroot", ".vs", ".git", "modules",
    };

    /// <summary>Creates a context with the root namespace and layout filled in.</summary>
    public static ProjectContext Scan(string projectFilePath)
    {
        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFilePath))!;
        var context = new ProjectContext(projectDirectory, ReadRootNamespace(projectFilePath));
        context.UsesFeatureFolders = DetectFeatureFolders(projectDirectory);
        context.ViewModelsFolder = FindViewModelsFolder(projectDirectory);
        context.RazorViews.AddRange(ReadRazorViews(projectDirectory));
        return context;
    }

    /// <summary>Every non-underscore .cshtml with an <c>@model</c> directive near the top.</summary>
    public static IEnumerable<RazorViewInfo> ReadRazorViews(string projectDirectory)
    {
        var root = projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var file in EnumerateFiles(root, "*.cshtml"))
        {
            if (Path.GetFileName(file).StartsWith("_", StringComparison.Ordinal))
            {
                continue;
            }

            var model = ReadModelDirective(file);
            if (model is not null)
            {
                yield return new RazorViewInfo(file.Substring(root.Length + 1).Replace('\\', '/'), model);
            }
        }
    }

    private static string? ReadModelDirective(string file)
    {
        try
        {
            foreach (var line in File.ReadLines(file).Take(40))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("@model ", StringComparison.Ordinal))
                {
                    return trimmed.Substring("@model ".Length).Trim();
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Unreadable view: just skip it.
        }

        return null;
    }

    /// <summary>
    /// The shallowest folder named <c>ViewModels</c> (any case), relative to the project with forward
    /// slashes; ties go to the alphabetically first path. Null when there is none.
    /// </summary>
    public static string? FindViewModelsFolder(string projectDirectory)
    {
        var root = projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return EnumerateDirectories(root)
            .Where(d => string.Equals(Path.GetFileName(d), "ViewModels", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.Substring(root.Length + 1).Replace('\\', '/'))
            .OrderBy(d => d.Count(c => c == '/'))
            .ThenBy(d => d, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    public static string ReadRootNamespace(string projectFilePath)
    {
        var fallback = Path.GetFileNameWithoutExtension(projectFilePath);
        try
        {
            var doc = XDocument.Load(projectFilePath);
            var value = doc.Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "RootNamespace" && !string.IsNullOrWhiteSpace(e.Value))
                ?.Value.Trim();
            if (!string.IsNullOrEmpty(value) && !value!.Contains("$("))
            {
                return value;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            // Fall back to the project name, which is the SDK default.
        }

        return NamespaceHelper.Sanitize(fallback);
    }

    /// <summary>
    /// True when more Razor views sit outside <c>Views/</c> and <c>Pages/</c> than inside them.
    /// </summary>
    public static bool DetectFeatureFolders(string projectDirectory)
    {
        int conventional = 0, feature = 0;
        foreach (var file in EnumerateFiles(projectDirectory, "*.cshtml"))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith("_", StringComparison.Ordinal))
            {
                continue;
            }

            var relative = file.Substring(projectDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var firstSegment = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
            if (firstSegment.Equals("Views", StringComparison.OrdinalIgnoreCase)
                || firstSegment.Equals("Pages", StringComparison.OrdinalIgnoreCase)
                || firstSegment.Equals("Areas", StringComparison.OrdinalIgnoreCase))
            {
                conventional++;
            }
            else
            {
                feature++;
            }
        }

        return feature > conventional;
    }

    private static IEnumerable<string> EnumerateDirectories(string directory)
    {
        var pending = new Stack<string>();
        pending.Push(directory);
        while (pending.Count > 0)
        {
            string[] dirs;
            try
            {
                dirs = Directory.GetDirectories(pending.Pop());
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var dir in dirs.Where(d => !IgnoredFolders.Contains(Path.GetFileName(d))))
            {
                yield return dir;
                pending.Push(dir);
            }
        }
    }

    private static IEnumerable<string> EnumerateFiles(string directory, string pattern)
    {
        var pending = new Stack<string>();
        pending.Push(directory);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            string[] files, dirs;
            try
            {
                files = Directory.GetFiles(current, pattern);
                dirs = Directory.GetDirectories(current);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (var dir in dirs)
            {
                if (!IgnoredFolders.Contains(Path.GetFileName(dir)))
                {
                    pending.Push(dir);
                }
            }
        }
    }
}
