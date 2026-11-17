using System.Collections.Generic;
using System.Linq;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Generation;
using ScaffolderForOptimizely.Core.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests.Generation;

public class OutputPlannerTests
{
    [Theory]
    [InlineData(ItemCatalog.Ids.PageType, "articlePage", "ArticlePage")]
    [InlineData(ItemCatalog.Ids.PageType, " Article Page ", "ArticlePage")]
    [InlineData(ItemCatalog.Ids.PageType, "1stPage", "_1stPage")]
    [InlineData(ItemCatalog.Ids.PageController, "ArticlePage", "ArticlePageController")]
    [InlineData(ItemCatalog.Ids.PageController, "ArticlePageController", "ArticlePageController")]
    [InlineData(ItemCatalog.Ids.BlockComponent, "TeaserBlock", "TeaserBlockViewComponent")]
    [InlineData(ItemCatalog.Ids.BlockComponent, "TeaserBlockComponent", "TeaserBlockComponent")]
    [InlineData(ItemCatalog.Ids.ViewComponent, "SiteFooterViewComponent", "SiteFooterViewComponent")]
    public void NormalizeName_adds_suffix_and_fixes_identifiers(string itemId, string typed, string expected)
    {
        Assert.Equal(expected, OutputPlanner.NormalizeName(typed, ItemCatalog.Get(itemId)));
    }

    [Theory]
    [InlineData("Contoso.Web.Models.Pages.StandardPage", "StandardPageController")]
    [InlineData("EPiServer.Core.PageData", "")]
    public void SuggestName_follows_the_content_type(string contentType, string expected)
    {
        var options = new Dictionary<string, string> { [OptionKeys.ContentType] = contentType };
        Assert.Equal(expected, OutputPlanner.SuggestName(ItemCatalog.Get(ItemCatalog.Ids.PageController), options));
    }

    [Theory]
    [InlineData("Views/Pages/Standard", "Views/Pages/Standard.cshtml")]
    [InlineData("~/Views/Pages/Standard.cshtml", "Views/Pages/Standard.cshtml")]
    [InlineData(@"\Views\Pages\Standard.CSHTML", "Views/Pages/Standard.CSHTML")]
    [InlineData("../Other/View.cshtml", null)]
    [InlineData("~/", null)]
    public void ResolveViewPath_normalises_and_stays_inside_the_project(string typed, string? expectedRelative)
    {
        var project = Scenarios.ProjectDirectory;

        var resolved = OutputPlanner.ResolveViewPath(project, typed);

        if (expectedRelative is null)
        {
            Assert.Null(resolved);
        }
        else
        {
            Assert.Equal(System.IO.Path.Combine(project, expectedRelative.Replace('/', System.IO.Path.DirectorySeparatorChar)), resolved);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task An_outside_view_path_falls_back_with_a_warning()
    {
        var scenario = Scenarios.Get("page-controller");
        var request = Scenarios.CreateRequest(scenario);
        request.Options[OptionKeys.ViewPath] = "../../elsewhere.cshtml";

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Contains(result.Files, f => f.RelativePath == "Views/StandardPage/Index.cshtml");
        Assert.Contains(result.Warnings, w => w.Contains("outside the project"));
    }

    [Fact]
    public async System.Threading.Tasks.Task Controllers_need_a_content_type_and_it_starts_blank()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("page-controller"));
        request.Options[OptionKeys.ContentType] = string.Empty;

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("Choose a content type.", result.Error);
        Assert.Empty(result.Files);

        var defaults = OptionDefaults.For(ItemCatalog.Get(ItemCatalog.Ids.PageController), Scenarios.CreateContext(Scenarios.Get("page-controller")), new ScaffolderConfig());
        Assert.Equal(string.Empty, defaults[OptionKeys.ContentType]);
    }

    [Fact]
    public async System.Threading.Tasks.Task Reports_resolved_paths_for_the_dialog()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-view-model"));

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("Views/ArticlePage/Index.cshtml", result.ResolvedOptions[OptionKeys.ViewPath]);
        Assert.Equal("Models/ViewModels/ArticlePageViewModel.cs", result.ResolvedOptions[OptionKeys.ViewModelPath]);
        Assert.Equal("false", result.ResolvedOptions[OptionKeys.PassViewPath]);
    }

    [Fact]
    public async System.Threading.Tasks.Task Config_view_paths_and_pass_setting_win_over_detection()
    {
        var scenario = Scenarios.Get("page-type-detected-view-pattern");
        var request = Scenarios.CreateRequest(scenario);
        request.Config.Config.ViewPaths.Page = "Views/Content/{name}/Index";
        request.Config.Config.PassViewPath = false;

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("Views/Content/ArticlePage/Index.cshtml", result.ResolvedOptions[OptionKeys.ViewPath]);
        Assert.Equal("false", result.ResolvedOptions[OptionKeys.PassViewPath]);
        Assert.Contains("return View(currentPage);", result.Files.Single(f => f.OutputId == "controller").Content);
    }

    [Fact]
    public async System.Threading.Tasks.Task Content_events_handler_needs_at_least_one_event()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("content-events-default"));
        foreach (var key in request.Options.Keys.Where(k => k.StartsWith(OptionKeys.EventPrefix, System.StringComparison.Ordinal)).ToList())
        {
            request.Options[key] = "false";
        }

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("Choose at least one event.", result.Error);
    }

    [Fact]
    public async System.Threading.Tasks.Task Configured_folder_beats_detected_folder_for_infrastructure_items()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("scheduled-job-detected-folder"));
        request.Config.Config.Folders.ScheduledJobs = "Jobs";

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("Jobs/ReindexJob.cs", Assert.Single(result.Files).RelativePath);
    }

    [Fact]
    public async System.Threading.Tasks.Task Derived_names_follow_the_item_name_until_typed()
    {
        var scenario = Scenarios.Get("service-collection-extension");
        var request = Scenarios.CreateRequest(scenario);
        Assert.Equal("AddSite", (await new ScaffoldGenerator().GenerateAsync(request)).ResolvedOptions[OptionKeys.MethodName]);

        request.Options[OptionKeys.MethodName] = "Add Site Services!";
        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Equal("AddSiteServices", result.ResolvedOptions[OptionKeys.MethodName]);
        Assert.Contains("public static IServiceCollection AddSiteServices(this IServiceCollection services)", Assert.Single(result.Files).Content);
    }

    [Fact]
    public void Catalog_items_offer_base_classes_for_their_own_root()
    {
        var context = Scenarios.CreateContext(Scenarios.Get("product-defaults"));
        context.CatalogBaseClasses[CatalogRoots.Product] = new List<string> { "Contoso.Web.Models.Catalog.BaseProduct" };
        context.CatalogBaseClasses[CatalogRoots.Variation] = new List<string> { "Contoso.Web.Models.Catalog.BaseVariant" };
        var product = ItemCatalog.Get(ItemCatalog.Ids.Product);
        var baseClass = product.Options.Single(o => o.Key == OptionKeys.BaseClass);

        var choices = OptionDefaults.ChoicesFor(baseClass, product, context).Select(c => c.Value);
        var defaults = OptionDefaults.For(product, context, new ScaffolderConfig());

        Assert.Equal(new[] { "Contoso.Web.Models.Catalog.BaseProduct", CatalogRoots.Product }, choices);
        Assert.Equal("Contoso.Web.Models.Catalog.BaseProduct", defaults[OptionKeys.BaseClass]);

        var config = new ScaffolderConfig();
        config.BaseClasses.Variation = "Contoso.Web.Models.Catalog.SiteVariant";
        var variationDefaults = OptionDefaults.For(ItemCatalog.Get(ItemCatalog.Ids.Variation), context, config);
        Assert.Equal("Contoso.Web.Models.Catalog.SiteVariant", variationDefaults[OptionKeys.BaseClass]);
    }

    [Fact]
    public async System.Threading.Tasks.Task Catalog_types_use_the_catalog_folder_and_view_pattern()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("product-defaults"));
        request.Context.CatalogViewPattern = "Views/Catalog/{name}.cshtml";

        var result = await new ScaffoldGenerator().GenerateAsync(request);

        Assert.Contains(result.Files, f => f.RelativePath == "Models/Catalog/ShirtProduct.cs");
        Assert.Contains(result.Files, f => f.RelativePath == "Views/Catalog/ShirtProduct.cshtml");
        Assert.Equal("ShirtProduct", result.ResolvedOptions[OptionKeys.MetaClassName]);
    }

    [Fact]
    public async System.Threading.Tasks.Task A_typed_controller_path_sets_the_folder_class_and_default_view()
    {
        var defaults = await new ScaffoldGenerator().GenerateAsync(Scenarios.CreateRequest(Scenarios.Get("product-defaults")));
        Assert.Equal("Controllers/ShirtProductController.cs", defaults.ResolvedOptions[OptionKeys.ControllerPath]);

        var result = await new ScaffoldGenerator().GenerateAsync(Scenarios.CreateRequest(Scenarios.Get("product-controller-path")));

        var controller = result.Files.Single(f => f.OutputId == "controller");
        Assert.Equal("Controllers/Commerce/BootController.cs", controller.RelativePath);
        Assert.Contains("namespace Contoso.Web.Controllers.Commerce;", controller.Content);
        Assert.Contains("public class BootController : ContentController<BootProduct>", controller.Content);
        Assert.Contains(result.Files, f => f.RelativePath == "Views/Boot/Index.cshtml");
        Assert.Equal("Controllers/Commerce/BootController.cs", result.ResolvedOptions[OptionKeys.ControllerPath]);
    }

    [Fact]
    public async System.Threading.Tasks.Task Configured_catalog_controller_folder_beats_detection_and_leaves_cms_controllers_alone()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("product-detected-controller-folder"));
        request.Config.Config.Folders.CatalogControllers = "Commerce/Controllers";

        var result = await new ScaffoldGenerator().GenerateAsync(request);
        Assert.Equal("Commerce/Controllers/HatProductController.cs", result.Files.Single(f => f.OutputId == "controller").RelativePath);

        var page = Scenarios.CreateRequest(Scenarios.Get("page-type-defaults"));
        page.Config.Config.Folders.CatalogControllers = "Commerce/Controllers";
        var pageResult = await new ScaffoldGenerator().GenerateAsync(page);
        Assert.Equal("Controllers/ArticlePageController.cs", pageResult.Files.Single(f => f.OutputId == "controller").RelativePath);
    }

    [Theory]
    [InlineData("ArticlePage", "Article Page")]
    [InlineData("FAQPage", "FAQ Page")]
    [InlineData("Page", "Page")]
    public void Humanize_splits_words(string name, string expected)
    {
        Assert.Equal(expected, OutputPlanner.Humanize(name));
    }
}
