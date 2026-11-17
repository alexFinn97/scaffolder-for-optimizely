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

    /// <summary>Set by the UI. For auto-filled options, any edit means "use what I typed".</summary>
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
            IsUserEdited = value.Trim().Length > 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BoolValue));
        }
    }

    /// <summary>
    /// For options with <see cref="ItemOption.IsAutoValue"/>: false while the value is the
    /// one the dialog filled in. Clearing the box goes back to auto.
    /// </summary>
    public bool IsUserEdited { get; private set; }

    public bool IsAuto => Option.IsAutoValue;

    /// <summary>Shows a computed value without marking it as edited. Returns true if it changed.</summary>
    public bool SetAutoValue(string value)
    {
        if (IsUserEdited || _value == value)
        {
            return false;
        }

        _value = value;
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(BoolValue));
        return true;
    }

    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
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
