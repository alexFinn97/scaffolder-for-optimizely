using System.Linq;
using System.Threading.Tasks;
using ScaffolderForOptimizely.Tests.Shared;
using Xunit;

namespace ScaffolderForOptimizely.Compile.Tests;

/// <summary>
/// Every snapshot scenario's C# must compile against EPiServer.CMS 12 with no errors or warnings.
/// </summary>
public class GeneratedCodeCompilesTests
{
    [Theory]
    [MemberData(nameof(Scenarios.Names), MemberType = typeof(Scenarios))]
    public async Task Compiles(string name)
    {
        var scenario = Scenarios.Get(name);
        var result = await Scenarios.GenerateAsync(scenario);

        var files = result.Files
            .Where(f => f.RelativePath.EndsWith(".cs"))
            .Select(f => (f.RelativePath, f.Content))
            .Append(("Support.cs", Scenarios.SupportSource));

        var problems = TestCompiler.Problems(TestCompiler.Create(files, scenario.Nullable));

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }
}
