using ScaffolderForOptimizely.Core.Context;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests.Context;

public class ViewConventionDetectorTests
{
    private static readonly string[] BlockTypes = { "Alloy.Models.Blocks.TeaserBlock", "Alloy.Models.Blocks.ButtonBlock", "Alloy.Models.Blocks.PageListBlock" };

    [Fact]
    public void Learns_the_most_common_block_view_pattern()
    {
        var views = new[]
        {
            new RazorViewInfo("Views/Shared/Blocks/TeaserBlock.cshtml", "TeaserBlock"),
            new RazorViewInfo("Views/Shared/Blocks/ButtonBlock.cshtml", "Alloy.Models.Blocks.ButtonBlock"),
            new RazorViewInfo("Views/Shared/Components/PageListBlock/Default.cshtml", "PageListBlockViewModel"),
            new RazorViewInfo("Views/StartPage/Index.cshtml", "PageViewModel<StartPage>"),
        };

        Assert.Equal("Views/Shared/Blocks/{name}.cshtml", ViewConventionDetector.Detect(views, BlockTypes));
    }

    [Fact]
    public void Matches_view_models_and_generic_arguments()
    {
        var views = new[]
        {
            new RazorViewInfo("Views/StartPage/Index.cshtml", "PageViewModel<StartPage>"),
            new RazorViewInfo("Views/ArticlePage/Index.cshtml", "Alloy.Models.ViewModels.ArticlePageViewModel"),
        };

        Assert.Equal("Views/{name}/Index.cshtml", ViewConventionDetector.Detect(views, new[] { "StartPage", "ArticlePage" }));
    }

    [Fact]
    public void A_single_view_is_enough_but_a_tie_of_singles_is_not()
    {
        var one = new[] { new RazorViewInfo("Views/Pages/StartPage.cshtml", "StartPage") };
        Assert.Equal("Views/Pages/{name}.cshtml", ViewConventionDetector.Detect(one, new[] { "StartPage" }));

        var split = new[]
        {
            new RazorViewInfo("Views/Pages/StartPage.cshtml", "StartPage"),
            new RazorViewInfo("Views/ArticlePage/Index.cshtml", "ArticlePage"),
        };
        Assert.Null(ViewConventionDetector.Detect(split, new[] { "StartPage", "ArticlePage" }));
    }

    [Fact]
    public void Ignores_views_for_unknown_types_and_feature_folder_projects()
    {
        var views = new[] { new RazorViewInfo("Views/Shared/Footer.cshtml", "FooterViewModel") };
        Assert.Null(ViewConventionDetector.Detect(views, BlockTypes));

        var context = new ProjectContext("C:\\p", "P") { UsesFeatureFolders = true };
        context.BlockTypes.Add("TeaserBlock");
        context.RazorViews.Add(new RazorViewInfo("Features/Teaser/TeaserBlock.cshtml", "TeaserBlock"));
        ViewConventionDetector.Apply(context);
        Assert.Null(context.BlockViewPattern);
    }
}
