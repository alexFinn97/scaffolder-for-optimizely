using System.Collections.Generic;
using System.Linq;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Generation;
using ScaffolderForOptimizely.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests;

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

        var defaults = OptionDefaults.For(ItemCatalog.Get(ItemCatalog.Ids.PageController), Scenarios.CreateContext(Scenarios.Get("page-controller")), new Config.ScaffolderConfig());
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

    [Theory]
    [InlineData("ArticlePage", "Article Page")]
    [InlineData("FAQPage", "FAQ Page")]
    [InlineData("Page", "Page")]
    public void Humanize_splits_words(string name, string expected)
    {
        Assert.Equal(expected, OutputPlanner.Humanize(name));
    }
}
