using Newtonsoft.Json;

namespace ScaffolderForOptimizely.Core.Config;

/// <summary>
/// Per-solution settings from <c>.optimizely-scaffolder.json</c>. Every value is optional:
/// anything left out falls back to what the extension detects in the project.
/// </summary>
public sealed class ScaffolderConfig
{
    public const string FileName = ".optimizely-scaffolder.json";

    /// <summary><c>dotnet-new</c> (wrap Optimizely's EPiServer.Templates where possible) or <c>embedded</c>.</summary>
    [JsonProperty("templateSource")]
    public string TemplateSource { get; set; } = TemplateSources.DotnetNew;

    /// <summary><c>conventional</c> (Models/Controllers/Views) or <c>feature</c> (everything in one folder). Null = detect.</summary>
    [JsonProperty("layout")]
    public string? Layout { get; set; }

    /// <summary>Overrides the project's root namespace.</summary>
    [JsonProperty("rootNamespace")]
    public string? RootNamespace { get; set; }

    /// <summary>Use file-scoped namespaces. Null = detect from existing code.</summary>
    [JsonProperty("fileScopedNamespaces")]
    public bool? FileScopedNamespaces { get; set; }

    [JsonProperty("folders")]
    public FolderConfig Folders { get; set; } = new();

    [JsonProperty("baseClasses")]
    public BaseClassConfig BaseClasses { get; set; } = new();

    /// <summary>Default content type group expression, e.g. <c>Globals.GroupNames.Content</c>.</summary>
    [JsonProperty("contentTypeGroup")]
    public string? ContentTypeGroup { get; set; }

    /// <summary><c>async</c>, <c>sync</c> or <c>viewComponent</c>.</summary>
    [JsonProperty("blockRenderStyle")]
    public string? BlockRenderStyle { get; set; }

    [JsonProperty("companions")]
    public CompanionConfig Companions { get; set; } = new();

    /// <summary>Razor view path patterns; null = detect from existing views.</summary>
    [JsonProperty("viewPaths")]
    public ViewPathConfig ViewPaths { get; set; } = new();

    /// <summary>
    /// Default for "Pass view path to View()". Null = only when the view isn't where MVC looks by
    /// default and the project has no IViewLocationExpander.
    /// </summary>
    [JsonProperty("passViewPath")]
    public bool? PassViewPath { get; set; }
}

/// <summary>View path patterns relative to the project. <c>{name}</c> is the content type's class name.</summary>
public sealed class ViewPathConfig
{
    /// <summary>e.g. <c>Views/Pages/{name}.cshtml</c>.</summary>
    [JsonProperty("page")]
    public string? Page { get; set; }

    /// <summary>e.g. <c>Views/Shared/Blocks/{name}.cshtml</c>.</summary>
    [JsonProperty("block")]
    public string? Block { get; set; }
}

public static class TemplateSources
{
    public const string DotnetNew = "dotnet-new";
    public const string Embedded = "embedded";
}

public static class Layouts
{
    public const string Conventional = "conventional";
    public const string Feature = "feature";
}

/// <summary>Folders relative to the project directory, used in the conventional layout.</summary>
public sealed class FolderConfig
{
    [JsonProperty("pages")]
    public string Pages { get; set; } = "Models/Pages";

    [JsonProperty("blocks")]
    public string Blocks { get; set; } = "Models/Blocks";

    [JsonProperty("media")]
    public string Media { get; set; } = "Models/Media";

    [JsonProperty("controllers")]
    public string Controllers { get; set; } = "Controllers";

    [JsonProperty("components")]
    public string Components { get; set; } = "Components";

    /// <summary>Null = use an existing ViewModels folder in the project, else <c>Models/ViewModels</c>.</summary>
    [JsonProperty("viewModels")]
    public string? ViewModels { get; set; }

    [JsonProperty("views")]
    public string Views { get; set; } = "Views";

    // Infrastructure and editor items: null = where existing implementations live, else Alloy-style.
    [JsonProperty("initialization")]
    public string? Initialization { get; set; }

    [JsonProperty("startupFilters")]
    public string? StartupFilters { get; set; }

    [JsonProperty("scheduledJobs")]
    public string? ScheduledJobs { get; set; }

    [JsonProperty("serviceExtensions")]
    public string? ServiceExtensions { get; set; }

    [JsonProperty("selectionFactories")]
    public string? SelectionFactories { get; set; }

    [JsonProperty("editorDescriptors")]
    public string? EditorDescriptors { get; set; }

    [JsonProperty("visitorGroups")]
    public string? VisitorGroups { get; set; }

    [JsonProperty("contentEvents")]
    public string? ContentEvents { get; set; }

    [JsonProperty("validators")]
    public string? Validators { get; set; }

    /// <summary>The configured folder for a <see cref="Catalog.FolderKeys"/> key, or null.</summary>
    public string? ForKey(string key) => key switch
    {
        Catalog.FolderKeys.Initialization => Initialization,
        Catalog.FolderKeys.StartupFilters => StartupFilters,
        Catalog.FolderKeys.ScheduledJobs => ScheduledJobs,
        Catalog.FolderKeys.ServiceExtensions => ServiceExtensions,
        Catalog.FolderKeys.SelectionFactories => SelectionFactories,
        Catalog.FolderKeys.EditorDescriptors => EditorDescriptors,
        Catalog.FolderKeys.VisitorGroups => VisitorGroups,
        Catalog.FolderKeys.ContentEvents => ContentEvents,
        Catalog.FolderKeys.Validators => Validators,
        _ => null,
    };
}

/// <summary>Default base classes (full type names). Null = pick from detection.</summary>
public sealed class BaseClassConfig
{
    [JsonProperty("page")]
    public string? Page { get; set; }

    [JsonProperty("block")]
    public string? Block { get; set; }

    [JsonProperty("media")]
    public string? Media { get; set; }
}

/// <summary>Default state of the companion checkboxes. Null = item default.</summary>
public sealed class CompanionConfig
{
    [JsonProperty("controller")]
    public bool? Controller { get; set; }

    [JsonProperty("component")]
    public bool? Component { get; set; }

    [JsonProperty("viewModel")]
    public bool? ViewModel { get; set; }

    [JsonProperty("view")]
    public bool? View { get; set; }
}
