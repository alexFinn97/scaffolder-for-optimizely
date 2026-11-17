using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Core.Tests.Shared;

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

    /// <summary>Simulates an existing ViewModels folder found in the project.</summary>
    public string? ViewModelsFolder { get; set; }

    /// <summary>Simulates view patterns learnt from existing views.</summary>
    public string? PageViewPattern { get; set; }

    public string? BlockViewPattern { get; set; }

    /// <summary>Simulates a custom IViewLocationExpander in the project.</summary>
    public bool HasViewLocationExpander { get; set; }

    /// <summary>Simulates folders detected from existing implementations (FolderKeys → folder).</summary>
    public Dictionary<string, string> DetectedFolders { get; set; } = new();

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
        new("page-type-custom-view-path", ItemCatalog.Ids.PageType, "ArticlePage")
        {
            Options = { [OptionKeys.ViewModel] = "true", [OptionKeys.ViewPath] = "Views/Pages/Article" },
        },
        new("page-type-custom-view-model-path", ItemCatalog.Ids.PageType, "ArticlePage")
        {
            Options = { [OptionKeys.ViewModel] = "true", [OptionKeys.ViewModelPath] = "Features/Article/ArticleModel" },
        },
        new("page-type-detected-view-pattern", ItemCatalog.Ids.PageType, "ArticlePage")
        {
            PageViewPattern = "Views/Pages/{name}.cshtml",
        },
        new("block-type-detected-view-pattern-with-expander", ItemCatalog.Ids.BlockType, "TeaserCardBlock")
        {
            BlockViewPattern = "Views/Shared/Blocks/{name}.cshtml",
            HasViewLocationExpander = true,
        },
        new("block-type-async", ItemCatalog.Ids.BlockType, "TeaserCardBlock"),
        new("block-type-custom-view-path", ItemCatalog.Ids.BlockType, "TeaserCardBlock")
        {
            Options = { [OptionKeys.ViewPath] = "~/Views/Shared/Blocks/TeaserCard.cshtml" },
        },
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
        new("page-controller-view-model", ItemCatalog.Ids.PageController, "StandardPage")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.ViewModel] = "true" },
        },
        new("page-controller-detected-view-models-folder", ItemCatalog.Ids.PageController, "StandardPage")
        {
            ViewModelsFolder = "Features/Shared/ViewModels",
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.ViewModel] = "true" },
        },
        new("page-controller-custom-view-path", ItemCatalog.Ids.PageController, "StandardPage")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.ViewPath] = "~/Views/Pages/Standard" },
        },
        new("page-controller-view-path-not-passed", ItemCatalog.Ids.PageController, "StandardPage")
        {
            Options =
            {
                [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage",
                [OptionKeys.ViewPath] = "Views/Pages/Standard",
                [OptionKeys.PassViewPath] = "false",
            },
        },
        new("page-controller-typed-conventional-view-path", ItemCatalog.Ids.PageController, "StandardPage")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.ViewPath] = @"Views\StandardPage\Index.cshtml" },
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
        new("view-component-custom-view-path", ItemCatalog.Ids.ViewComponent, "SiteFooter")
        {
            Options = { [OptionKeys.ViewPath] = "Views/Shared/Footer" },
        },
        new("partial-component-custom-view-path", ItemCatalog.Ids.PartialComponent, "StandardPagePartial")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage", [OptionKeys.ViewPath] = "Views/Shared/Partials/StandardPage" },
        },
        // v0.2: infrastructure and editor items.
        new("init-module-plain", ItemCatalog.Ids.InitializationModule, "DisplayRegistryInitialization"),
        new("init-module-configurable", ItemCatalog.Ids.InitializationModule, "ServicesInitialization")
        {
            Options = { [OptionKeys.Configurable] = "true" },
        },
        new("startup-filter-defaults", ItemCatalog.Ids.StartupFilter, "Site"),
        new("startup-filter-middleware-only", ItemCatalog.Ids.StartupFilter, "SecurityHeadersStartupFilter")
        {
            Options =
            {
                [OptionKeys.Scope] = "false",
                [OptionKeys.Logger] = "false",
                [OptionKeys.Middleware] = "true",
                [OptionKeys.Registration] = "false",
            },
        },
        new("startup-filter-detected-folder", ItemCatalog.Ids.StartupFilter, "Warmup")
        {
            DetectedFolders = { [FolderKeys.StartupFilters] = "Startup/Filters" },
            Options = { [OptionKeys.Middleware] = "true" },
        },
        new("scheduled-job-defaults", ItemCatalog.Ids.ScheduledJob, "ImportProducts")
        {
            Options = { [OptionKeys.Description] = "Imports products from the \"PIM\"." },
        },
        new("scheduled-job-restartable-no-extras", ItemCatalog.Ids.ScheduledJob, "CleanupJob")
        {
            Nullable = false,
            Options =
            {
                [OptionKeys.Stoppable] = "false",
                [OptionKeys.StatusProgress] = "false",
                [OptionKeys.Logger] = "false",
                [OptionKeys.Restartable] = "true",
            },
        },
        new("scheduled-job-detected-folder", ItemCatalog.Ids.ScheduledJob, "ReindexJob")
        {
            DetectedFolders = { [FolderKeys.ScheduledJobs] = "Infrastructure/Jobs" },
            Options = { [OptionKeys.Logger] = "false" },
        },
        new("service-collection-extension", ItemCatalog.Ids.ServiceCollectionExtension, "Site"),
        new("selection-factory-list", ItemCatalog.Ids.SelectionFactory, "ColourSelectionFactory"),
        new("selection-factory-autosuggest", ItemCatalog.Ids.SelectionFactory, "CountrySelectionQuery")
        {
            Options = { [OptionKeys.Autosuggest] = "true" },
        },
        new("selection-factory-autosuggest-not-nullable", ItemCatalog.Ids.SelectionFactory, "CitySelectionQuery")
        {
            Nullable = false,
            Options = { [OptionKeys.Autosuggest] = "true" },
        },
        new("editor-descriptor-defaults", ItemCatalog.Ids.EditorDescriptor, "Colour"),
        new("editor-descriptor-custom", ItemCatalog.Ids.EditorDescriptor, "LinkPickerEditorDescriptor")
        {
            Options =
            {
                [OptionKeys.TargetType] = "ContentReference",
                [OptionKeys.UiHint] = "link-picker",
                [OptionKeys.ClientEditingClass] = "app/editors/LinkPicker",
            },
        },
        new("visitor-group-criterion", ItemCatalog.Ids.VisitorGroupCriterion, "Country")
        {
            Options = { [OptionKeys.Description] = "Matches the visitor's country." },
        },
        new("content-events-default", ItemCatalog.Ids.ContentEventsHandler, "ContentPublishing"),
        new("content-events-all-not-nullable", ItemCatalog.Ids.ContentEventsHandler, "AuditTrail")
        {
            Nullable = false,
            FileScopedNamespaces = false,
            Options =
            {
                [OptionKeys.EventPrefix + "SavingContent"] = "true",
                [OptionKeys.EventPrefix + "SavedContent"] = "true",
                [OptionKeys.EventPrefix + "MovingContent"] = "true",
                [OptionKeys.EventPrefix + "MovedContent"] = "true",
                [OptionKeys.EventPrefix + "DeletingContent"] = "true",
                [OptionKeys.EventPrefix + "DeletedContent"] = "true",
            },
        },
        new("content-validator", ItemCatalog.Ids.ContentValidator, "StandardPage")
        {
            Options = { [OptionKeys.ContentType] = "Contoso.Web.Models.Pages.StandardPage" },
        },
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
            ViewModelsFolder = scenario.ViewModelsFolder,
            PageViewPattern = scenario.PageViewPattern,
            BlockViewPattern = scenario.BlockViewPattern,
            HasViewLocationExpander = scenario.HasViewLocationExpander,
        };
        foreach (var pair in scenario.DetectedFolders)
        {
            context.DetectedFolders[pair.Key] = pair.Value;
        }

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
