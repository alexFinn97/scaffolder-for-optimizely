using System;
using ScaffolderForOptimizely.Core.Generation;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests;

public class CSharpPostProcessorTests
{
    private const string Source = "using EPiServer.Core;\nusing System;\n\nnamespace Contoso.Web;\n\npublic class A\n{\n    public int B { get; set; }\n}\n";

    [Fact]
    public void Adds_missing_usings_sorted_with_System_first()
    {
        var result = CSharpPostProcessor.Process(Source, new[] { "Contoso.Web.Models", "EPiServer.Core", "" }, fileScopedNamespaces: true, newLine: "\n");

        Assert.StartsWith("using System;\nusing Contoso.Web.Models;\nusing EPiServer.Core;\n\nnamespace Contoso.Web;", result);
    }

    [Fact]
    public void Converts_to_block_scoped_namespace()
    {
        var result = CSharpPostProcessor.Process(Source, Array.Empty<string>(), fileScopedNamespaces: false, newLine: "\n");

        Assert.Equal(
            "using EPiServer.Core;\nusing System;\n\nnamespace Contoso.Web\n{\n    public class A\n    {\n        public int B { get; set; }\n    }\n}\n",
            result);
    }

    [Fact]
    public void Adds_usings_when_there_are_none()
    {
        var result = CSharpPostProcessor.Process("namespace X;\n\npublic class A { }\n", new[] { "Y" }, true, "\n");

        Assert.Equal("using Y;\n\nnamespace X;\n\npublic class A { }\n", result);
    }
}
