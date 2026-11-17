using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;

namespace ScaffolderForOptimizely.Services;

/// <summary>Writes to a "Scaffolder for Optimizely" pane in the Output window.</summary>
internal sealed class OutputLog
{
    private OutputWindowPane? _pane;

    public static OutputLog Instance { get; } = new();

    public async Task WriteAsync(string message)
    {
        _pane ??= await VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name);
        if (_pane is not null)
        {
            await _pane.WriteLineAsync(message);
        }
    }

    public async Task ShowAsync()
    {
        _pane ??= await VS.Windows.CreateOutputWindowPaneAsync(Vsix.Name);
        if (_pane is not null)
        {
            await _pane.ActivateAsync();
        }
    }
}
