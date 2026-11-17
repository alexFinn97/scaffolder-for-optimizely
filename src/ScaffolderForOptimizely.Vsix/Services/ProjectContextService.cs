using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Threading;
using ScaffolderForOptimizely.Core.Context;

namespace ScaffolderForOptimizely.Services;

/// <summary>Builds a <see cref="ProjectContext"/> from the project file and Roslyn's view of the project.</summary>
internal static class ProjectContextService
{
    public static async Task<ProjectContext> BuildAsync(Project project, OutputLog log, CancellationToken cancellationToken)
    {
        var projectPath = project.FullPath!;
        var context = ProjectFileScanner.Scan(projectPath);

        var rootNamespace = await project.GetAttributeAsync("RootNamespace");
        if (!string.IsNullOrWhiteSpace(rootNamespace))
        {
            context.RootNamespace = rootNamespace!;
        }

        try
        {
            var workspace = await VS.GetMefServiceAsync<VisualStudioWorkspace>();
            await TaskScheduler.Default;

            var roslynProject = workspace.CurrentSolution.Projects
                .FirstOrDefault(p => string.Equals(p.FilePath, projectPath, StringComparison.OrdinalIgnoreCase));
            var compilation = roslynProject is null ? null : await roslynProject.GetCompilationAsync(cancellationToken);

            if (compilation is null)
            {
                await log.WriteAsync($"Couldn't load a compilation for {Path.GetFileName(projectPath)}; using defaults for base classes and group names.");
            }
            else
            {
                CompilationAnalyzer.Apply(compilation, context);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Detection only fills in defaults; scaffolding still works without it.
            await log.WriteAsync($"Project analysis failed: {ex.Message}");
        }

        // Needs both the views (file scan) and the content types (Roslyn).
        ViewConventionDetector.Apply(context);

        if (context.CmsMajorVersion is > 12)
        {
            await log.WriteAsync($"CMS {context.CmsMajorVersion} detected. Version-specific templates aren't available yet, so the CMS 12 templates are used.");
        }

        return context;
    }
}
