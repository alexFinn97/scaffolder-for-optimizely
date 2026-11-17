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

    [JsonProperty("viewModels")]
    public string ViewModels { get; set; } = "Models/ViewModels";

    [JsonProperty("views")]
    public string Views { get; set; } = "Views";
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
