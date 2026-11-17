using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ScaffolderForOptimizely.Commerce.Extension;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(CommerceVsix.Name, CommerceVsix.Description, CommerceVsix.Version)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(CommercePackageGuids.CommercePackageString)]
// Same as the main extension: load once a solution is open so the commands can check the project.
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class CommerceScaffolderPackage : ToolkitPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();
    }
}
