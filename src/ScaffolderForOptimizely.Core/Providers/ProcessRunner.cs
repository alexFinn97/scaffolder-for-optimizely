using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScaffolderForOptimizely.Core.Providers;

public sealed class ProcessResult
{
    public ProcessResult(int exitCode, string output)
    {
        ExitCode = exitCode;
        Output = output;
    }

    public int ExitCode { get; }

    /// <summary>Standard output and error combined.</summary>
    public string Output { get; }
}

/// <summary>Runs external processes; swapped for a fake in tests.</summary>
public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string fileName, string arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken);
}

public sealed class ProcessRunner : IProcessRunner
{
    public static ProcessRunner Instance { get; } = new();

    public async Task<ProcessResult> RunAsync(string fileName, string arguments, string workingDirectory, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        startInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";

        var output = new StringBuilder();
        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var exited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) { lock (output) { output.AppendLine(e.Data); } } };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) { lock (output) { output.AppendLine(e.Data); } } };
        process.Exited += (_, _) => exited.TrySetResult(true);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);
        using (timeoutCts.Token.Register(() => exited.TrySetCanceled()))
        {
            try
            {
                await exited.Task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                try
                {
                    process.Kill();
                }
                catch (InvalidOperationException)
                {
                    // Already exited.
                }

                cancellationToken.ThrowIfCancellationRequested();
                throw new TimeoutException($"'{fileName} {arguments}' did not finish within {timeout.TotalSeconds:0} seconds.");
            }
        }

        // Make sure the async output readers have drained.
        process.WaitForExit();
        lock (output)
        {
            return new ProcessResult(process.ExitCode, output.ToString());
        }
    }
}
