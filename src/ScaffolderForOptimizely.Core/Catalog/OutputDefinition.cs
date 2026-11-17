namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>The job a generated file does; decides its default folder and namespace.</summary>
public enum OutputRole
{
    Model,
    Controller,
    Component,
    ViewModel,
    View,

    /// <summary>A standalone class such as an initialization module or scheduled job.</summary>
    Class,

    /// <summary>A second class that lives next to the primary file (e.g. a criterion's model).</summary>
    Companion,
}

/// <summary>What kind of content a model represents; decides the model folder and base classes offered.</summary>
public enum ContentKind
{
    None,
    Page,
    Block,
    Media,

    /// <summary>Commerce catalog content: product, variation, bundle, package or category (node).</summary>
    Catalog,
}

/// <summary>Which Razor view path convention a view output follows.</summary>
public enum ViewKind
{
    None,

    /// <summary><c>Views/{Name}/Index.cshtml</c>, rendered by a page controller.</summary>
    PageIndex,

    /// <summary><c>Views/Shared/Components/{Name}/Default.cshtml</c>, rendered by a view component.</summary>
    ComponentDefault,
}

/// <summary>One file an item can produce.</summary>
public sealed class OutputDefinition
{
    public OutputDefinition(string id, OutputRole role, string templateName)
    {
        Id = id;
        Role = role;
        TemplateName = templateName;
    }

    /// <summary>Identifier within the item, e.g. <c>model</c>, <c>controller</c>.</summary>
    public string Id { get; }

    public OutputRole Role { get; }

    /// <summary>Embedded/override Scriban template name, e.g. <c>PageType.cs.sbn</c>.</summary>
    public string TemplateName { get; }

    /// <summary>
    /// For companions: appended to the item name minus its suffix, e.g. criterion "GeoCriterion"
    /// + "CriterionModel" → "GeoCriterionModel".
    /// </summary>
    public string ClassNameSuffix { get; set; } = string.Empty;

    /// <summary>Bool option that switches this output on. Null means always generated.</summary>
    public string? EnabledByOption { get; set; }

    /// <summary>For <see cref="OutputRole.View"/> outputs: the path convention to use.</summary>
    public ViewKind ViewKind { get; set; } = ViewKind.None;

    /// <summary>
    /// For views: which detected or configured pattern applies (page or block views); None uses
    /// the MVC default path.
    /// </summary>
    public ContentKind ViewFor { get; set; } = ContentKind.None;

    /// <summary>
    /// Optional mapping onto an Optimizely <c>dotnet new</c> item template. Used only when the
    /// template source is <c>dotnet-new</c> and the pack is installed.
    /// </summary>
    public DotnetNewMapping? DotnetNew { get; set; }
}

/// <summary>How to produce an output with <c>dotnet new</c> from EPiServer.Templates.</summary>
public sealed class DotnetNewMapping
{
    public DotnetNewMapping(string shortName)
    {
        ShortName = shortName;
    }

    /// <summary>Template short name, e.g. <c>epi-cms-contenttype</c>.</summary>
    public string ShortName { get; }

    /// <summary>
    /// Pass the chosen base class as <c>--basetype</c> (epi-cms-contenttype; the CLI option is lower case).
    /// </summary>
    public bool PassBaseType { get; set; }

    /// <summary>
    /// Pass the rendered content type as <c>--contentType</c> (pagecontroller / contentcomponent).
    /// </summary>
    public bool PassContentType { get; set; }

    /// <summary>Bool options passed straight through as "--name true|false", keyed by option key.</summary>
    public System.Collections.Generic.Dictionary<string, string> BoolArguments { get; set; } = new();

    /// <summary>
    /// Options the Optimizely template can't express. When any is set (a non-empty value other
    /// than "false"), the embedded template is used instead, e.g. a controller that returns a view
    /// model, or a content type with a display name or group.
    /// </summary>
    public string[] UnsupportedWhenOptionsSet { get; set; } = new string[0];

    /// <summary>
    /// The Optimizely template always returns the default view, so it can't be used when an
    /// explicit view path is needed (feature-folder layout).
    /// </summary>
    public bool RendersView { get; set; }
}
