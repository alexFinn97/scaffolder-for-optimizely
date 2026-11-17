# Scaffolder for Optimizely

Right-click scaffolding for **Optimizely CMS 12** in Visual Studio 2022 and 2026. Add page types, block types, controllers, view components, scheduled jobs, editor extensions and more from Solution Explorer, with defaults taken from the project you're working in.

> Preview: this extension is new. Please report anything that generates the wrong code in [GitHub issues](https://github.com/alexFinn97/scaffolder-for-optimizely/issues).

## How it works

Right-click a project or folder in Solution Explorer and choose **Add > Optimizely CMS**. Each item opens a dialog with its options and a live preview of every file it will create. Click **Add** and the files are written into the project.

The menu only appears in projects that reference Optimizely CMS.

## What you can add

**Content types**
- Page Type, with an optional controller, view model and Razor view
- Block Type, with an optional block component, view model and Razor view
- Media Type (image, video or generic file)

**Rendering**
- Page Controller
- Block Component (`AsyncBlockComponent<T>`, `BlockComponent<T>` or a plain `ViewComponent`)
- Partial Content Component
- View Component

**Infrastructure**
- Initialization Module (plain or `IConfigurableModule`)
- Startup Filter (`IStartupFilter`, the ASP.NET Core alternative to initialization modules)
- Scheduled Job (stoppable, progress reporting, `ILogger`, restartable)
- Service Collection Extension

**Editor**
- Selection Factory (dropdown or autosuggest)
- Editor Descriptor
- Visitor Group Criterion and its settings model
- Content Events Handler, registered and subscribed for you
- Content Validator (`IValidate<T>`)

## Fits your project

When the dialog opens, the extension reads your project and suggests:

- your own base classes, such as `SitePageData`, and your `GroupNames` constants
- your namespace style (file-scoped or block-scoped) and nullable setting
- where views live, learnt from your existing views, and whether `View()` needs the view path
- where your view models, initialization modules, scheduled jobs and other classes already live

Every suggestion stays editable. Pin team conventions in a `.optimizely-scaffolder.json` file next to your solution, and override any template per solution. See the [documentation](https://github.com/alexFinn97/scaffolder-for-optimizely#readme).

## Optimizely Commerce

Install **Scaffolder for Optimizely Commerce** as well to scaffold catalog content, promotions, payment and shipping providers and order service decorators.

---

Free and open source under the MIT licence. Not affiliated with or endorsed by Optimizely.
