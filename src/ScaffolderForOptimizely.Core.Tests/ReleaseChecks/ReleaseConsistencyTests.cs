using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests.ReleaseChecks;

/// <summary>
/// The two extensions are released together, and the version lives in several files. These tests
/// fail before a release goes out with mismatched numbers, a dependency range that excludes the
/// main extension, or no changelog entry.
/// </summary>
public class ReleaseConsistencyTests
{
    private static readonly XNamespace Vsx = "http://schemas.microsoft.com/developer/vsx-schema/2011";

    private static readonly string Root = FindRepositoryRoot();

    private static readonly string CurrentVersion =
        XDocument.Load(Path.Combine(Root, "src", "Directory.Build.props")).Descendants("Version").Single().Value;

    public static TheoryData<string> VsixProjects => new() { "ScaffolderForOptimizely.Vsix", "ScaffolderForOptimizely.Commerce.Vsix" };

    [Theory]
    [MemberData(nameof(VsixProjects))]
    public void Every_version_number_matches_Directory_Build_props(string project)
    {
        var directory = Path.Combine(Root, "src", project);

        Assert.Equal(CurrentVersion, Identity(project).Attribute("Version")!.Value);
        Assert.Contains($"public const string Version = \"{CurrentVersion}\";", File.ReadAllText(Path.Combine(directory, "Extension", "source.extension.cs")));

        var assemblyInfo = File.ReadAllText(Path.Combine(directory, "Properties", "AssemblyInfo.cs"));
        Assert.Contains($"[assembly: AssemblyVersion(\"{CurrentVersion}.0\")]", assemblyInfo);
        Assert.Contains($"[assembly: AssemblyFileVersion(\"{CurrentVersion}.0\")]", assemblyInfo);
    }

    [Theory]
    [MemberData(nameof(VsixProjects))]
    public void Manifest_ids_match_the_code(string project)
    {
        var id = Identity(project).Attribute("Id")!.Value;
        var code = File.ReadAllText(Path.Combine(Root, "src", project, "Extension", "source.extension.cs"));

        Assert.Contains($"public const string Id = \"{id}\";", code);
    }

    [Fact]
    public void The_add_on_depends_on_this_version_of_the_main_extension()
    {
        var mainId = Identity("ScaffolderForOptimizely.Vsix").Attribute("Id")!.Value;
        var dependency = Manifest("ScaffolderForOptimizely.Commerce.Vsix")
            .Descendants(Vsx + "Dependency")
            .Single(d => d.Attribute("Id")!.Value == mainId);

        // e.g. [0.9.0,0.10.0): the add-on uses the main extension's internals, so only its own minor version.
        var range = Regex.Match(dependency.Attribute("Version")!.Value, @"^\[(?<min>[\d.]+),(?<max>[\d.]+)\)$");
        Assert.True(range.Success, $"Unexpected dependency range '{dependency.Attribute("Version")!.Value}'.");

        var version = new Version(CurrentVersion);
        Assert.Equal(version, new Version(range.Groups["min"].Value));
        Assert.Equal(new Version(version.Major, version.Minor + 1, 0), new Version(range.Groups["max"].Value));
    }

    [Fact]
    public void Both_extensions_share_preview_status_and_publisher()
    {
        string Preview(string project) => Manifest(project).Descendants(Vsx + "Preview").SingleOrDefault()?.Value ?? "false";

        Assert.Equal(Preview("ScaffolderForOptimizely.Vsix"), Preview("ScaffolderForOptimizely.Commerce.Vsix"));
        Assert.Equal(
            Identity("ScaffolderForOptimizely.Vsix").Attribute("Publisher")!.Value,
            Identity("ScaffolderForOptimizely.Commerce.Vsix").Attribute("Publisher")!.Value);

        var publishers = Directory.GetFiles(Path.Combine(Root, "marketplace"), "*.json")
            .Select(f => (string)JObject.Parse(File.ReadAllText(f))["publisher"]!)
            .Distinct()
            .ToList();
        Assert.Single(publishers);
    }

    [Fact]
    public void The_changelog_has_an_entry_for_this_version()
    {
        var changelog = File.ReadAllText(Path.Combine(Root, "CHANGELOG.md"));

        Assert.Matches(new Regex($@"(?m)^## \[{Regex.Escape(CurrentVersion)}\]"), changelog);
    }

    /// <summary>
    /// The Marketplace publisher's own validation (VsixPublisher), which otherwise only shows up
    /// when a release is already being published. Some of it runs after sign-in, so a local run
    /// with a dummy token doesn't catch it.
    /// </summary>
    [Theory]
    [MemberData(nameof(VsixProjects))]
    public void Manifests_pass_the_Marketplace_publishers_rules(string project)
    {
        var manifest = Manifest(project);
        string Value(string element) => manifest.Descendants(Vsx + element).Single().Value;

        var description = Value("Description");
        Assert.True(description.Length is > 0 and < 280, $"Description is {description.Length} characters; the Marketplace needs 1 to 279.");

        var displayName = Value("DisplayName");
        Assert.True(displayName.Length is > 0 and < 100, $"Display name is {displayName.Length} characters.");

        // Separated by semicolons: the publisher doesn't split on commas, so "a, b" is one long tag.
        foreach (var tag in Value("Tags").Split(';'))
        {
            Assert.DoesNotContain(",", tag);
            Assert.InRange(tag.Trim().Length, 1, 50);
        }

        Assert.False(string.IsNullOrEmpty(Identity(project).Attribute("Language")?.Value));
    }

    [Fact]
    public void Marketplace_internal_names_are_valid_and_unique()
    {
        var names = Directory.GetFiles(Path.Combine(Root, "marketplace"), "*.json")
            .Select(f => (string)JObject.Parse(File.ReadAllText(f))["identity"]!["internalName"]!)
            .ToList();

        Assert.All(names, name => Assert.Matches("^[A-Za-z0-9-]{1,62}$", name));
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    private static XDocument Manifest(string project) =>
        XDocument.Load(Path.Combine(Root, "src", project, "source.extension.vsixmanifest"));

    private static XElement Identity(string project) => Manifest(project).Descendants(Vsx + "Identity").Single();

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "Directory.Build.props"))
                && Directory.Exists(Path.Combine(directory.FullName, "marketplace")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Couldn't find the repository root above " + AppContext.BaseDirectory);
    }
}
