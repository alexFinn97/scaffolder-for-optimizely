using System;
using System.IO;
using System.Linq;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Config;
using ScaffolderForOptimizely.Core.Generation;
using ScaffolderForOptimizely.Extension;
using ScaffolderForOptimizely.Services;
using ScaffolderForOptimizely.UI;
using Task = System.Threading.Tasks.Task;

namespace ScaffolderForOptimizely.Commands;

/// <summary>
/// Shared behaviour for every "Add > Optimizely > ..." command: only visible in Optimizely
/// projects; opens the scaffold dialog for its item and writes the result into the project.
/// </summary>
internal abstract class ScaffoldCommand<T> : BaseCommand<T>
    where T : class, new()
{
    protected abstract string ItemId { get; }

    protected override void BeforeQueryStatus(EventArgs e)
    {
        var visible = ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            var project = await VS.Solutions.GetActiveProjectAsync();
            return project?.FullPath is { } path && OptimizelyProjectDetector.IsOptimizelyProject(path);
        });

        Command.Visible = visible;
        Command.Enabled = visible;
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        var log = OutputLog.Instance;
        try
        {
            await ScaffoldAsync(log);
        }
        catch (Exception ex)
        {
            await log.WriteAsync($"Scaffolding failed: {ex}");
            await VS.MessageBox.ShowErrorAsync(Vsix.Name, ex.Message);
        }
    }

    private async Task ScaffoldAsync(OutputLog log)
    {
        var selected = await VS.Solutions.GetActiveItemAsync();
        var project = selected as Project ?? selected?.FindParent(SolutionItemType.Project) as Project;
        if (project?.FullPath is null)
        {
            return;
        }

        var projectDirectory = Path.GetDirectoryName(project.FullPath)!;
        var targetDirectory = selected is PhysicalFolder folder && folder.FullPath is not null ? folder.FullPath : projectDirectory;

        var solution = await VS.Solutions.GetCurrentSolutionAsync();
        var solutionDirectory = solution?.FullPath is null ? null : Path.GetDirectoryName(solution.FullPath);

        await VS.StatusBar.ShowMessageAsync($"Analysing {project.Name}...");
        var context = await ProjectContextService.BuildAsync(project, log, CancellationToken.None);
        var config = ConfigLoader.Load(projectDirectory, solutionDirectory);
        await VS.StatusBar.ClearAsync();

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var viewModel = new ScaffoldDialogViewModel(ItemCatalog.Get(ItemId), context, config, targetDirectory);
        var dialog = new ScaffoldDialog(viewModel);
        if (dialog.ShowModal() != true)
        {
            return;
        }

        await VS.StatusBar.ShowMessageAsync($"Creating {viewModel.Item.DisplayName}...");
        var result = await new ScaffoldGenerator().GenerateAsync(viewModel.CreateRequest(previewOnly: false));
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        foreach (var warning in result.Warnings)
        {
            await log.WriteAsync(warning);
        }

        var written = await ProjectWriter.WriteAsync(project, result, log);

        var summary = $"Created {written.Written.Count} file(s)" + (written.Skipped.Count > 0 ? $", skipped {written.Skipped.Count} that already existed." : ".");
        await VS.StatusBar.ShowMessageAsync(summary);

        if (result.DotnetTemplatePackMissing)
        {
            await TemplatePackInstaller.OfferAsync();
        }

        if (written.Skipped.Count > 0 || result.Warnings.Any())
        {
            await log.ShowAsync();
        }
    }
}
