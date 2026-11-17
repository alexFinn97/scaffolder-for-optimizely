using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using ScaffolderForOptimizely.Core.Providers;

namespace ScaffolderForOptimizely.Services;

/// <summary>
/// Offers to install Optimizely's EPiServer.Templates pack when the config wants
/// <c>dotnet-new</c> templates but the pack isn't there.
/// </summary>
internal static class TemplatePackInstaller
{
    private const string InstallAction = "install";
    private static bool _offered;

    /// <summary>Shows the offer once per Visual Studio session.</summary>
    public static async Task OfferAsync()
    {
        if (_offered)
        {
            return;
        }

        _offered = true;
        var model = new InfoBarModel(
            new IVsInfoBarTextSpan[]
            {
                new InfoBarTextSpan($"Optimizely's '{DotnetNewTemplateProvider.PackageId}' templates aren't installed, so the built-in templates were used. "),
                new InfoBarHyperlink("Install them", InstallAction),
            },
            KnownMonikers.StatusInformation,
            isCloseButtonVisible: true);

        var infoBar = await VS.InfoBar.CreateAsync(ToolWindowGuids80.SolutionExplorer, model);
        if (infoBar is null)
        {
            return;
        }

        infoBar.ActionItemClicked += (_, e) =>
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (e.ActionItem.ActionContext as string == InstallAction)
            {
                e.InfoBarUIElement.Close();
                InstallAsync().FireAndForget();
            }
        };

        await infoBar.TryShowInfoBarUIAsync();
    }

    private static async Task InstallAsync()
    {
        var log = OutputLog.Instance;
        await log.ShowAsync();
        await log.WriteAsync($"Running: dotnet {DotnetNewTemplateProvider.InstallArguments}");
        await VS.StatusBar.ShowMessageAsync($"Installing {DotnetNewTemplateProvider.PackageId}...");

        await TaskScheduler.Default;
        string message;
        try
        {
            var result = await ProcessRunner.Instance.RunAsync("dotnet", DotnetNewTemplateProvider.InstallArguments,
                Path.GetTempPath(), TimeSpan.FromMinutes(3), CancellationToken.None);
            DotnetNewTemplateProvider.ResetInstalledCache();
            message = result.ExitCode == 0
                ? $"{DotnetNewTemplateProvider.PackageId} installed. New items will use Optimizely's templates where available."
                : $"Installing {DotnetNewTemplateProvider.PackageId} failed (exit code {result.ExitCode}).";
            await log.WriteAsync(result.Output.Trim());
        }
        catch (Exception ex)
        {
            message = $"Installing {DotnetNewTemplateProvider.PackageId} failed: {ex.Message}";
        }

        await log.WriteAsync(message);
        await VS.StatusBar.ShowMessageAsync(message);
    }
}
