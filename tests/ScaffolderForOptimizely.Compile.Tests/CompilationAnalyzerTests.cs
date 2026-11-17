using System.Linq;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Compile.Tests;

public class CompilationAnalyzerTests
{
    [Fact]
    public void Detects_alloy_style_base_classes_content_types_and_group_names()
    {
        var compilation = TestCompiler.Create(new[] { ("Support.cs", Scenarios.SupportSource) }, nullable: true);
        var context = new ProjectContext(Scenarios.ProjectDirectory, Scenarios.RootNamespace);

        CompilationAnalyzer.Apply(compilation, context);

        Assert.Equal(new[] { "Contoso.Web.Models.Pages.SitePageData" }, context.PageBaseClasses);
        Assert.Equal(new[] { "Contoso.Web.Models.Pages.StandardPage" }, context.PageTypes);
        Assert.Equal(new[] { "Contoso.Web.Models.Blocks.SiteBlockData" }, context.BlockBaseClasses);
        Assert.Equal(new[] { "Contoso.Web.Models.Blocks.TeaserBlock" }, context.BlockTypes);
        Assert.Equal(
            new[] { "Globals.GroupNames.Specialized", "Globals.GroupNames.Content" },
            context.GroupNames.Select(g => g.Expression));
        Assert.All(context.GroupNames, g => Assert.Equal("Contoso.Web", g.Namespace));
        Assert.Equal(12, context.CmsMajorVersion);
        Assert.Equal(Profiles.Cms12, context.CmsProfile);
        Assert.Null(context.CommerceMajorVersion);
        Assert.True(context.NullableEnabled);

        // The support source uses block-scoped namespaces.
        Assert.False(context.FileScopedNamespaces);
    }

    [Fact]
    public void Detects_a_view_location_expander_and_where_view_models_live()
    {
        string InProject(string relative) => System.IO.Path.Combine(Scenarios.ProjectDirectory, relative);

        var compilation = TestCompiler.Create(
            new[]
            {
                (InProject(@"Business\SiteViewLocationExpander.cs"), """
                    using System.Collections.Generic;
                    using Microsoft.AspNetCore.Mvc.Razor;
                    namespace Contoso.Web.Business;
                    public class SiteViewLocationExpander : IViewLocationExpander
                    {
                        public void PopulateValues(ViewLocationExpanderContext context) { }
                        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations) => viewLocations;
                    }
                    """),
                (InProject(@"Models\ViewModels\LayoutViewModel.cs"), "namespace Contoso.Web.Models.ViewModels;\npublic class LayoutViewModel { }"),
                (InProject(@"Models\ViewModels\PageViewModel.cs"), "namespace Contoso.Web.Models.ViewModels;\npublic class PageViewModel<T> { }"),
                (InProject(@"Features\Search\SearchViewModel.cs"), "namespace Contoso.Web.Features.Search;\npublic class SearchViewModel { }"),
            },
            nullable: true);
        var context = new ProjectContext(Scenarios.ProjectDirectory, Scenarios.RootNamespace) { ViewModelsFolder = "Other/ViewModels" };

        CompilationAnalyzer.Apply(compilation, context);

        Assert.True(context.HasViewLocationExpander);
        Assert.Equal("Models/ViewModels", context.ViewModelsFolder);
    }

    [Fact]
    public void Detects_file_scoped_namespaces_and_disabled_nullable()
    {
        var compilation = TestCompiler.Create(
            new[]
            {
                ("A.cs", "namespace Contoso.Web.A;\npublic class A { }"),
                ("B.cs", "namespace Contoso.Web.B;\npublic class B { }"),
                ("C.cs", "namespace Contoso.Web.C { public class C { } }"),
            },
            nullable: false);
        var context = new ProjectContext(Scenarios.ProjectDirectory, Scenarios.RootNamespace);

        CompilationAnalyzer.Apply(compilation, context);

        Assert.True(context.FileScopedNamespaces);
        Assert.False(context.NullableEnabled);
    }
}
