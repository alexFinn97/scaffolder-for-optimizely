using System.Collections.Generic;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>
/// Describes one thing the extension can scaffold (e.g. a page type). Commands, the dialog
/// and the tests are all driven from these definitions.
/// </summary>
public sealed class ItemDefinition
{
    public ItemDefinition(
        string id,
        string category,
        string displayName,
        string description,
        OutputRole primaryRole,
        IReadOnlyList<ItemOption> options,
        IReadOnlyList<OutputDefinition> outputs)
    {
        Id = id;
        Category = category;
        DisplayName = displayName;
        Description = description;
        PrimaryRole = primaryRole;
        Options = options;
        Outputs = outputs;
    }

    /// <summary>Stable identifier, e.g. <c>cms-page-type</c>.</summary>
    public string Id { get; }

    /// <summary>Menu grouping, e.g. <c>CMS/Content types</c>.</summary>
    public string Category { get; }

    public string DisplayName { get; }

    public string Description { get; }

    /// <summary>Role of the file the user is really asking for; it lands in the folder they right-clicked.</summary>
    public OutputRole PrimaryRole { get; }

    /// <summary>What kind of content the model is (decides the default folder and base class).</summary>
    public ContentKind ContentKind { get; set; } = ContentKind.None;

    /// <summary>Suffix added to the entered name when it is missing (e.g. <c>Controller</c>).</summary>
    public string NameSuffix { get; set; } = string.Empty;

    /// <summary>Example shown under the name box.</summary>
    public string NameExample { get; set; } = string.Empty;

    /// <summary>
    /// Name the dialog starts with, e.g. <c>NewPageController</c>. A neutral placeholder rather than
    /// an existing type's name, which would usually clash with a class the project already has.
    /// </summary>
    public string DefaultName { get; set; } = string.Empty;

    /// <summary>
    /// When set, the class name is suggested from this option's value (e.g. a controller named after
    /// the content type it renders).
    /// </summary>
    public string? NameFromOption { get; set; }

    /// <summary>
    /// For infrastructure/editor items: the folder setting (see FolderConfig) and detection key used
    /// when scaffolding from the project node, e.g. <c>scheduledJobs</c>.
    /// </summary>
    public string? FolderKey { get; set; }

    /// <summary>Alloy-style fallback when nothing is configured or detected, e.g. <c>Business/ScheduledJobs</c>.</summary>
    public string DefaultFolder { get; set; } = string.Empty;

    public IReadOnlyList<ItemOption> Options { get; }

    public IReadOnlyList<OutputDefinition> Outputs { get; }
}
