using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>
/// Works out class names, folders, namespaces and template variables for every enabled output
/// of a request. Pure logic: no file or process access.
/// </summary>
public static class OutputPlanner
{
    private const string ControllerSuffix = "Controller";
    private const string ViewComponentSuffix = "ViewComponent";
    private const string ViewModelSuffix = "ViewModel";

    public static IReadOnlyList<PlannedOutput> Plan(GenerationRequest request)
    {
        var item = request.Item;
        var context = request.Context;
        var config = request.Config.Config;
        var options = request.Options;

        var feature = IsFeatureLayout(config, context);
        var rootNamespace = string.IsNullOrWhiteSpace(config.RootNamespace) ? context.RootNamespace : config.RootNamespace!;
        var name = NormalizeName(request.Name, item);
        var primaryDirectory = PrimaryDirectory(request, item, config);

        string DirectoryFor(OutputRole role) =>
            role == item.PrimaryRole || feature
                ? primaryDirectory
                : Path.Combine(context.ProjectDirectory, ToOsPath(ConventionalFolder(role, item.ContentKind, config)));

        string NamespaceFor(OutputRole role) => NamespaceHelper.ForFolder(rootNamespace, context.ProjectDirectory, DirectoryFor(role));

        // The content model: the new type for content-type items, the chosen type for controllers/components.
        string modelClass, modelNamespace;
        if (item.PrimaryRole == OutputRole.Model)
        {
            modelClass = name;
            modelNamespace = NamespaceFor(OutputRole.Model);
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
        var viewModelClass = modelClass + ViewModelSuffix;
        var viewModelNamespace = NamespaceFor(OutputRole.ViewModel);

        var enabled = item.Outputs.Where(o => o.EnabledByOption is null || IsTrue(options, o.EnabledByOption)).ToList();
        var hasViewModel = enabled.Any(o => o.Role == OutputRole.ViewModel);

        // View paths first: controllers and components need them when the layout is feature folders.
        var viewOutput = enabled.FirstOrDefault(o => o.Role == OutputRole.View);
        string? viewFullPath = null;
        if (viewOutput is not null)
        {
            viewFullPath = ViewPath(viewOutput.ViewKind, feature, primaryDirectory, context.ProjectDirectory, config,
                controllerClass, componentName, modelClass);
        }

        var explicitViewPath = feature && viewFullPath is not null
            ? "~/" + NamespaceHelper.GetRelativePath(context.ProjectDirectory, viewFullPath).Replace('\\', '/')
            : string.Empty;

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

        var planned = new List<PlannedOutput>();
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
                    ns = NamespaceFor(OutputRole.Controller);
                    fullPath = Path.Combine(DirectoryFor(OutputRole.Controller), className + ".cs");
                    break;
                case OutputRole.Component:
                    className = componentClass;
                    ns = NamespaceFor(OutputRole.Component);
                    fullPath = Path.Combine(DirectoryFor(OutputRole.Component), className + ".cs");
                    break;
                case OutputRole.ViewModel:
                    className = viewModelClass;
                    ns = viewModelNamespace;
                    fullPath = Path.Combine(DirectoryFor(OutputRole.ViewModel), className + ".cs");
                    break;
                case OutputRole.View:
                    className = string.Empty;
                    ns = string.Empty;
                    fullPath = viewFullPath!;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown role {output.Role}.");
            }

            var plan = new PlannedOutput(output, fullPath, Relative(context.ProjectDirectory, fullPath), className, ns)
            {
                IsPrimary = output.Role == item.PrimaryRole,
            };

            foreach (var pair in common)
            {
                plan.Model[pair.Key] = pair.Value;
            }

            plan.Model["class_name"] = className;
            plan.Model["namespace"] = ns;

            switch (output.Role)
            {
                case OutputRole.Model:
                    plan.RequiredUsings.Add(NamespaceHelper.NamespaceOf(baseClass));
                    plan.RequiredUsings.Add(groupNamespace);
                    break;
                case OutputRole.Controller:
                case OutputRole.Component:
                    plan.RequiredUsings.Add(modelNamespace);
                    if (hasViewModel)
                    {
                        plan.RequiredUsings.Add(viewModelNamespace);
                    }

                    break;
                case OutputRole.ViewModel:
                    plan.RequiredUsings.Add(modelNamespace);
                    break;
            }

            // Types in the file's own namespace or a parent namespace are already in scope.
            plan.RequiredUsings.RemoveAll(u => u.Length == 0 || u == ns || ns.StartsWith(u + ".", StringComparison.Ordinal));
            planned.Add(plan);
        }

        return planned;
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
        return Path.Combine(request.Context.ProjectDirectory, ToOsPath(ConventionalFolder(item.PrimaryRole, item.ContentKind, config)));
    }

    private static string ConventionalFolder(OutputRole role, ContentKind kind, ScaffolderConfig config) => role switch
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
        OutputRole.ViewModel => config.Folders.ViewModels,
        OutputRole.View => config.Folders.Views,
        _ => string.Empty,
    };

    private static string ViewPath(ViewKind kind, bool feature, string primaryDirectory, string projectDirectory,
        ScaffolderConfig config, string controllerClass, string componentName, string modelClass)
    {
        if (feature)
        {
            var fileName = kind == ViewKind.PageIndex ? modelClass : componentName;
            return Path.Combine(primaryDirectory, fileName + ".cshtml");
        }

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
