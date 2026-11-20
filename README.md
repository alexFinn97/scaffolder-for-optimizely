# Scaffolder for Optimizely

<img src="branding/logo-cms.svg" width="64" height="64" alt="Scaffolder for Optimizely logo"> <img src="branding/logo-commerce.svg" width="64" height="64" alt="Scaffolder for Optimizely Commerce logo">

A Visual Studio 2022/2026 extension that brings back right-click scaffolding for Optimizely CMS 12 and Commerce 14: content types, controllers and components, infrastructure such as initialization modules and scheduled jobs, editor extensions, and Commerce catalog, promotion, payment, shipping and order items, with defaults taken from your project.

> **Status: preview (0.9).** Everything below works and is compile-tested, but few teams have used it yet. Please [report](https://github.com/alexFinn97/scaffolder-for-optimizely/issues) anything that generates the wrong code. 1.0 follows once it has had wider use.

## Install

From the Visual Studio Marketplace, or **Extensions > Manage Extensions** in Visual Studio:

- **Scaffolder for Optimizely**: the CMS items.
- **Scaffolder for Optimizely Commerce**: the Commerce items. It's an add-on, so install Scaffolder for Optimizely first.

Each [GitHub release](https://github.com/alexFinn97/scaffolder-for-optimizely/releases) also has both `.vsix` files; double-click one to install it. See the [changelog](CHANGELOG.md) for what's new.

## What it does

In Solution Explorer, right-click a project or folder and choose **Add > Optimizely CMS**:

| Content types | Rendering |
|---|---|
| Page Type (+ controller, view model, Razor view) | Page Controller |
| Block Type (+ block component, view model, Razor view) | Block Component (`AsyncBlockComponent<T>`, `BlockComponent<T>` or plain `ViewComponent`) |
| Media Type (image, video or generic file) | Partial Content Component |
| | View Component |

| Infrastructure | Editor |
|---|---|
| Initialization Module (plain or `IConfigurableModule`) | Selection Factory (`ISelectionFactory` or autosuggest `ISelectionQuery`) |
| Startup Filter (`IStartupFilter`, optional middleware, + `services.AddX()` registration) | Editor Descriptor (UI hint, optional Dojo editor) |
| Scheduled Job (stoppable, progress, `ILogger`, restartable) | Visitor Group Criterion (+ settings model) |
| Service Collection Extension (`services.AddX()`) | Content Events Handler (DI-registered handler + initialization module that subscribes it) |
| | Content Validator (`IValidate<T>`) |

### Optimizely Commerce add-on

Commerce items come in a separate extension, **Scaffolder for Optimizely Commerce**, so CMS-only developers don't get a Commerce menu. It needs Scaffolder for Optimizely installed first (same version line), and uses its dialog, detection and templates. In projects that reference Optimizely Commerce, **Add > Optimizely Commerce** adds:

| Catalog | Marketing and orders |
|---|---|
| Product, Variation, Bundle, Package, Category (`[CatalogContentType]`, + controller, view model, Razor view, each with an editable path) | Promotion (entry, order or shipping, + its promotion processor) |
| Content Controller (`ContentController<T>` for an existing Commerce content type) | Payment Provider (`AbstractPaymentGateway` + `IPaymentPlugin`, + optional checkout `IPaymentMethod`) |
| | Shipping Provider (`IShippingPlugin` + `IShippingGateway`) |
| | Order Service Decorator (wraps `ILineItemValidator`, `IPlacedPriceProcessor`, `ITaxCalculator` or `IShippingCalculator`, + `services.AddX()` registration) |

Each command opens a dialog with the item's options and a live preview of every file it will create. The menus only appear in projects that reference Optimizely CMS (and Commerce, for the Commerce menu).

Plain item templates for the core items are also available under **Add > New Item > Optimizely**, for people who prefer that route. They don't have the project-aware options.

### Project-aware defaults

When the dialog opens, the extension reads the project with Roslyn and offers:

- abstract base classes you already have (e.g. `SitePageData`) as the default base class
- `GroupNames`-style constants (or classes marked `[GroupDefinitions]`) as content type groups
- existing page, block and catalog types for controllers and components
- abstract catalog base classes per kind (e.g. a `BaseProduct : ProductContent` for products)
- where existing Commerce controllers live, if they're kept apart from the CMS ones
- your namespace style (file-scoped or block-scoped) and nullable setting
- feature-folder vs `Views/` layout, based on where existing views live
- view path patterns learnt from existing views' `@model` (e.g. `Views/Shared/Blocks/{name}.cshtml`)
- the folder holding most existing `*ViewModel` classes (or a folder named `ViewModels`)
- whether a custom `IViewLocationExpander` exists, which decides whether to pass the view path to `View()`
- where your existing initialization modules, startup filters, scheduled jobs, selection factories, editor descriptors, criteria, content event handlers, validators, promotions, payment and shipping providers and order services live

Content type pickers start blank. The view path and view model path fill in from the above, and stay editable. Right-clicking a folder puts the main file there. Right-clicking the project uses the configured or detected folder, else a conventional one (e.g. `Models/Pages`, `Business/ScheduledJobs`). Existing files are never overwritten.

## Settings: `.optimizely-scaffolder.json`

Put this file next to your solution (or in any folder above the project) to pin conventions for the whole team. Every value is optional.

```json
{
  "$schema": "https://raw.githubusercontent.com/alexFinn97/scaffolder-for-optimizely/master/schema/optimizely-scaffolder.schema.json",
  "templateSource": "embedded",
  "layout": "conventional",
  "folders": { "pages": "Models/Pages", "blocks": "Models/Blocks", "controllers": "Controllers" },
  "baseClasses": { "page": "MySite.Models.Pages.SitePageData" },
  "contentTypeGroup": "Globals.GroupNames.Content",
  "blockRenderStyle": "async",
  "viewPaths": { "page": "Views/{name}/Index.cshtml", "block": "Views/Shared/Blocks/{name}.cshtml" },
  "passViewPath": false,
  "companions": { "controller": true, "viewModel": false, "view": true }
}
```

See [schema/optimizely-scaffolder.schema.json](schema/optimizely-scaffolder.schema.json) for every setting.

## Where the code comes from

For each file, the first source that can produce it wins:

1. **Your overrides:** `.optimizely/templates/cms12/<Template>.sbn` next to the settings file (or the solution). Copy any file from [src/ScaffolderForOptimizely.Core/Templates/cms12](src/ScaffolderForOptimizely.Core/Templates/cms12) and edit it. Templates use [Scriban](https://github.com/scriban/scriban).
2. **Built-in templates** (`templateSource: "embedded"`, the default): every item, compile-tested against CMS 12 and Commerce 14. What the dialog previews is exactly what gets written.
3. **Optimizely's own templates**, only with `templateSource: "dotnet-new"`: content types, page controllers, partial components and initialization modules use `dotnet new` from `EPiServer.Templates` when the dialog's options allow it, and the built-in templates otherwise. If the pack isn't installed, the extension offers to install it. Note that `EPiServer.Templates` 2.x targets CMS 13, and the preview still shows the built-in templates.

## Building

Requirements: Visual Studio 2022 17.x or 2026 with the **Visual Studio extension development** workload (needed for F5 debugging; command-line builds work without it), and the .NET 8 SDK.

```bash
dotnet test src/ScaffolderForOptimizely.Core.Tests
dotnet test src/ScaffolderForOptimizely.Compile.Tests
msbuild src/ScaffolderForOptimizely.Commerce.Vsix/ScaffolderForOptimizely.Commerce.Vsix.csproj /restore
```

Building the Commerce add-on also builds the main extension, so that one command produces both `.vsix` files. The VSIX projects use the classic project format, so build them with `msbuild` (or Visual Studio), not `dotnet build`. To debug, set **ScaffolderForOptimizely.Commerce.Vsix** (both extensions) or **ScaffolderForOptimizely.Vsix** (CMS only) as the startup project and press F5. It starts an experimental instance of Visual Studio.

Restore fails if any package has a known moderate, high or critical vulnerability. See [CONTRIBUTING.md](CONTRIBUTING.md#vulnerable-packages) for how to fix one.

### Tests

- **Core.Tests:** unit tests plus [Verify](https://github.com/VerifyTests/Verify) snapshot tests of every scenario in [src/ScaffolderForOptimizely.Core.Tests/Shared/Scenarios.cs](src/ScaffolderForOptimizely.Core.Tests/Shared/Scenarios.cs). After an intended template change, review the `.received.txt` files and rename them to `.verified.txt`.
- **Compile.Tests:** compiles every scenario's generated C# (it links the same `Scenarios.cs`) against the real `EPiServer.CMS` 12 and `EPiServer.Commerce.Core` 14 packages and fails on any error or warning. It also checks project detection against an Alloy-style project.

### Layout

```
src/
  ScaffolderForOptimizely.sln
  Directory.Build.props                   version and package metadata shared by every project
  nuget.config                            nuget.org plus Optimizely's feed
  ScaffolderForOptimizely.Core            netstandard2.0: item catalogue, config, detection, generation, templates
  ScaffolderForOptimizely.Vsix            net48 VSIX: CMS commands, dialog, Roslyn workspace access, file writing
  ScaffolderForOptimizely.Commerce.Vsix   net48 VSIX add-on: the Optimizely Commerce menu; depends on the main extension
  ScaffolderForOptimizely.Core.Tests      unit and snapshot tests, plus the shared test scenarios
  ScaffolderForOptimizely.Compile.Tests   compile tests against the real Optimizely packages
schema/                                   JSON schema for .optimizely-scaffolder.json
```

Test folders mirror the Core folders (`Catalog`, `Config`, `Context`, `Generation`, `Providers`). Snapshots live in `ScaffolderForOptimizely.Core.Tests/Generation/Snapshots`.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for tests, adding an item and pull requests, and [docs/publishing.md](docs/publishing.md) for releases. To report a vulnerability, see [SECURITY.md](SECURITY.md).

## Licence

[MIT](LICENSE). Not affiliated with or endorsed by Optimizely. Optimizely and Episerver are trademarks of Optimizely.
