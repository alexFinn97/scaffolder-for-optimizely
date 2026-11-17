using System.Collections.Generic;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Generation;
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
    [InlineData("ArticlePage", "Article Page")]
    [InlineData("FAQPage", "FAQ Page")]
    [InlineData("Page", "Page")]
    public void Humanize_splits_words(string name, string expected)
    {
        Assert.Equal(expected, OutputPlanner.Humanize(name));
    }
}
