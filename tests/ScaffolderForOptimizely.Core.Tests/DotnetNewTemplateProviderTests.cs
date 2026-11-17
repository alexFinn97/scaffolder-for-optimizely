using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Generation;
using ScaffolderForOptimizely.Core.Providers;
using ScaffolderForOptimizely.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests;

/// <summary>Runs one test at a time: the "is the pack installed" answer is cached process-wide.</summary>
[Collection(nameof(DotnetNewTemplateProviderTests))]
public class DotnetNewTemplateProviderTests
{
    [Fact]
    public async Task Uses_dotnet_new_output_and_adds_usings()
    {
        var runner = new FakeRunner(installed: true);
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-defaults"), TemplateSources.DotnetNew);

        var result = await Generate(runner, request);

        var model = result.Files.Single(f => f.OutputId == "model");
        var controller = result.Files.Single(f => f.OutputId == "controller");
        Assert.Equal(TemplateSources.DotnetNew, model.Source);
        Assert.Equal(TemplateSources.DotnetNew, controller.Source);
        Assert.Contains("using Contoso.Web.Models.Pages;", controller.Content);
        Assert.Contains(runner.Calls, c => c.StartsWith("new epi-cms-contenttype -n ArticlePage ", StringComparison.Ordinal) && c.Contains("--basetype PageData"));
        Assert.Contains(runner.Calls, c => c.StartsWith("new epi-cms-pagecontroller -n ArticlePageController ", StringComparison.Ordinal) && c.Contains("--contentType ArticlePage"));

        // Optimizely has no template for views, so the view still comes from the embedded set.
        Assert.Equal(TemplateSources.Embedded, result.Files.Single(f => f.OutputId == "view").Source);
    }

    [Fact]
    public async Task Falls_back_to_embedded_when_the_pack_is_missing()
    {
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-defaults"), TemplateSources.DotnetNew);

        var result = await Generate(new FakeRunner(installed: false), request);

        Assert.True(result.DotnetTemplatePackMissing);
        Assert.All(result.Files, f => Assert.Equal(TemplateSources.Embedded, f.Source));
    }

    [Fact]
    public async Task Skips_dotnet_new_for_a_controller_that_needs_a_view_model()
    {
        var runner = new FakeRunner(installed: true);
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-view-model"), TemplateSources.DotnetNew);

        var result = await Generate(runner, request);

        Assert.Equal(TemplateSources.Embedded, result.Files.Single(f => f.OutputId == "controller").Source);
        Assert.DoesNotContain(runner.Calls, c => c.Contains("epi-cms-pagecontroller"));
    }

    [Fact]
    public async Task Uses_the_built_in_content_type_when_a_display_name_or_group_is_set()
    {
        var runner = new FakeRunner(installed: true);
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-site-base-and-group"), TemplateSources.DotnetNew);

        var result = await Generate(runner, request);

        var model = result.Files.Single(f => f.OutputId == "model");
        Assert.Equal(TemplateSources.Embedded, model.Source);
        Assert.Contains("GroupName = Globals.GroupNames.Specialized", model.Content);
        Assert.DoesNotContain(runner.Calls, c => c.Contains("epi-cms-contenttype"));
    }

    [Fact]
    public async Task Never_runs_dotnet_new_for_the_preview()
    {
        var runner = new FakeRunner(installed: true);
        var request = Scenarios.CreateRequest(Scenarios.Get("page-type-defaults"), TemplateSources.DotnetNew);
        request.PreviewOnly = true;

        var result = await Generate(runner, request);

        Assert.Empty(runner.Calls);
        Assert.All(result.Files, f => Assert.Equal(TemplateSources.Embedded, f.Source));
    }

    private static Task<GenerationResult> Generate(FakeRunner runner, GenerationRequest request)
    {
        DotnetNewTemplateProvider.ResetInstalledCache();
        return new ScaffoldGenerator(runner).GenerateAsync(request);
    }

    /// <summary>Pretends to be the dotnet CLI: writes a file like Optimizely's template would.</summary>
    private sealed class FakeRunner : IProcessRunner
    {
        private readonly bool _installed;

        public FakeRunner(bool installed) => _installed = installed;

        public List<string> Calls { get; } = new();

        public Task<ProcessResult> RunAsync(string fileName, string arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (arguments.StartsWith("new list", StringComparison.Ordinal))
            {
                return Task.FromResult(_installed
                    ? new ProcessResult(0, "epi-cms-contenttype  [C#]")
                    : new ProcessResult(103, "No templates found"));
            }

            Calls.Add(arguments);
            var name = Regex.Match(arguments, @"-n (\S+)").Groups[1].Value;
            var ns = Regex.Match(arguments, @"--namespace (\S+)").Groups[1].Value;
            var output = Regex.Match(arguments, "-o \"([^\"]+)\"").Groups[1].Value;
            File.WriteAllText(Path.Combine(output, name + ".cs"), $"namespace {ns};\n\npublic class {name}\n{{\n}}\n");
            return Task.FromResult(new ProcessResult(0, "The template was created successfully."));
        }
    }
}
