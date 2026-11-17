using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Services;

/// <summary>Writes generated files, adds them to the project and opens the main one.</summary>
internal static class ProjectWriter
{
    /// <summary>UTF-8 with a byte order mark, matching new files created by Visual Studio.</summary>
    private static readonly Encoding FileEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    /// <summary>Writes every file that doesn't exist yet. Existing files are never overwritten.</summary>
    public static async Task<WriteResult> WriteAsync(Project project, GenerationResult result, OutputLog log)
    {
        var written = new List<GeneratedFile>();
        var skipped = new List<GeneratedFile>();

        foreach (var file in result.Files)
        {
            if (File.Exists(file.FullPath))
            {
                skipped.Add(file);
                await log.WriteAsync($"Skipped {file.RelativePath}: it already exists.");
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file.FullPath)!);
            File.WriteAllText(file.FullPath, file.Content, FileEncoding);
            written.Add(file);
            await log.WriteAsync($"Created {file.RelativePath} ({file.Source} template).");
        }

        if (written.Count > 0)
        {
            await project.AddExistingFilesAsync(written.Select(f => f.FullPath).ToArray());
        }

        var primary = written.FirstOrDefault(f => f.IsPrimary) ?? written.FirstOrDefault();
        if (primary is not null)
        {
            await VS.Documents.OpenAsync(primary.FullPath);
        }

        return new WriteResult(written, skipped);
    }
}

internal sealed class WriteResult
{
    public WriteResult(IReadOnlyList<GeneratedFile> written, IReadOnlyList<GeneratedFile> skipped)
    {
        Written = written;
        Skipped = skipped;
    }

    public IReadOnlyList<GeneratedFile> Written { get; }

    public IReadOnlyList<GeneratedFile> Skipped { get; }
}
