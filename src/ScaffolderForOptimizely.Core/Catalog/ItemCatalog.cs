using System;
using System.Collections.Generic;
using System.Linq;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>Every item the extension can scaffold.</summary>
public static class ItemCatalog
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
    }

    public static class Categories
    {
        public const string CmsContentTypes = "CMS/Content types";
        public const string CmsRendering = "CMS/Rendering";
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
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
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
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
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
        "A PageController<T> for an existing page type, with an optional Razor view.",
        OutputRole.Controller,
        new[]
        {
            ContentTypeOption(ChoiceSource.PageTypes, "EPiServer.Core.PageData"),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
        },
        new[]
        {
            new OutputDefinition("controller", OutputRole.Controller, "PageController.cs.sbn")
            {
                DotnetNew = new DotnetNewMapping("epi-cms-pagecontroller") { PassContentType = true, RendersView = true },
            },
            new OutputDefinition("view", OutputRole.View, "PageView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.PageIndex,
            },
        })
    {
        NameSuffix = "Controller",
        NameExample = "ArticlePageController",
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
            ContentTypeOption(ChoiceSource.BlockTypes, "EPiServer.Core.BlockData"),
            BlockStyleOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
        },
        new[]
        {
            new OutputDefinition("component", OutputRole.Component, "BlockComponent.cs.sbn"),
            new OutputDefinition("view", OutputRole.View, "ComponentView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.ComponentDefault,
            },
        })
    {
        NameSuffix = "ViewComponent",
        NameExample = "TeaserBlockViewComponent",
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
            ContentTypeOption(ChoiceSource.ContentTypes, "EPiServer.Core.PageData"),
            new ItemOption(OptionKeys.Async, "Async", OptionKind.Bool) { DefaultValue = "true" },
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
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

    private static ItemOption GroupOption() => new(OptionKeys.Group, "Content type group", OptionKind.Choice)
    {
        ChoiceSource = ChoiceSource.ContentTypeGroups,
        AllowCustomValue = true,
        Description = "Groups the type in the editor's create dialog. Constants found in the project are listed.",
    };

    private static ItemOption ContentTypeOption(ChoiceSource source, string defaultValue) => new(OptionKeys.ContentType, "Content type", OptionKind.Choice)
    {
        ChoiceSource = source,
        DefaultValue = defaultValue,
        AllowCustomValue = true,
        Description = "Content types found in the project are listed.",
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
