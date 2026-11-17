using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ScaffolderForOptimizely.Core.Catalog;

namespace ScaffolderForOptimizely.UI;

/// <summary>One option row in the dialog. The XAML picks a template from <see cref="Kind"/>.</summary>
internal sealed class OptionViewModel : INotifyPropertyChanged
{
    private string _value;

    public OptionViewModel(ItemOption option, string value, IReadOnlyList<OptionChoice> choices)
    {
        Option = option;
        _value = value;
        Choices = choices;
        ChoiceValues = choices.Select(c => c.Value).Where(v => v.Length > 0).ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ItemOption Option { get; }

    public string Key => Option.Key;

    public string Label => Option.Label;

    public string Description => Option.Description;

    public bool HasDescription => Option.Description.Length > 0;

    /// <summary>Text, EditableChoice, FixedChoice or Bool; matched by name in the XAML.</summary>
    public string Kind => Option.Kind switch
    {
        OptionKind.Bool => "Bool",
        OptionKind.Choice when Option.AllowCustomValue => "EditableChoice",
        OptionKind.Choice => "FixedChoice",
        _ => "Text",
    };

    public IReadOnlyList<OptionChoice> Choices { get; }

    /// <summary>Plain values for editable combo boxes (the empty "(none)" choice is just an empty box).</summary>
    public IReadOnlyList<string> ChoiceValues { get; }

    public string Value
    {
        get => _value;
        set
        {
            value ??= string.Empty;
            if (_value == value)
            {
                return;
            }

            _value = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoolValue));
        }
    }

    public bool BoolValue
    {
        get => string.Equals(_value, "true", StringComparison.OrdinalIgnoreCase);
        set => Value = value ? "true" : "false";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>A file in the preview list.</summary>
internal sealed class PreviewFile
{
    public PreviewFile(string relativePath, string content, string source, bool exists)
    {
        RelativePath = relativePath;
        Content = content;
        Source = source;
        Exists = exists;
    }

    public string RelativePath { get; }

    public string Content { get; }

    public string Source { get; }

    public bool Exists { get; }

    public string Caption => Exists ? $"{RelativePath} (exists, will be skipped)" : RelativePath;
}
