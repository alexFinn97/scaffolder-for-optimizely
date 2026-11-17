using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ScaffolderForOptimizely.Core.Context;

public static class NamespaceHelper
{
    /// <summary>
    /// Namespace for a folder the same way Visual Studio does it: root namespace plus the folder
    /// path, each segment made into a valid identifier.
    /// </summary>
    public static string ForFolder(string rootNamespace, string projectDirectory, string folder)
    {
        var relative = GetRelativePath(projectDirectory, folder);
        if (relative.Length == 0)
        {
            return rootNamespace;
        }

        var segments = relative
            .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
            .Select(Sanitize);
        return string.Join(".", new[] { rootNamespace }.Concat(segments));
    }

    /// <summary>Folder path relative to the project directory ("" for the project itself).</summary>
    public static string GetRelativePath(string projectDirectory, string folder)
    {
        var root = Path.GetFullPath(projectDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var full = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (full.Equals(root, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        if (full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return full.Substring(root.Length + 1);
        }

        return string.Empty;
    }

    /// <summary>Turns a folder or project name into a valid C# identifier segment.</summary>
    public static string Sanitize(string segment)
    {
        var parts = segment.Split('.').Select(SanitizeIdentifier);
        return string.Join(".", parts);
    }

    private static string SanitizeIdentifier(string value)
    {
        var sb = new StringBuilder(value.Length + 1);
        foreach (var ch in value)
        {
            sb.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
        }

        if (sb.Length == 0 || char.IsDigit(sb[0]))
        {
            sb.Insert(0, '_');
        }

        return sb.ToString();
    }

    /// <summary>Namespace part of a full type name ("" when there is none).</summary>
    public static string NamespaceOf(string fullTypeName)
    {
        var index = fullTypeName.LastIndexOf('.');
        return index < 0 ? string.Empty : fullTypeName.Substring(0, index);
    }

    /// <summary>Type name without its namespace.</summary>
    public static string ShortName(string fullTypeName)
    {
        var index = fullTypeName.LastIndexOf('.');
        return index < 0 ? fullTypeName : fullTypeName.Substring(index + 1);
    }
}
