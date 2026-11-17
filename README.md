# Scaffolder for Optimizely

A Visual Studio 2022/2026 extension that brings back right-click scaffolding for Optimizely CMS 12: content types, controllers and components, infrastructure such as initialization modules and scheduled jobs, and editor extensions, with defaults taken from your project.

> **Status: v0.2 preview.** CMS content types, rendering, infrastructure and editor items are in. Commerce comes in v0.3.

## What it does

In Solution Explorer, right-click a project or folder and choose **Add > Optimizely**:

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

Each command opens a dialog with the item's options and a live preview of every file it will create. The menu only appears in projects that reference Optimizely CMS.

Plain item templates for the core items are also available under **Add > New Item > Optimizely**, for people who prefer that route. They don't have the project-aware options.

### Project-aware defaults

When the dialog opens, the extension reads the project with Roslyn and offers:

- abstract base classes you already have (e.g. `SitePageData`) as the default base class
- `GroupNames`-style constants (or classes marked `[GroupDefinitions]`) as content type groups
- existing page and block types for controllers and components
- your namespace style (file-scoped or block-scoped) and nullable setting
- feature-folder vs `Views/` layout, based on where existing views live
- view path patterns learnt from existing views' `@model` (e.g. `Views/Shared/Blocks/{name}.cshtml`)
- the folder holding most existing `*ViewModel` classes (or a folder named `ViewModels`)
- whether a custom `IViewLocationExpander` exists, which decides whether to pass the view path to `View()`
- where your existing initialization modules, startup filters, scheduled jobs, selection factories, editor descriptors, criteria, content event handlers and validators live

Content type pickers start blank. The view path and view model path fill in from the above, and stay editable. Right-clicking a folder puts the main file there. Right-clicking the project uses the configured or detected folder, else a conventional one (e.g. `Models/Pages`, `Business/ScheduledJobs`). Existing files are never overwritten.

## Settings: `.optimizely-scaffolder.json`

Put this file next to your solution (or in any folder above the project) to pin conventions for the whole team. Every value is optional.

```json
{
  "$schema": "https://raw.githubusercontent.com/alexFinn97/scaffolder-for-optimizely/master/schema/optimizely-scaffolder.schema.json",
  "templateSource": "dotnet-new",
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
2. **Optimizely's own templates** (`templateSource: "dotnet-new"`, the default): content types, page controllers and partial components use `dotnet new` from `EPiServer.Templates`. If the pack isn't installed, the extension offers to install it.
3. **Built-in templates** for everything else: views, view models, block components and media types, plus anything option 2 can't express (e.g. a controller that uses a view model).

The dialog preview always shows the built-in or override templates, so it stays instant.

## Building

Requirements: Visual Studio 2022 17.x or 2026 with the **Visual Studio extension development** workload (needed for F5 debugging; command-line builds work without it), and the .NET 8 SDK.

```bash
dotnet test tests/ScaffolderForOptimizely.Core.Tests
dotnet test tests/ScaffolderForOptimizely.Compile.Tests
msbuild src/ScaffolderForOptimizely.Vsix/ScaffolderForOptimizely.Vsix.csproj /restore
```

The VSIX project uses the classic project format, so build it with `msbuild` (or Visual Studio), not `dotnet build`. To debug, set **ScaffolderForOptimizely.Vsix** as the startup project and press F5. It starts an experimental instance of Visual Studio.

### Tests

- **Core.Tests:** unit tests plus [Verify](https://github.com/VerifyTests/Verify) snapshot tests of every scenario in [tests/Shared/Scenarios.cs](tests/Shared/Scenarios.cs). After an intended template change, review the `.received.txt` files and rename them to `.verified.txt`.
- **Compile.Tests:** compiles every scenario's generated C# against the real `EPiServer.CMS` 12 packages and fails on any error or warning. It also checks project detection against an Alloy-style project.

### Layout

```
src/ScaffolderForOptimizely.Core   netstandard2.0: item catalogue, config, detection, generation, templates
src/ScaffolderForOptimizely.Vsix   net48 VSIX: commands, dialog, Roslyn workspace access, file writing
tests/                             unit, snapshot and compile tests
schema/                            JSON schema for .optimizely-scaffolder.json
```

## Licence

[MIT](LICENSE)
