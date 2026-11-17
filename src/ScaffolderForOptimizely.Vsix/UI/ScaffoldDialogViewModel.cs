using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.UI;

/// <summary>State of the scaffold dialog: name, options and a live preview of the files.</summary>
internal sealed class ScaffoldDialogViewModel : INotifyPropertyChanged
{
    private readonly ProjectContext _context;
    private readonly LoadedConfig _config;
    private readonly string _targetDirectory;
    private readonly Dictionary<string, string> _hiddenOptions;
    private readonly DispatcherTimer _previewTimer;
    private CancellationTokenSource? _previewCts;

    private string _name = string.Empty;
    private bool _nameEdited;
    private PreviewFile? _selectedFile;
    private string _message = string.Empty;
    private bool _applyingAutoValues;

    public ScaffoldDialogViewModel(ItemDefinition item, ProjectContext context, LoadedConfig config, string targetDirectory)
    {
        Item = item;
        _context = context;
        _config = config;
        _targetDirectory = targetDirectory;

        var defaults = OptionDefaults.For(item, context, config.Config);
        _hiddenOptions = defaults.Where(d => item.Options.All(o => o.Key != d.Key)).ToDictionary(d => d.Key, d => d.Value);

        foreach (var option in item.Options)
        {
            var vm = new OptionViewModel(option, defaults[option.Key], OptionDefaults.ChoicesFor(option, item, context));
            vm.PropertyChanged += OnOptionChanged;
            Options.Add(vm);
        }

        // Start from a placeholder, not the default content type's name, which usually already
        // has a controller/component. Picking a content type still renames it until the user types.
        _name = item.DefaultName;
        UpdateEnabledStates();

        _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            RefreshPreviewAsync().FireAndForget();
        };
        SchedulePreview();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ItemDefinition Item { get; }

    public string Title => $"Add Optimizely {Item.DisplayName}";

    public string Description => Item.Description;

    public string NameHint => Item.NameExample.Length > 0 ? $"e.g. {Item.NameExample}" : string.Empty;

    public string TargetFolder
    {
        get
        {
            var relative = NamespaceHelper.GetRelativePath(_context.ProjectDirectory, _targetDirectory);
            return relative.Length == 0 ? "(project root: conventional folders are used)" : relative.Replace('\\', '/');
        }
    }

    public string TemplateSourceNote =>
        string.Equals(_config.Config.TemplateSource, TemplateSources.DotnetNew, StringComparison.OrdinalIgnoreCase)
            ? "Preview uses the built-in templates. Optimizely's dotnet new templates are applied, where one exists, when you click Add."
            : string.Empty;

    public string ConfigNote => _config.FilePath is null
        ? "No .optimizely-scaffolder.json found: using detected defaults."
        : $"Settings from {_config.FilePath}" + (_config.Error is null ? string.Empty : $" (couldn't read it: {_config.Error})");

    public ObservableCollection<OptionViewModel> Options { get; } = new();

    public ObservableCollection<PreviewFile> PreviewFiles { get; } = new();

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value ?? string.Empty;
            _nameEdited = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanAdd));
            SchedulePreview();
        }
    }

    /// <summary>False until there's a name and, where needed, a content type.</summary>
    public bool CanAdd => OutputPlanner.Validate(CreateRequest(previewOnly: true)) is null;

    public PreviewFile? SelectedFile
    {
        get => _selectedFile;
        set
        {
            _selectedFile = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Warnings and conflicts shown under the preview.</summary>
    public string Message
    {
        get => _message;
        private set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public GenerationRequest CreateRequest(bool previewOnly) =>
        new(Item, _name, _targetDirectory, CurrentOptions(), _context, _config) { PreviewOnly = previewOnly };

    private Dictionary<string, string> CurrentOptions()
    {
        var options = new Dictionary<string, string>(_hiddenOptions, StringComparer.Ordinal);
        foreach (var option in Options)
        {
            // An auto-filled value the user hasn't touched means "use the default".
            options[option.Key] = option.IsAuto && !option.IsUserEdited ? string.Empty : option.Value;
        }

        return options;
    }

    private void OnOptionChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_applyingAutoValues || e.PropertyName is not (nameof(OptionViewModel.Value)))
        {
            return;
        }

        if (sender is OptionViewModel option && option.Key == Item.NameFromOption && !_nameEdited)
        {
            var suggested = OutputPlanner.SuggestName(Item, CurrentOptions());
            if (suggested.Length > 0)
            {
                _name = suggested;
                OnPropertyChanged(nameof(Name));
            }
        }

        OnPropertyChanged(nameof(CanAdd));
        UpdateEnabledStates();
        SchedulePreview();
    }

    /// <summary>Greys out options whose switch is off (e.g. view path when "Razor view" is unticked).</summary>
    private void UpdateEnabledStates()
    {
        foreach (var option in Options.Where(o => o.Option.EnabledWhenOption is not null))
        {
            option.IsEnabled = Options.FirstOrDefault(o => o.Key == option.Option.EnabledWhenOption)?.BoolValue ?? true;
        }
    }

    /// <summary>Shows what the generator decided (paths, pass-view-path) in auto options the user hasn't edited.</summary>
    private void ApplyAutoValues(GenerationResult result)
    {
        _applyingAutoValues = true;
        try
        {
            foreach (var option in Options.Where(o => o.IsAuto))
            {
                if (result.ResolvedOptions.TryGetValue(option.Key, out var value))
                {
                    option.SetAutoValue(value);
                }
            }
        }
        finally
        {
            _applyingAutoValues = false;
        }
    }

    private void SchedulePreview()
    {
        _previewTimer.Stop();
        _previewTimer.Start();
    }

    private async System.Threading.Tasks.Task RefreshPreviewAsync()
    {
        _previewCts?.Cancel();
        _previewCts = new CancellationTokenSource();
        var token = _previewCts.Token;

        var problem = OutputPlanner.Validate(CreateRequest(previewOnly: true));
        if (problem is not null)
        {
            PreviewFiles.Clear();
            Message = problem + " The files that will be created are previewed here.";
            return;
        }

        GenerationResult result;
        try
        {
            result = await new ScaffoldGenerator().GenerateAsync(CreateRequest(previewOnly: true), token);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            PreviewFiles.Clear();
            Message = ex.Message;
            return;
        }

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var selectedPath = _selectedFile?.RelativePath;
        PreviewFiles.Clear();
        foreach (var file in result.Files)
        {
            PreviewFiles.Add(new PreviewFile(file.RelativePath, file.Content, file.Source, File.Exists(file.FullPath)));
        }

        SelectedFile = PreviewFiles.FirstOrDefault(f => f.RelativePath == selectedPath) ?? PreviewFiles.FirstOrDefault();
        ApplyAutoValues(result);

        var notes = result.Warnings.ToList();
        var existing = PreviewFiles.Where(f => f.Exists).Select(f => f.RelativePath).ToList();
        if (existing.Count > 0)
        {
            notes.Insert(0, "Already exists and will be skipped: " + string.Join(", ", existing));
        }

        Message = string.Join(Environment.NewLine, notes);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
