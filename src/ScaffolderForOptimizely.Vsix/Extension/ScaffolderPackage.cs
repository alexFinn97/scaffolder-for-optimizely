using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ScaffolderForOptimizely.Extension;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.ScaffolderPackageString)]
// Load in the background once a solution is open, so the hidden-by-default commands can decide
// whether to show for the selected project.
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
// Lets add-on extensions (Scaffolder for Optimizely Commerce) load this extension's assemblies
// (the dialog, Core and Scriban) instead of shipping their own copies.
[ProvideBindingPath]
public sealed class ScaffolderPackage : ToolkitPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();
    }
}
