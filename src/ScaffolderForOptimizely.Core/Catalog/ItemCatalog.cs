using System;
using System.Collections.Generic;
using System.Linq;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>Every item the extension can scaffold.</summary>
public static partial class ItemCatalog
{
    public static class Ids
    {
        public const string PageType = "cms-page-type";
        public const string BlockType = "cms-block-type";
        public const string MediaType = "cms-media-type";
        public const string PageController = "cms-page-controller";
        public const string BlockComponent = "cms-block-component";
        public const string PartialComponent = "cms-partial-component";
        public const string ViewComponent = "cms-view-component";
        public const string InitializationModule = "cms-initialization-module";
        public const string StartupFilter = "cms-startup-filter";
        public const string ScheduledJob = "cms-scheduled-job";
        public const string ServiceCollectionExtension = "cms-service-collection-extension";
        public const string SelectionFactory = "cms-selection-factory";
        public const string EditorDescriptor = "cms-editor-descriptor";
        public const string VisitorGroupCriterion = "cms-visitor-group-criterion";
        public const string ContentEventsHandler = "cms-content-events-handler";
        public const string ContentValidator = "cms-content-validator";
        public const string Product = "commerce-product";
        public const string Variation = "commerce-variation";
        public const string Bundle = "commerce-bundle";
        public const string Package = "commerce-package";
        public const string CatalogCategory = "commerce-category";
        public const string ContentController = "commerce-content-controller";
        public const string Promotion = "commerce-promotion";
        public const string PaymentProvider = "commerce-payment-provider";
        public const string ShippingProvider = "commerce-shipping-provider";
        public const string OrderServiceDecorator = "commerce-order-service-decorator";
    }

    public static class Categories
    {
        public const string CmsContentTypes = "CMS/Content types";
        public const string CmsRendering = "CMS/Rendering";
        public const string CmsInfrastructure = "CMS/Infrastructure";
        public const string CmsEditor = "CMS/Editor";
        public const string CommerceCatalog = "Commerce/Catalog";
        public const string CommerceRendering = "Commerce/Rendering";
        public const string CommerceMarketing = "Commerce/Marketing";
        public const string CommerceOrders = "Commerce/Orders";

        /// <summary>Commerce items are only offered in projects that reference Optimizely Commerce.</summary>
        public static bool IsCommerce(string category) => category.StartsWith("Commerce/", StringComparison.Ordinal);
    }

    private static readonly Lazy<IReadOnlyList<ItemDefinition>> LazyAll = new(Build);

    public static IReadOnlyList<ItemDefinition> All => LazyAll.Value;

    public static ItemDefinition Get(string id) =>
        All.FirstOrDefault(i => i.Id == id) ?? throw new ArgumentException($"Unknown item '{id}'.", nameof(id));

    private static IReadOnlyList<ItemDefinition> Build() => new List<ItemDefinition>
    {
        PageType(),
        BlockType(),
        MediaType(),
        PageController(),
        BlockComponent(),
        PartialComponent(),
        ViewComponent(),
        InitializationModule(),
        StartupFilter(),
        ScheduledJob(),
        ServiceCollectionExtension(),
        SelectionFactory(),
        EditorDescriptor(),
        VisitorGroupCriterion(),
        ContentEventsHandler(),
        ContentValidator(),
        CatalogType(Ids.Product, "Product", CatalogRoots.Product, "A product: the item customers browse to, grouping its variations.", "FashionProduct"),
        CatalogType(Ids.Variation, "Variation", CatalogRoots.Variation, "A variation (SKU): the item customers buy, with its own price and stock.", "FashionVariation"),
        CatalogType(Ids.Bundle, "Bundle", CatalogRoots.Bundle, "A bundle: entries sold together but priced and shipped separately.", "OutfitBundle"),
        CatalogType(Ids.Package, "Package", CatalogRoots.Package, "A package: entries sold as one item with a single price.", "StarterPackage"),
        CatalogType(Ids.CatalogCategory, "Category", CatalogRoots.Category, "A catalog category (node) that groups products.", "FashionCategory"),
        ContentController(),
        Promotion(),
        PaymentProvider(),
        ShippingProvider(),
        OrderServiceDecorator(),
    };

    private static ItemDefinition PageType() => new(
        Ids.PageType,
        Categories.CmsContentTypes,
        "Page Type",
        "A page content type, with an optional controller, view model and Razor view.",
        OutputRole.Model,
        new[]
        {
            DisplayNameOption(),
            DescriptionOption(),
            BaseClassOption("EPiServer.Core.PageData"),
            GroupOption(),
            Companion(OptionKeys.Controller, "Controller", "companions.controller", true),
            Companion(OptionKeys.ViewModel, "View model", "companions.viewModel", false),
            ViewModelPathOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("model", OutputRole.Model, "PageType.cs.sbn")
            {
                DotnetNew = ContentTypeMapping(),
            },
            new OutputDefinition("controller", OutputRole.Controller, "PageController.cs.sbn")
            {
                EnabledByOption = OptionKeys.Controller,
                DotnetNew = new DotnetNewMapping("epi-cms-pagecontroller")
                {
                    PassContentType = true,
                    UnsupportedWhenOptionsSet = new[] { OptionKeys.ViewModel },
                    RendersView = true,
                },
            },
            new OutputDefinition("viewModel", OutputRole.ViewModel, "PageViewModel.cs.sbn")
            {
                EnabledByOption = OptionKeys.ViewModel,
            },
            new OutputDefinition("view", OutputRole.View, "PageView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.PageIndex,
                ViewFor = ContentKind.Page,
            },
        })
    {
        ContentKind = ContentKind.Page,
        NameExample = "ArticlePage",
    };

    private static ItemDefinition BlockType() => new(
        Ids.BlockType,
        Categories.CmsContentTypes,
        "Block Type",
        "A block content type, with an optional block component, view model and Razor view.",
        OutputRole.Model,
        new[]
        {
            DisplayNameOption(),
            DescriptionOption(),
            BaseClassOption("EPiServer.Core.BlockData"),
            GroupOption(),
            Companion(OptionKeys.Component, "Block component", "companions.component", true),
            BlockStyleOption(),
            Companion(OptionKeys.ViewModel, "View model", "companions.viewModel", false),
            ViewModelPathOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("model", OutputRole.Model, "BlockType.cs.sbn")
            {
                DotnetNew = ContentTypeMapping(),
            },
            new OutputDefinition("component", OutputRole.Component, "BlockComponent.cs.sbn")
            {
                EnabledByOption = OptionKeys.Component,
            },
            new OutputDefinition("viewModel", OutputRole.ViewModel, "BlockViewModel.cs.sbn")
            {
                EnabledByOption = OptionKeys.ViewModel,
            },
            new OutputDefinition("view", OutputRole.View, "ComponentView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.ComponentDefault,
                ViewFor = ContentKind.Block,
            },
        })
    {
        ContentKind = ContentKind.Block,
        NameExample = "TeaserBlock",
    };

    private static ItemDefinition MediaType() => new(
        Ids.MediaType,
        Categories.CmsContentTypes,
        "Media Type",
        "An image, video or generic file content type mapped to file extensions.",
        OutputRole.Model,
        new[]
        {
            DisplayNameOption(),
            DescriptionOption(),
            BaseClassOption("EPiServer.Core.ImageData"),
            new ItemOption(OptionKeys.Extensions, "File extensions", OptionKind.Text)
            {
                DefaultValue = "jpg,jpeg,png,gif,webp",
                Description = "Comma-separated, without dots.",
            },
        },
        new[]
        {
            new OutputDefinition("model", OutputRole.Model, "MediaType.cs.sbn"),
        })
    {
        ContentKind = ContentKind.Media,
        NameExample = "ImageFile",
    };

    private static ItemDefinition PageController() => new(
        Ids.PageController,
        Categories.CmsRendering,
        "Page Controller",
        "A PageController<T> for an existing page type, with an optional view model and Razor view.",
        OutputRole.Controller,
        new[]
        {
            ContentTypeOption(ChoiceSource.PageTypes),
            Companion(OptionKeys.ViewModel, "Use a view model", "companions.viewModel", false),
            ViewModelPathOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("controller", OutputRole.Controller, "PageController.cs.sbn")
            {
                DotnetNew = new DotnetNewMapping("epi-cms-pagecontroller")
                {
                    PassContentType = true,
                    RendersView = true,
                    UnsupportedWhenOptionsSet = new[] { OptionKeys.ViewModel },
                },
            },
            new OutputDefinition("viewModel", OutputRole.ViewModel, "PageViewModel.cs.sbn")
            {
                EnabledByOption = OptionKeys.ViewModel,
            },
            new OutputDefinition("view", OutputRole.View, "PageView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.PageIndex,
                ViewFor = ContentKind.Page,
            },
        })
    {
        NameSuffix = "Controller",
        NameExample = "ArticlePageController",
        DefaultName = "NewPageController",
        NameFromOption = OptionKeys.ContentType,
    };

    private static ItemDefinition BlockComponent() => new(
        Ids.BlockComponent,
        Categories.CmsRendering,
        "Block Component",
        "Renders an existing block type (CMS 12's replacement for BlockController).",
        OutputRole.Component,
        new[]
        {
            ContentTypeOption(ChoiceSource.BlockTypes),
            BlockStyleOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("component", OutputRole.Component, "BlockComponent.cs.sbn"),
            new OutputDefinition("view", OutputRole.View, "ComponentView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.ComponentDefault,
                ViewFor = ContentKind.Block,
            },
        })
    {
        NameSuffix = "ViewComponent",
        NameExample = "TeaserBlockViewComponent",
        DefaultName = "NewBlockViewComponent",
        NameFromOption = OptionKeys.ContentType,
    };

    private static ItemDefinition PartialComponent() => new(
        Ids.PartialComponent,
        Categories.CmsRendering,
        "Partial Content Component",
        "A PartialContentComponent<T> that renders any content type inside a content area.",
        OutputRole.Component,
        new[]
        {
            ContentTypeOption(ChoiceSource.ContentTypes),
            new ItemOption(OptionKeys.Async, "Async", OptionKind.Bool) { DefaultValue = "true" },
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("component", OutputRole.Component, "PartialComponent.cs.sbn")
            {
                DotnetNew = new DotnetNewMapping("epi-cms-contentcomponent")
                {
                    PassContentType = true,
                    UnsupportedWhenOptionsSet = new[] { OptionKeys.Async },
                    RendersView = true,
                },
            },
            new OutputDefinition("view", OutputRole.View, "ComponentView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.ComponentDefault,
            },
        })
    {
        NameSuffix = "ViewComponent",
        NameExample = "ArticlePagePartialViewComponent",
    };

    private static ItemDefinition ViewComponent() => new(
        Ids.ViewComponent,
        Categories.CmsRendering,
        "View Component",
        "A plain ASP.NET Core ViewComponent with an optional Razor view.",
        OutputRole.Component,
        new[]
        {
            new ItemOption(OptionKeys.Async, "Async", OptionKind.Bool) { DefaultValue = "true" },
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        new[]
        {
            new OutputDefinition("component", OutputRole.Component, "ViewComponent.cs.sbn"),
            new OutputDefinition("view", OutputRole.View, "ComponentView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.ComponentDefault,
            },
        })
    {
        NameSuffix = "ViewComponent",
        NameExample = "SiteFooterViewComponent",
    };

    private static ItemOption DisplayNameOption() => new(OptionKeys.DisplayName, "Display name", OptionKind.Text)
    {
        Description = "Shown to editors. Leave empty to use the class name.",
    };

    private static ItemOption DescriptionOption() => new(OptionKeys.Description, "Description", OptionKind.Text);

    private static ItemOption BaseClassOption(string defaultValue) => new(OptionKeys.BaseClass, "Base class", OptionKind.Choice)
    {
        ChoiceSource = ChoiceSource.BaseClasses,
        DefaultValue = defaultValue,
        AllowCustomValue = true,
        Description = "Abstract base classes found in the project are listed first.",
    };

    /// <summary>
    /// Optimizely's content type template only takes a name, namespace and base type, so a display
    /// name, description or group means using the built-in template.
    /// </summary>
    private static DotnetNewMapping ContentTypeMapping() => new("epi-cms-contenttype")
    {
        PassBaseType = true,
        UnsupportedWhenOptionsSet = new[] { OptionKeys.DisplayName, OptionKeys.Description, OptionKeys.Group },
    };

    /// <summary>
    /// Where the Razor view goes. Left empty, the configured, detected or MVC default path is
    /// used; the dialog shows that path until the user edits it.
    /// </summary>
    private static ItemOption ViewPathOption() => new(OptionKeys.ViewPath, "View path", OptionKind.Text)
    {
        Description = "Relative to the project. Follows your existing views where possible.",
        EnabledWhenOption = OptionKeys.View,
        IsAutoValue = true,
    };

    /// <summary>
    /// Whether the controller/component names the view path in View(). Left alone, it's ticked when
    /// MVC wouldn't find the view by itself and the project has no IViewLocationExpander.
    /// </summary>
    private static ItemOption PassViewPathOption() => new(OptionKeys.PassViewPath, "Pass view path to View()", OptionKind.Bool)
    {
        EnabledWhenOption = OptionKeys.View,
        IsAutoValue = true,
    };

    /// <summary>
    /// Where the view model goes. The file name is the class name. Left empty, the configured or
    /// detected view model folder is used.
    /// </summary>
    private static ItemOption ViewModelPathOption() => new(OptionKeys.ViewModelPath, "View model path", OptionKind.Text)
    {
        Description = "Relative to the project. The file name becomes the class name.",
        EnabledWhenOption = OptionKeys.ViewModel,
        IsAutoValue = true,
    };

    private static ItemOption GroupOption() => new(OptionKeys.Group, "Content type group", OptionKind.Choice)
    {
        ChoiceSource = ChoiceSource.ContentTypeGroups,
        AllowCustomValue = true,
        Description = "Groups the type in the editor's create dialog. Constants found in the project are listed.",
    };

    /// <summary>Starts blank so a controller/component is never created for the wrong type by accident.</summary>
    private static ItemOption ContentTypeOption(ChoiceSource source) => new(OptionKeys.ContentType, "Content type", OptionKind.Choice)
    {
        ChoiceSource = source,
        AllowCustomValue = true,
        Description = "Required. Content types found in the project are listed.",
    };

    private static ItemOption BlockStyleOption() => new(OptionKeys.BlockStyle, "Component style", OptionKind.Choice)
    {
        DefaultValue = BlockStyles.Async,
        ConfigDefault = "blockRenderStyle",
        Choices = new[]
        {
            new OptionChoice(BlockStyles.Async, "AsyncBlockComponent<T>"),
            new OptionChoice(BlockStyles.Sync, "BlockComponent<T>"),
            new OptionChoice(BlockStyles.ViewComponent, "Plain ViewComponent (not registered as a block template)"),
        },
    };

    private static ItemOption Companion(string key, string label, string configDefault, bool defaultValue) => new(key, label, OptionKind.Bool)
    {
        DefaultValue = defaultValue ? "true" : "false",
        ConfigDefault = configDefault,
    };
}
