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

    /// <summary>
    /// Where view models live, relative to the project with forward slashes: the folder holding most
    /// existing <c>*ViewModel</c> classes, else a folder named ViewModels, else null.
    /// </summary>
    public string? ViewModelsFolder { get; set; }

    /// <summary>The project has a custom IViewLocationExpander, so views may be found outside MVC's defaults.</summary>
    public bool HasViewLocationExpander { get; set; }

    /// <summary>View path pattern learnt from existing page views, e.g. <c>Views/{name}/Index.cshtml</c>.</summary>
    public string? PageViewPattern { get; set; }

    /// <summary>View path pattern learnt from existing block views, e.g. <c>Views/Shared/Blocks/{name}.cshtml</c>.</summary>
    public string? BlockViewPattern { get; set; }

    /// <summary>Existing Razor views and their <c>@model</c>, used to learn the view patterns.</summary>
    public List<RazorViewInfo> RazorViews { get; } = new();

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

/// <summary>An existing Razor view: path relative to the project (forward slashes) and its <c>@model</c>.</summary>
public sealed class RazorViewInfo
{
    public RazorViewInfo(string relativePath, string modelType)
    {
        RelativePath = relativePath;
        ModelType = modelType;
    }

    public string RelativePath { get; }

    public string ModelType { get; }
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
