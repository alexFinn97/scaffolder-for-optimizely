# Contributing

Issues and pull requests are welcome. Generated code that doesn't compile, or doesn't match how Optimizely sites are usually built, is the most useful kind of bug report. Include the item, the options you picked, and what you expected.

[TODO.md](TODO.md) lists what still needs checking or sorting out. It's a good place to start if you'd like to help.

## Getting set up

- Visual Studio 2022 17.x or 2026, with the **Visual Studio extension development** workload (needed to load and debug the VSIX projects).
- The .NET 8 SDK.

Open `src/ScaffolderForOptimizely.sln`. To try a change, set **ScaffolderForOptimizely.Commerce.Vsix** as the startup project and press F5. It builds and deploys both extensions to an experimental instance of Visual Studio. See the [README](README.md#building) for command-line builds.

## Tests

```bash
dotnet test src/ScaffolderForOptimizely.Core.Tests
dotnet test src/ScaffolderForOptimizely.Compile.Tests
```

- **Snapshots:** every scenario in [`Scenarios.cs`](src/ScaffolderForOptimizely.Core.Tests/Shared/Scenarios.cs) has a [Verify](https://github.com/VerifyTests/Verify) snapshot in `Generation/Snapshots`. After an intended change, review each `.received.txt` and rename it to `.verified.txt`.
- **Compile tests:** every scenario's generated C# is compiled against the real Optimizely CMS 12 and Commerce 14 packages. Errors and warnings both fail.
- **Release tests:** check the version numbers, manifests and changelog agree.

Please add or update scenarios with any template change. The compile tests are what keep the generated code honest.

## Adding an item

1. **Catalogue:** add an id and an `ItemDefinition` in `src/ScaffolderForOptimizely.Core/Catalog/` (`ItemCatalog.cs`, `ItemCatalog.Infrastructure.cs` or `ItemCatalog.Commerce.cs`), and list it in `ItemCatalog.Build()`. The dialog is built from the item's options, so there's no UI code to write.
2. **Templates:** add `.sbn` files under `src/ScaffolderForOptimizely.Core/Templates/cms12/`. They're embedded automatically and use [Scriban](https://github.com/scriban/scriban). Look at a similar template for the model it receives (`class_name`, `namespace`, `options.*`, `class_<output id>`...).
3. **Menu:** add a button and an `IDSymbol` to the `VSCommandTable.vsct` of the right extension (CMS or Commerce), the matching constant in `Extension/VSCommandTable.cs`, and a command class in `Commands/`.
4. **Folder (if it has its own):** add a key to `FolderKeys`, a property to `FolderConfig` (and `ForKey`), a detection rule in `CompilationAnalyzer.FolderRules`, and an entry in [the schema](schema/optimizely-scaffolder.schema.json).
5. **Scenarios:** add one or more to `Scenarios.cs`, run the tests, review and approve the snapshots.
6. **Docs:** add it to the README, the matching `marketplace/*.md` overview and `CHANGELOG.md`.

## Pull requests

- Branch from `develop`, and keep each pull request to one change.
- Match the existing style: `.editorconfig` is the reference, with file-scoped namespaces and nullable enabled.
- Add a line to the `Unreleased` section of `CHANGELOG.md` for anything users would notice.
- CI's **dependency-review** check fails a pull request that adds or bumps a package with a known moderate, high or critical vulnerability.

## Vulnerable packages

Restore fails with **NU1902**, **NU1903** or **NU1904** when a package, direct or transitive, has a known moderate, high or critical vulnerability. This is set in `src/Directory.Build.props` and applies locally, in CI and to releases. CI also lists every such package in its "Vulnerable package check" step.

To fix one:

1. Find what pulls it in. `dotnet nuget why <project> <package>` works for the SDK projects. For the VSIX projects, look in their `obj/project.assets.json`.
2. Bump the direct package if a newer version in the same major resolves a fixed version.
3. Otherwise, add a direct `PackageReference` pinning the first fixed version. Use the same major where one exists. Put a comment above it naming the advisory and where the package comes from, so the pin can be removed once the parent package catches up. See the pins in `ScaffolderForOptimizely.Compile.Tests.csproj` and the two VSIX projects.
4. In the VSIX projects, use `ExcludeAssets="runtime" PrivateAssets="all"` for anything Visual Studio already loads, so nothing new is shipped. Check the `.vsix` contents haven't changed.

Only use `<NuGetAuditSuppress Include="<advisory URL>" />` if there's no fix. Use it only for packages that don't ship, and give the reason in a comment.

### Dependabot

[`.github/dependabot.yml`](.github/dependabot.yml) opens two grouped pull requests against `develop` every Monday: one for all the NuGet projects and one for GitHub Actions. Security fixes can arrive in between.

All the NuGet projects share one entry on purpose. Dependabot follows project references, so the test projects' reference to `ScaffolderForOptimizely.Core` means it updates Core from there too. With separate entries, the same package was updated in different pull requests to different versions, and restore failed with NU1605 (package downgrade). One entry updates every copy of a package together.

The ignore list is deliberate:

- **Visual Studio set:** `Microsoft.CodeAnalysis.CSharp`, `Microsoft.VisualStudio.LanguageServices`, `Newtonsoft.Json` and `Community.VisualStudio.Toolkit.17` match what the oldest supported Visual Studio loads, so they're never bumped, including the test projects' own copies. Update those by hand.
- **Security pins:** no major versions. `MessagePack` stays on 2.x, which Visual Studio loads. The `System.*` pins in Compile.Tests can't go to 11.0 or later, which don't support net8.0 and stop the test host starting.
- **System.Text.Json** is referenced by both Core (for Scriban) and Compile.Tests (a security pin), and the two must stay on the same version. Core's reference flows into Compile.Tests, so a lower pin fails restore with NU1605. Dependabot keeps them together; keep them together when you change either by hand.
- **`Microsoft.VSSDK.BuildTools`:** no major versions. Take those on deliberately, after installing the built `.vsix` in Visual Studio 2022 and 2026, and checking the VsixPublisher workaround in TODO.md.
- **`EPiServer.*`:** no major versions, so the compile tests stay on CMS 12 and Commerce 14.

A Dependabot pull request only needs a `CHANGELOG.md` line if it changes what ships, for example a Scriban update.

## Releasing

See [docs/publishing.md](docs/publishing.md).
