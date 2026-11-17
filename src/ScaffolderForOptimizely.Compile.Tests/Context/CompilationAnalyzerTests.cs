using System.Linq;
using ScaffolderForOptimizely.Compile.Tests.Support;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Compile.Tests.Context;

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
    public void Detects_where_existing_infrastructure_and_editor_classes_live()
    {
        string InProject(string relative) => System.IO.Path.Combine(Scenarios.ProjectDirectory, relative);

        var compilation = TestCompiler.Create(
            new[]
            {
                (InProject(@"Infrastructure\Jobs\ImportJob.cs"), "namespace Contoso.Web.Infrastructure.Jobs;\npublic class ImportJob : EPiServer.Scheduler.ScheduledJobBase { public override string Execute() => string.Empty; }"),
                (InProject(@"Infrastructure\Jobs\ExportJob.cs"), "namespace Contoso.Web.Infrastructure.Jobs;\npublic class ExportJob : EPiServer.Scheduler.ScheduledJobBase { public override string Execute() => string.Empty; }"),
                (InProject(@"Old\LegacyJob.cs"), "namespace Contoso.Web.Old;\npublic class LegacyJob : EPiServer.Scheduler.ScheduledJobBase { public override string Execute() => string.Empty; }"),
                (InProject(@"Startup\Modules\SiteInit.cs"), """
                    namespace Contoso.Web.Startup.Modules;
                    public class SiteInit : EPiServer.Framework.IInitializableModule
                    {
                        public void Initialize(EPiServer.Framework.Initialization.InitializationEngine context) { }
                        public void Uninitialize(EPiServer.Framework.Initialization.InitializationEngine context) { }
                    }
                    """),
                (InProject(@"Startup\Filters\WarmupFilter.cs"), """
                    using System;
                    using Microsoft.AspNetCore.Builder;
                    using Microsoft.AspNetCore.Hosting;
                    namespace Contoso.Web.Startup.Filters;
                    public class WarmupFilter : IStartupFilter { public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => next; }
                    """),
                (InProject(@"Startup\Services.cs"), """
                    using Microsoft.Extensions.DependencyInjection;
                    namespace Contoso.Web.Startup;
                    public static class Services { public static IServiceCollection AddSite(this IServiceCollection services) => services; }
                    """),
                (InProject(@"Editing\Colours.cs"), """
                    using System.Collections.Generic;
                    using EPiServer.Shell.ObjectEditing;
                    namespace Contoso.Web.Editing;
                    public class Colours : ISelectionFactory { public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata) => new ISelectItem[0]; }
                    """),
                (InProject(@"Rules\PageRules.cs"), """
                    using System.Collections.Generic;
                    using EPiServer.Core;
                    using EPiServer.Validation;
                    namespace Contoso.Web.Rules;
                    public class PageRules : IValidate<PageData> { public IEnumerable<ValidationError> Validate(PageData instance) { yield break; } }
                    """),
            },
            nullable: true);
        var context = new ProjectContext(Scenarios.ProjectDirectory, Scenarios.RootNamespace);

        CompilationAnalyzer.Apply(compilation, context);

        Assert.Equal("Infrastructure/Jobs", context.DetectedFolders[FolderKeys.ScheduledJobs]);
        Assert.Equal("Startup/Modules", context.DetectedFolders[FolderKeys.Initialization]);
        Assert.Equal("Startup/Filters", context.DetectedFolders[FolderKeys.StartupFilters]);
        Assert.Equal("Startup", context.DetectedFolders[FolderKeys.ServiceExtensions]);
        Assert.Equal("Editing", context.DetectedFolders[FolderKeys.SelectionFactories]);
        Assert.Equal("Rules", context.DetectedFolders[FolderKeys.Validators]);
        Assert.False(context.DetectedFolders.ContainsKey(FolderKeys.EditorDescriptors));
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
