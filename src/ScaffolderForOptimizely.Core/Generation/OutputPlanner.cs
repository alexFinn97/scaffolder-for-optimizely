using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>The files to generate plus what the planner decided along the way.</summary>
public sealed class OutputPlan
{
    public List<PlannedOutput> Outputs { get; } = new();

    /// <summary>Notes about options that were ignored (e.g. a path outside the project).</summary>
    public List<string> Warnings { get; } = new();

    /// <summary>Values chosen for auto options (view path, view model path, pass view path), for the dialog to show.</summary>
    public Dictionary<string, string> ResolvedOptions { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Works out class names, folders, namespaces and template variables for every enabled output
/// of a request. Pure logic: no file or process access.
/// </summary>
public static class OutputPlanner
{
    private const string ControllerSuffix = "Controller";
    private const string ViewComponentSuffix = "ViewComponent";
    private const string ViewModelSuffix = "ViewModel";

    /// <summary>Why the request can't be generated yet, or null when it can.</summary>
    public static string? Validate(GenerationRequest request)
    {
        if (NormalizeName(request.Name, request.Item).Length == 0)
        {
            return "Enter a name.";
        }

        if (request.Item.Options.Any(o => o.Key == OptionKeys.ContentType) && Get(request.Options, OptionKeys.ContentType).Length == 0)
        {
            return "Choose a content type.";
        }

        return null;
    }

    public static OutputPlan Plan(GenerationRequest request)
    {
        var plan = new OutputPlan();
        var item = request.Item;
        var context = request.Context;
        var config = request.Config.Config;
        var options = request.Options;
        var projectDirectory = context.ProjectDirectory;

        var feature = IsFeatureLayout(config, context);
        var rootNamespace = string.IsNullOrWhiteSpace(config.RootNamespace) ? context.RootNamespace : config.RootNamespace!;
        var name = NormalizeName(request.Name, item);
        var primaryDirectory = PrimaryDirectory(request, item, config);

        string DirectoryFor(OutputRole role) =>
            role == item.PrimaryRole || feature
                ? primaryDirectory
                : Path.Combine(projectDirectory, ToOsPath(ConventionalFolder(role, item.ContentKind, config, context)));

        string NamespaceForDirectory(string directory) => NamespaceHelper.ForFolder(rootNamespace, projectDirectory, directory);

        // The content model: the new type for content-type items, the chosen type for controllers/components.
        string modelClass, modelNamespace;
        if (item.PrimaryRole == OutputRole.Model)
        {
            modelClass = name;
            modelNamespace = NamespaceForDirectory(DirectoryFor(OutputRole.Model));
        }
        else
        {
            var contentType = Get(options, OptionKeys.ContentType);
            modelClass = contentType.Length == 0 ? string.Empty : NamespaceHelper.ShortName(contentType);
            modelNamespace = NamespaceHelper.NamespaceOf(contentType);
        }

        var controllerClass = item.PrimaryRole == OutputRole.Controller ? name : modelClass + ControllerSuffix;
        var componentClass = item.PrimaryRole == OutputRole.Component ? name : modelClass + ViewComponentSuffix;
        var componentName = componentClass.EndsWith(ViewComponentSuffix, StringComparison.Ordinal)
            ? componentClass.Substring(0, componentClass.Length - ViewComponentSuffix.Length)
            : componentClass;

        var enabled = item.Outputs.Where(o => o.EnabledByOption is null || IsTrue(options, o.EnabledByOption)).ToList();
        var hasViewModel = enabled.Any(o => o.Role == OutputRole.ViewModel);

        // View model: default folder and class, or a typed file path whose file name is the class.
        var viewModelPath = Path.Combine(DirectoryFor(OutputRole.ViewModel), modelClass + ViewModelSuffix + ".cs");
        var typedViewModelPath = Get(options, OptionKeys.ViewModelPath);
        if (typedViewModelPath.Length > 0)
        {
            var resolved = ResolvePath(projectDirectory, typedViewModelPath, ".cs");
            if (resolved is null)
            {
                plan.Warnings.Add($"View model path '{typedViewModelPath}' is outside the project, so the default path was used.");
            }
            else
            {
                viewModelPath = resolved;
            }
        }

        var viewModelClass = NamespaceHelper.Sanitize(Path.GetFileNameWithoutExtension(viewModelPath)).Replace(".", "_");
        var viewModelNamespace = NamespaceForDirectory(Path.GetDirectoryName(viewModelPath)!);
        if (item.Options.Any(o => o.Key == OptionKeys.ViewModelPath))
        {
            plan.ResolvedOptions[OptionKeys.ViewModelPath] = Relative(projectDirectory, viewModelPath);
        }

        // View: typed path, else configured/detected pattern, else MVC's default location.
        var viewOutput = enabled.FirstOrDefault(o => o.Role == OutputRole.View);
        string? viewFullPath = null;
        var explicitViewPath = string.Empty;
        if (viewOutput is not null)
        {
            var mvcDefault = MvcDefaultViewPath(viewOutput.ViewKind, projectDirectory, config, controllerClass, componentName);
            viewFullPath = DefaultViewPath(viewOutput, feature, primaryDirectory, projectDirectory, config, context, modelClass, componentName)
                ?? mvcDefault;

            var typedViewPath = Get(options, OptionKeys.ViewPath);
            if (typedViewPath.Length > 0)
            {
                var resolved = ResolvePath(projectDirectory, typedViewPath, ".cshtml");
                if (resolved is null)
                {
                    plan.Warnings.Add($"View path '{typedViewPath}' is outside the project, so the default path was used.");
                }
                else
                {
                    viewFullPath = resolved;
                }
            }

            var isMvcDefault = string.Equals(Path.GetFullPath(viewFullPath), Path.GetFullPath(mvcDefault), StringComparison.OrdinalIgnoreCase);
            var passViewPath = Get(options, OptionKeys.PassViewPath) switch
            {
                var v when v.Equals("true", StringComparison.OrdinalIgnoreCase) => true,
                var v when v.Equals("false", StringComparison.OrdinalIgnoreCase) => false,
                _ => config.PassViewPath ?? (!isMvcDefault && !context.HasViewLocationExpander),
            };

            if (passViewPath)
            {
                explicitViewPath = "~/" + Relative(projectDirectory, viewFullPath);
            }

            plan.ResolvedOptions[OptionKeys.ViewPath] = Relative(projectDirectory, viewFullPath);
            plan.ResolvedOptions[OptionKeys.PassViewPath] = passViewPath ? "true" : "false";
        }

        var baseClass = Get(options, OptionKeys.BaseClass);
        var group = Get(options, OptionKeys.Group);
        var groupNamespace = context.GroupNames.FirstOrDefault(g => g.Expression == group)?.Namespace ?? string.Empty;
        var displayName = Get(options, OptionKeys.DisplayName);
        var blockStyle = Get(options, OptionKeys.BlockStyle);

        var common = new Dictionary<string, object>
        {
            ["name"] = name,
            ["root_namespace"] = rootNamespace,
            ["model_class"] = modelClass,
            ["model_namespace"] = modelNamespace,
            ["model_full_name"] = Join(modelNamespace, modelClass),
            ["controller_class"] = controllerClass,
            ["component_class"] = componentClass,
            ["component_name"] = componentName,
            ["view_model_class"] = viewModelClass,
            ["view_model_namespace"] = viewModelNamespace,
            ["view_model_full_name"] = Join(viewModelNamespace, viewModelClass),
            ["has_view_model"] = hasViewModel,
            ["view_model_type"] = hasViewModel ? Join(viewModelNamespace, viewModelClass) : Join(modelNamespace, modelClass),
            ["view_path"] = explicitViewPath,
            ["base_class"] = baseClass.Length == 0 ? string.Empty : NamespaceHelper.ShortName(baseClass),
            ["base_class_namespace"] = NamespaceHelper.NamespaceOf(baseClass),
            ["display_name"] = EscapeString(displayName.Length == 0 ? Humanize(name) : displayName),
            ["description"] = EscapeString(Get(options, OptionKeys.Description)),
            ["guid"] = Get(options, OptionKeys.Guid).Length == 0 ? Guid.NewGuid().ToString("D").ToUpperInvariant() : Get(options, OptionKeys.Guid),
            ["group"] = group,
            ["extensions"] = EscapeString(Get(options, OptionKeys.Extensions).Replace(" ", string.Empty).Replace(".", string.Empty)),
            ["block_style"] = blockStyle.Length == 0 ? BlockStyles.Async : blockStyle,
            ["async"] = IsTrue(options, OptionKeys.Async),
            ["nullable"] = context.NullableEnabled,
            ["q"] = context.NullableEnabled ? "?" : string.Empty,
        };

        foreach (var output in enabled)
        {
            string className, fullPath, ns;
            switch (output.Role)
            {
                case OutputRole.Model:
                    className = modelClass;
                    ns = modelNamespace;
                    fullPath = Path.Combine(DirectoryFor(OutputRole.Model), className + ".cs");
                    break;
                case OutputRole.Controller:
                    className = controllerClass;
                    fullPath = Path.Combine(DirectoryFor(OutputRole.Controller), className + ".cs");
                    ns = NamespaceForDirectory(DirectoryFor(OutputRole.Controller));
                    break;
                case OutputRole.Component:
                    className = componentClass;
                    fullPath = Path.Combine(DirectoryFor(OutputRole.Component), className + ".cs");
                    ns = NamespaceForDirectory(DirectoryFor(OutputRole.Component));
                    break;
                case OutputRole.ViewModel:
                    className = viewModelClass;
                    ns = viewModelNamespace;
                    fullPath = viewModelPath;
                    break;
                case OutputRole.View:
                    className = string.Empty;
                    ns = string.Empty;
                    fullPath = viewFullPath!;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown role {output.Role}.");
            }

            var planned = new PlannedOutput(output, fullPath, Relative(projectDirectory, fullPath), className, ns)
            {
                IsPrimary = output.Role == item.PrimaryRole,
            };

            foreach (var pair in common)
            {
                planned.Model[pair.Key] = pair.Value;
            }

            planned.Model["class_name"] = className;
            planned.Model["namespace"] = ns;

            switch (output.Role)
            {
                case OutputRole.Model:
                    planned.RequiredUsings.Add(NamespaceHelper.NamespaceOf(baseClass));
                    planned.RequiredUsings.Add(groupNamespace);
                    break;
                case OutputRole.Controller:
                case OutputRole.Component:
                    planned.RequiredUsings.Add(modelNamespace);
                    if (hasViewModel)
                    {
                        planned.RequiredUsings.Add(viewModelNamespace);
                    }

                    break;
                case OutputRole.ViewModel:
                    planned.RequiredUsings.Add(modelNamespace);
                    break;
            }

            // Types in the file's own namespace or a parent namespace are already in scope.
            planned.RequiredUsings.RemoveAll(u => u.Length == 0 || u == ns || ns.StartsWith(u + ".", StringComparison.Ordinal));
            plan.Outputs.Add(planned);
        }

        return plan;
    }

    public static bool IsFeatureLayout(ScaffolderConfig config, ProjectContext context) =>
        config.Layout is null
            ? context.UsesFeatureFolders
            : string.Equals(config.Layout, Layouts.Feature, StringComparison.OrdinalIgnoreCase);

    /// <summary>Makes the typed name a valid class name and adds the item's suffix if it's missing.</summary>
    public static string NormalizeName(string typed, ItemDefinition item)
    {
        var sb = new StringBuilder();
        foreach (var ch in typed.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sb.Append(ch);
            }
        }

        var name = sb.ToString();
        if (name.Length == 0)
        {
            return name;
        }

        if (char.IsDigit(name[0]))
        {
            name = "_" + name;
        }

        name = char.ToUpperInvariant(name[0]) + name.Substring(1);

        var suffix = item.NameSuffix;
        if (suffix.Length > 0
            && !name.EndsWith(suffix, StringComparison.Ordinal)
            && !(suffix == ViewComponentSuffix && name.EndsWith("Component", StringComparison.Ordinal)))
        {
            name += suffix;
        }

        return name;
    }

    /// <summary>Suggested class name for items whose name follows an option (e.g. ArticlePage → ArticlePageController).</summary>
    public static string SuggestName(ItemDefinition item, IDictionary<string, string> options)
    {
        if (item.NameFromOption is null)
        {
            return string.Empty;
        }

        var value = Get(options, item.NameFromOption);
        var shortName = NamespaceHelper.ShortName(value);
        if (shortName is "PageData" or "BlockData" or "")
        {
            return string.Empty;
        }

        return NormalizeName(shortName, item);
    }

    private static string PrimaryDirectory(GenerationRequest request, ItemDefinition item, ScaffolderConfig config)
    {
        var target = Path.GetFullPath(request.TargetDirectory);
        if (NamespaceHelper.GetRelativePath(request.Context.ProjectDirectory, target).Length > 0)
        {
            return target;
        }

        // Right-clicked the project itself: use the conventional folder for the primary file.
        return Path.Combine(request.Context.ProjectDirectory, ToOsPath(ConventionalFolder(item.PrimaryRole, item.ContentKind, config, request.Context)));
    }

    private static string ConventionalFolder(OutputRole role, ContentKind kind, ScaffolderConfig config, ProjectContext context) => role switch
    {
        OutputRole.Model => kind switch
        {
            ContentKind.Page => config.Folders.Pages,
            ContentKind.Block => config.Folders.Blocks,
            ContentKind.Media => config.Folders.Media,
            _ => "Models",
        },
        OutputRole.Controller => config.Folders.Controllers,
        OutputRole.Component => config.Folders.Components,
        // Config wins, then where existing view models live, then the default.
        OutputRole.ViewModel => config.Folders.ViewModels ?? context.ViewModelsFolder ?? "Models/ViewModels",
        OutputRole.View => config.Folders.Views,
        _ => string.Empty,
    };

    /// <summary>
    /// Configured pattern, then feature folder, then the pattern learnt from existing views.
    /// Null means use MVC's default location.
    /// </summary>
    private static string? DefaultViewPath(OutputDefinition view, bool feature, string primaryDirectory, string projectDirectory,
        ScaffolderConfig config, ProjectContext context, string modelClass, string componentName)
    {
        var configured = view.ViewFor switch
        {
            ContentKind.Page => config.ViewPaths.Page,
            ContentKind.Block => config.ViewPaths.Block,
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(configured) && modelClass.Length > 0)
        {
            return ResolvePath(projectDirectory, configured!.Replace(ViewConventionDetector.NamePlaceholder, modelClass), ".cshtml");
        }

        if (feature)
        {
            var fileName = view.ViewKind == ViewKind.PageIndex ? modelClass : componentName;
            return Path.Combine(primaryDirectory, fileName + ".cshtml");
        }

        var detected = view.ViewFor switch
        {
            ContentKind.Page => context.PageViewPattern,
            ContentKind.Block => context.BlockViewPattern,
            _ => null,
        };

        return detected is not null && modelClass.Length > 0
            ? ResolvePath(projectDirectory, detected.Replace(ViewConventionDetector.NamePlaceholder, modelClass), ".cshtml")
            : null;
    }

    /// <summary>Where MVC looks without help: Views/{Controller}/Index.cshtml or Views/Shared/Components/{Name}/Default.cshtml.</summary>
    private static string MvcDefaultViewPath(ViewKind kind, string projectDirectory, ScaffolderConfig config, string controllerClass, string componentName)
    {
        var views = Path.Combine(projectDirectory, ToOsPath(config.Folders.Views));
        if (kind == ViewKind.PageIndex)
        {
            var controllerName = controllerClass.EndsWith(ControllerSuffix, StringComparison.Ordinal)
                ? controllerClass.Substring(0, controllerClass.Length - ControllerSuffix.Length)
                : controllerClass;
            return Path.Combine(views, controllerName, "Index.cshtml");
        }

        return Path.Combine(views, "Shared", "Components", componentName, "Default.cshtml");
    }

    /// <summary>Typed view path to a full path; see <see cref="ResolvePath"/>.</summary>
    public static string? ResolveViewPath(string projectDirectory, string typed) => ResolvePath(projectDirectory, typed, ".cshtml");

    /// <summary>
    /// Turns a typed path ("Views/Foo/Bar", "~/Views/Foo/Bar.cshtml", "Views\Foo\Bar.cshtml") into a
    /// full path, adding <paramref name="extension"/> if missing. Returns null for paths outside the project.
    /// </summary>
    public static string? ResolvePath(string projectDirectory, string typed, string extension)
    {
        var path = typed.Trim().Replace('\\', '/');
        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            path = path.Substring(2);
        }

        path = path.TrimStart('/');
        if (path.Length == 0 || path.EndsWith("/", StringComparison.Ordinal))
        {
            return null;
        }

        if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            path += extension;
        }

        string full;
        try
        {
            full = Path.GetFullPath(Path.Combine(projectDirectory, ToOsPath(path)));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }

        return NamespaceHelper.GetRelativePath(projectDirectory, full).Length == 0 ? null : full;
    }

    private static string Relative(string projectDirectory, string fullPath)
    {
        var relative = NamespaceHelper.GetRelativePath(projectDirectory, fullPath);
        return (relative.Length == 0 ? Path.GetFileName(fullPath) : relative).Replace('\\', '/');
    }

    private static string ToOsPath(string configPath) =>
        configPath.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);

    private static string Join(string ns, string className) =>
        className.Length == 0 ? string.Empty : ns.Length == 0 ? className : ns + "." + className;

    private static string Get(IDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && value is not null ? value.Trim() : string.Empty;

    private static bool IsTrue(IDictionary<string, string> options, string key) =>
        string.Equals(Get(options, key), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>"ArticlePage" → "Article Page".</summary>
    public static string Humanize(string name)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (i > 0 && char.IsUpper(ch) && (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]) && char.IsUpper(name[i - 1]))))
            {
                sb.Append(' ');
            }

            sb.Append(ch);
        }

        return sb.ToString().Trim('_', ' ');
    }

    private static string EscapeString(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
