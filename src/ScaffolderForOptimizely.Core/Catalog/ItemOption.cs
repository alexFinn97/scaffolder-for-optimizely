using System.Collections.Generic;

namespace ScaffolderForOptimizely.Core.Catalog;

public enum OptionKind
{
    Text,
    Choice,
    Bool,
}

/// <summary>
/// Where a choice list gets its values from. Static choices are listed on the option itself;
/// the others are filled from what the extension detects in the open project.
/// </summary>
public enum ChoiceSource
{
    Static,
    BaseClasses,
    ContentTypeGroups,
    PageTypes,
    BlockTypes,
    ContentTypes,
}

public sealed class ItemOption
{
    public ItemOption(string key, string label, OptionKind kind)
    {
        Key = key;
        Label = label;
        Kind = kind;
    }

    public string Key { get; }

    public string Label { get; }

    public OptionKind Kind { get; }

    public string Description { get; set; } = string.Empty;

    /// <summary>Default value. For <see cref="OptionKind.Bool"/> use "true"/"false".</summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Name of a config default that overrides <see cref="DefaultValue"/>, e.g. <c>companions.view</c>.
    /// </summary>
    public string? ConfigDefault { get; set; }

    public ChoiceSource ChoiceSource { get; set; } = ChoiceSource.Static;

    public IReadOnlyList<OptionChoice> Choices { get; set; } = new List<OptionChoice>();

    /// <summary>Allows typing a value that isn't in the list (e.g. a base class in another project).</summary>
    public bool AllowCustomValue { get; set; }

    /// <summary>Bool option that must be on for this option to be editable (e.g. view path needs "Razor view").</summary>
    public string? EnabledWhenOption { get; set; }

    /// <summary>
    /// The dialog shows the value the generator worked out (see GenerationResult.ResolvedOptions)
    /// until the user edits it. Until then the option is passed as empty, meaning "work it out".
    /// </summary>
    public bool IsAutoValue { get; set; }
}

public sealed class OptionChoice
{
    public OptionChoice(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; }

    public string Label { get; }

    public override string ToString() => Label;
}
