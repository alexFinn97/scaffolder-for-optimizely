using System;
using System.IO;
using ScaffolderForOptimizely.Core.Config;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests.Config;

public sealed class ConfigLoaderTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "sfo-config-" + Guid.NewGuid().ToString("N"));

    public ConfigLoaderTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src", "Web"));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Finds_config_in_a_parent_folder()
    {
        File.WriteAllText(Path.Combine(_root, ScaffolderConfig.FileName),
            """{ "templateSource": "dotnet-new", "layout": "feature", "folders": { "pages": "Features/Pages" }, "companions": { "view": false } }""");

        var loaded = ConfigLoader.Load(Path.Combine(_root, "src", "Web"), _root);

        Assert.Equal(Path.Combine(_root, ScaffolderConfig.FileName), loaded.FilePath);
        Assert.Equal(_root, loaded.RootDirectory);
        Assert.Equal(TemplateSources.DotnetNew, loaded.Config.TemplateSource);
        Assert.Equal(Layouts.Feature, loaded.Config.Layout);
        Assert.Equal("Features/Pages", loaded.Config.Folders.Pages);
        Assert.Equal("Controllers", loaded.Config.Folders.Controllers);
        Assert.False(loaded.Config.Companions.View);
    }

    [Fact]
    public void Uses_defaults_when_there_is_no_config()
    {
        var loaded = ConfigLoader.Load(Path.Combine(_root, "src", "Web"), _root);

        Assert.Null(loaded.FilePath);
        Assert.Equal(TemplateSources.Embedded, loaded.Config.TemplateSource);
        Assert.Equal(_root, loaded.RootDirectory);
    }

    [Fact]
    public void Reports_invalid_json_and_falls_back_to_defaults()
    {
        File.WriteAllText(Path.Combine(_root, "src", ScaffolderConfig.FileName), "{ not json");

        var loaded = ConfigLoader.Load(Path.Combine(_root, "src", "Web"), _root);

        Assert.NotNull(loaded.Error);
        Assert.Equal(TemplateSources.Embedded, loaded.Config.TemplateSource);
    }
}
