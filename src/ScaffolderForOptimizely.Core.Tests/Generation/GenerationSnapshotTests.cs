using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Core.Generation;
using ScaffolderForOptimizely.Core.Tests.Shared;
using VerifyXunit;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests.Generation;

public class GenerationSnapshotTests
{
    [Theory]
    [MemberData(nameof(Scenarios.Names), MemberType = typeof(Scenarios))]
    public async Task Generates(string scenario)
    {
        var result = await Scenarios.GenerateAsync(Scenarios.Get(scenario));

        Assert.Empty(result.Warnings);
        await Verifier.Verify(Format(result))
            .UseDirectory("Snapshots")
            .UseParameters(scenario);
    }

    /// <summary>All files of a run in one snapshot, each headed by its path and source.</summary>
    internal static string Format(GenerationResult result)
    {
        var sb = new StringBuilder();
        foreach (var file in result.Files.OrderBy(f => f.RelativePath, StringComparer.Ordinal))
        {
            sb.Append("=== ").Append(file.RelativePath)
                .Append(" (").Append(file.Source).Append(file.IsPrimary ? ", primary" : string.Empty).Append(')')
                .Append('\n')
                .Append(file.Content.Replace("\r\n", "\n"))
                .Append('\n');
        }

        return sb.ToString();
    }
}
