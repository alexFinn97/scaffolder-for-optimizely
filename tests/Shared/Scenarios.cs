using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Tests.Shared;

/// <summary>
/// One scaffolding run used by both the snapshot tests and the compile tests, so every
/// snapshotted output is also proven to compile.
/// </summary>
public sealed class Scenario
{
    public Scenario(string name, string itemId, string typedName)
    {
        Name = name;
        ItemId = itemId;
        TypedName = typedName;
    }

    public string Name { get; }

    public string ItemId { get; }

    public string TypedName { get; }

    /// <summary>Right-clicked folder relative to the project ("" = the project itself).</summary>
    public string TargetFolder { get; set; } = string.Empty;

    public Dictionary<string, string> Options { get; set; } = new();

    public bool FileScopedNamespaces { get; set; } = true;

    public bool Nullable { get; set; } = true;

    public string? Layout { get; set; }

    public override string ToString() => Name;
}

public static class Scenarios
{
    public const string RootNamespace = "Contoso.Web";
    public const string FixedGuid = "11111111-2222-3333-4444-555555555555";

    public static readonly string ProjectDirectory =
        Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "Contoso", "Contoso.Web");

    public static IReadOnlyList<Scenario> All { get; } = new List<Scenario>
    {
        new("page-type-defaults", ItemCatalog.Ids.PageType, "ArticlePage"),
        new("page-type-view-model", ItemCatalog.Ids.PageType, "ArticlePage")
        {
            Options = { [OptionKeys.ViewModel] = "true" },
        },
        new("page-type-model-only-in-folder", ItemCatalog.Ids.PageType, "NewsPage")
        {
            TargetFolder = "Models/Pages/News",
            Options = { [OptionKeys.Controller] = "false", [OptionKeys.View] = "false", [OptionKeys.DisplayName] = "News \"page\"" },
        },
        new("page-type-site-base-and-group", ItemCatalog.Ids.PageType, "ProductPage")
        {
            Options =
            {
                [OptionKeys.BaseClass] = "Contoso.Web.Models.Pages.SitePageData",
                [OptionKeys.Group] = "Globals.GroupNames.Specialized",
                [OptionKeys.Description] = "Shows a product.",
            },
        },
        new("page-type-block-scoped-not-nullable", ItemCatalog.Ids.PageType, "LandingPage")
        {
            FileScopedNamespaces = false,
            Nullable = false,
            Options = { [OptionKeys.ViewModel] = "true" },
        },
        new("page-type-feature-layout", ItemCatalog.Ids.PageType, "EventPage")
        {
            TargetFolder = "Features/Events",
            Layout = Layouts.Feature,
            Options = { [OptionKeys.ViewModel] = "true" },
        },
        new("block-type-async", ItemCatalog.Ids.BlockType, "TeaserCardBlock"),
        new("block-type-sync-view-model", ItemCatalog.Ids.BlockType, "HeroBlock")
        {
            Options = { [OptionKeys.BlockStyle] = BlockStyles.Sync, [OptionKeys.ViewModel] = "true" },
        },
        new("block-type-plain-view-component", ItemCatalog.Ids.BlockType, "QuoteBlock")
        {
            Options = { [OptionKeys.BlockStyle] = BlockStyles.ViewComponent },
        },
        new("block-type-feature-layout", ItemCatalog.Ids.BlockType, "PromoBlock")
        {
            TargetFolder = "Features/Promo",
            Layout = Layouts.Feature,
        },
        new("media-type-image", ItemCatalog.Ids.MediaType, "ImageFile"),
        new("media-type-video", ItemCatalog.Ids.MediaType, "VideoFile")
        {
            Options = { [OptionKeys.BaseClass] = "EPiServer.Core.VideoData", [OptionKeys.Extensions] = ".mp4, .webm" },
        },
        new("media-type-generic", ItemCatalog.Ids.MediaType, "GenericMedia")
        {
            Options = { [OptionKeys.BaseClass] = "EPiServer.Core.MediaData", [OptionKeys.Extensions] = "pdf,docx" },
        },
        new("page-controller", ItemCatalog.Ids.PageController, "StandardPage")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage" },
        },
        new("block-component-sync", ItemCatalog.Ids.BlockComponent, "TeaserBlock")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Blocks.TeaserBlock", [OptionKeys.BlockStyle] = BlockStyles.Sync },
        },
        new("partial-component-async", ItemCatalog.Ids.PartialComponent, "StandardPagePartial")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage" },
        },
        new("partial-component-sync", ItemCatalog.Ids.PartialComponent, "StandardPagePartial")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.Async] = "false" },
        },
        new("view-component-async", ItemCatalog.Ids.ViewComponent, "SiteFooter"),
        new("view-component-sync-no-view", ItemCatalog.Ids.ViewComponent, "Breadcrumbs")
        {
            Options = { [OptionKeys.Async] = "false", [OptionKeys.View] = "false" },
        },
    };

    public static Scenario Get(string name) => All.Single(s => s.Name == name);

    public static IEnumerable<object[]> Names => All.Select(s => new object[] { s.Name });

    /// <summary>A context as if the extension had analysed <see cref="SupportSource"/>.</summary>
    public static ProjectContext CreateContext(Scenario scenario)
    {
        var context = new ProjectContext(ProjectDirectory, RootNamespace)
        {
            CmsMajorVersion = 12,
            FileScopedNamespaces = scenario.FileScopedNamespaces,
            NullableEnabled = scenario.Nullable,
        };
        context.PageBaseClasses.Add("Contoso.Web.Models.Pages.SitePageData");
        context.BlockBaseClasses.Add("Contoso.Web.Models.Blocks.SiteBlockData");
        context.PageTypes.Add("Contoso.Web.Models.Pages.StandardPage");
        context.BlockTypes.Add("Contoso.Web.Models.Blocks.TeaserBlock");
        context.GroupNames.Add(new GroupNameCandidate("Contoso.Web", "Globals.GroupNames.Specialized"));
        context.GroupNames.Add(new GroupNameCandidate("Contoso.Web", "Globals.GroupNames.Content"));
        return context;
    }

    public static GenerationRequest CreateRequest(Scenario scenario, string templateSource = TemplateSources.Embedded)
    {
        var item = ItemCatalog.Get(scenario.ItemId);
        var context = CreateContext(scenario);
        var config = new ScaffolderConfig { TemplateSource = templateSource, Layout = scenario.Layout };

        var options = OptionDefaults.For(item, context, config);

        // Scenarios start from plain Optimizely types unless they say otherwise, so snapshots
        // show the defaults rather than the detected Site* base classes.
        if (options.ContainsKey(OptionKeys.BaseClass))
        {
            options[OptionKeys.BaseClass] = item.Options.Single(o => o.Key == OptionKeys.BaseClass).DefaultValue;
        }

        options[OptionKeys.Guid] = FixedGuid;
        foreach (var pair in scenario.Options)
        {
            options[pair.Key] = pair.Value;
        }

        var target = scenario.TargetFolder.Length == 0
            ? ProjectDirectory
            : Path.Combine(ProjectDirectory, scenario.TargetFolder.Replace('/', Path.DirectorySeparatorChar));

        return new GenerationRequest(item, scenario.TypedName, target, options, context,
            new LoadedConfig(config, null, null) { RootDirectory = null });
    }

    public static Task<GenerationResult> GenerateAsync(Scenario scenario) =>
        new ScaffoldGenerator().GenerateAsync(CreateRequest(scenario));

    /// <summary>
    /// Hand-written types an Alloy-style project would already have. Compiled alongside the
    /// generated code, and analysed by the analyzer tests.
    /// </summary>
    public const string SupportSource = """
        using System.ComponentModel.DataAnnotations;
        using EPiServer.Core;
        using EPiServer.DataAnnotations;

        namespace Contoso.Web
        {
            public class Globals
            {
                [GroupDefinitions]
                public static class GroupNames
                {
                    [Display(Order = 10)]
                    public const string Specialized = "Specialized";

                    [Display(Order = 20)]
                    public const string Content = "Information";
                }
            }
        }

        namespace Contoso.Web.Models.Pages
        {
            public abstract class SitePageData : PageData
            {
            }

            [ContentType(GUID = "9CCC8A41-5C8C-4BE0-8E73-520FF3DE8267")]
            public class StandardPage : SitePageData
            {
            }
        }

        namespace Contoso.Web.Models.Blocks
        {
            public abstract class SiteBlockData : BlockData
            {
            }

            [ContentType(GUID = "EB67A99A-E239-41B8-9C59-20EAA5936047")]
            public class TeaserBlock : SiteBlockData
            {
            }
        }
        """;
}
