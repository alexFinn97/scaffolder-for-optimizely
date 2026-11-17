using System.Collections.Generic;

namespace ScaffolderForOptimizely.Core.Context;

/// <summary>
/// What the extension knows about the project being scaffolded into. Filled by
/// <see cref="CompilationAnalyzer"/> and <see cref="ProjectFileScanner"/>; every list may be empty.
/// </summary>
public sealed class ProjectContext
{
    public ProjectContext(string projectDirectory, string rootNamespace)
    {
        ProjectDirectory = projectDirectory;
        RootNamespace = rootNamespace;
    }

    public string ProjectDirectory { get; }

    public string RootNamespace { get; set; }

    /// <summary>Template profile for the CMS version, e.g. <c>cms12</c>.</summary>
    public string CmsProfile { get; set; } = Profiles.Cms12;

    /// <summary>Major version of the referenced EPiServer assembly, or null if not referenced.</summary>
    public int? CmsMajorVersion { get; set; }

    /// <summary>Major version of the referenced Commerce assembly, or null if not referenced.</summary>
    public int? CommerceMajorVersion { get; set; }

    public bool FileScopedNamespaces { get; set; } = true;

    public bool NullableEnabled { get; set; }

    /// <summary>True when existing views sit next to their classes rather than under Views/.</summary>
    public bool UsesFeatureFolders { get; set; }

    /// <summary>Abstract page base classes declared in the project (full names).</summary>
    public List<string> PageBaseClasses { get; } = new();

    public List<string> BlockBaseClasses { get; } = new();

    public List<string> MediaBaseClasses { get; } = new();

    /// <summary>Concrete page types declared in the project (full names).</summary>
    public List<string> PageTypes { get; } = new();

    public List<string> BlockTypes { get; } = new();

    /// <summary>Constants that can be used as a content type group.</summary>
    public List<GroupNameCandidate> GroupNames { get; } = new();
}

public static class Profiles
{
    public const string Cms12 = "cms12";
    public const string Cms13 = "cms13";
}

/// <summary>A string constant usable as a GroupName, e.g. <c>Globals.GroupNames.Specialized</c>.</summary>
public sealed class GroupNameCandidate
{
    public GroupNameCandidate(string @namespace, string expression)
    {
        Namespace = @namespace;
        Expression = expression;
    }

    /// <summary>Namespace that must be imported for <see cref="Expression"/> to compile.</summary>
    public string Namespace { get; }

    /// <summary>C# expression relative to <see cref="Namespace"/>, e.g. <c>Globals.GroupNames.Content</c>.</summary>
    public string Expression { get; }
}
