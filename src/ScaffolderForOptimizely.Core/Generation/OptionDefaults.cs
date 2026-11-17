using System;
using System.Collections.Generic;
using System.Linq;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>Default option values and choice lists, from config first, then detection, then the catalogue.</summary>
public static class OptionDefaults
{
    public static Dictionary<string, string> For(ItemDefinition item, ProjectContext context, ScaffolderConfig config)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var option in item.Options)
        {
            values[option.Key] = DefaultFor(option, item, context, config);
        }

        ApplyBaseClassConfig(values, item, config);
        values[OptionKeys.Guid] = Guid.NewGuid().ToString("D").ToUpperInvariant();
        return values;
    }

    public static IReadOnlyList<OptionChoice> ChoicesFor(ItemOption option, ItemDefinition item, ProjectContext context)
    {
        IEnumerable<string> values;
        switch (option.ChoiceSource)
        {
            case ChoiceSource.Static:
                return option.Choices;
            case ChoiceSource.BaseClasses:
                values = BaseClassChoices(option, item.ContentKind, context);
                break;
            case ChoiceSource.ContentTypeGroups:
                return new[] { new OptionChoice(string.Empty, "(none)") }
                    .Concat(context.GroupNames.Select(g => new OptionChoice(g.Expression, g.Expression)))
                    .ToList();
            case ChoiceSource.PageTypes:
                values = context.PageTypes.Concat(new[] { "EPiServer.Core.PageData" });
                break;
            case ChoiceSource.BlockTypes:
                values = context.BlockTypes.Concat(new[] { "EPiServer.Core.BlockData" });
                break;
            case ChoiceSource.ContentTypes:
                values = context.PageTypes.Concat(context.BlockTypes).Concat(context.CatalogTypes)
                    .Concat(new[] { "EPiServer.Core.PageData", "EPiServer.Core.BlockData" });
                break;
            case ChoiceSource.CatalogTypes:
                values = context.CatalogTypes.Concat(CatalogRoots.All);
                break;
            default:
                return option.Choices;
        }

        return values.Distinct(StringComparer.Ordinal).Select(v => new OptionChoice(v, v)).ToList();
    }

    private static IEnumerable<string> BaseClassChoices(ItemOption option, ContentKind kind, ProjectContext context) => kind switch
    {
        ContentKind.Page => context.PageBaseClasses.Concat(new[] { "EPiServer.Core.PageData" }),
        ContentKind.Block => context.BlockBaseClasses.Concat(new[] { "EPiServer.Core.BlockData" }),
        ContentKind.Media => context.MediaBaseClasses.Concat(new[] { "EPiServer.Core.ImageData", "EPiServer.Core.VideoData", "EPiServer.Core.MediaData" }),
        // Each catalog item's default is its root type (ProductContent...); offer the project's bases for that root.
        ContentKind.Catalog => CatalogBaseClasses(option.DefaultValue, context).Concat(new[] { option.DefaultValue }),
        _ => Enumerable.Empty<string>(),
    };

    private static List<string> CatalogBaseClasses(string root, ProjectContext context) =>
        context.CatalogBaseClasses.TryGetValue(root, out var bases) ? bases : new List<string>();

    private static string DefaultFor(ItemOption option, ItemDefinition item, ProjectContext context, ScaffolderConfig config)
    {
        var fromConfig = ConfigValue(option, config);
        if (!string.IsNullOrEmpty(fromConfig))
        {
            return fromConfig!;
        }

        switch (option.ChoiceSource)
        {
            case ChoiceSource.BaseClasses:
                return PreferSite(item.ContentKind switch
                {
                    ContentKind.Page => context.PageBaseClasses,
                    ContentKind.Block => context.BlockBaseClasses,
                    ContentKind.Catalog => CatalogBaseClasses(option.DefaultValue, context),
                    _ => new List<string>(),
                }) ?? option.DefaultValue;
            default:
                // Content type pickers start blank: preselecting an existing type usually means a
                // controller/component for a type that already has one.
                return option.DefaultValue;
        }
    }

    /// <summary>Prefers a base class named like <c>SitePageData</c>, the usual convention.</summary>
    private static string? PreferSite(List<string> candidates) =>
        candidates.FirstOrDefault(c => NamespaceHelper.ShortName(c).StartsWith("Site", StringComparison.Ordinal))
        ?? candidates.FirstOrDefault();

    private static string? ConfigValue(ItemOption option, ScaffolderConfig config)
    {
        if (option.Key == OptionKeys.BaseClass)
        {
            // Depends on the content kind; applied afterwards by ApplyBaseClassConfig.
            return null;
        }

        if (option.Key == OptionKeys.Group)
        {
            return config.ContentTypeGroup;
        }

        return option.ConfigDefault switch
        {
            "blockRenderStyle" => config.BlockRenderStyle,
            "companions.controller" => BoolText(config.Companions.Controller),
            "companions.component" => BoolText(config.Companions.Component),
            "companions.viewModel" => BoolText(config.Companions.ViewModel),
            "companions.view" => BoolText(config.Companions.View),
            _ => null,
        };
    }

    /// <summary>Applies <c>baseClasses</c> from config, which depends on the item's content kind.</summary>
    private static void ApplyBaseClassConfig(Dictionary<string, string> values, ItemDefinition item, ScaffolderConfig config)
    {
        if (!values.ContainsKey(OptionKeys.BaseClass))
        {
            return;
        }

        var configured = item.ContentKind switch
        {
            ContentKind.Page => config.BaseClasses.Page,
            ContentKind.Block => config.BaseClasses.Block,
            ContentKind.Media => config.BaseClasses.Media,
            ContentKind.Catalog => config.BaseClasses.ForCatalogRoot(
                item.Options.First(o => o.Key == OptionKeys.BaseClass).DefaultValue),
            _ => null,
        };

        if (!string.IsNullOrWhiteSpace(configured))
        {
            values[OptionKeys.BaseClass] = configured!;
        }
    }

    private static string? BoolText(bool? value) => value.HasValue ? (value.Value ? "true" : "false") : null;
}
