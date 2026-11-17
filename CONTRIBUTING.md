# Contributing

Issues and pull requests are welcome. Generated code that doesn't compile, or doesn't match how Optimizely sites are usually built, is the most useful kind of bug report. Include the item, the options you picked, and what you expected.

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

## Releasing

See [docs/publishing.md](docs/publishing.md).
