using System;
using System.IO;
using ScaffolderForOptimizely.Core.Context;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests;

public sealed class ProjectFileScannerTests : IDisposable
{
    private readonly string _project = Path.Combine(Path.GetTempPath(), "sfo-scan-" + Guid.NewGuid().ToString("N"));

    public ProjectFileScannerTests() => Directory.CreateDirectory(_project);

    public void Dispose() => Directory.Delete(_project, recursive: true);

    [Fact]
    public void Finds_the_shallowest_ViewModels_folder()
    {
        Directory.CreateDirectory(Path.Combine(_project, "Features", "News", "ViewModels"));
        Directory.CreateDirectory(Path.Combine(_project, "Business", "viewmodels"));
        Directory.CreateDirectory(Path.Combine(_project, "Models", "ViewModels"));

        Assert.Equal("Business/viewmodels", ProjectFileScanner.FindViewModelsFolder(_project));
    }

    [Fact]
    public void Ignores_build_output_and_returns_null_when_there_is_none()
    {
        Directory.CreateDirectory(Path.Combine(_project, "obj", "ViewModels"));
        Directory.CreateDirectory(Path.Combine(_project, "Models", "Pages"));

        Assert.Null(ProjectFileScanner.FindViewModelsFolder(_project));
    }
}
