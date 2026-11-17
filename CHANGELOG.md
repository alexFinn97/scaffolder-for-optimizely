# Changelog

Both extensions, Scaffolder for Optimizely and Scaffolder for Optimizely Commerce, are released together with the same version number. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and each release's section becomes its GitHub release notes.

## [Unreleased]

## [0.9.3] - 2026-11-17

First Visual Studio Marketplace release, as a preview. 0.9.0 to 0.9.2 didn't reach the Marketplace because of release workflow and listing problems; everything listed under them ships in this release.

### Fixed
- The Marketplace rejected the extensions' descriptions for being over 280 characters; both are shorter now.
- Release tests now check the Marketplace publisher's rules (description and display name length, tags, language, internal names) before a release goes out.

## [0.9.2] - 2026-11-17 (GitHub release only)

### Fixed
- The Marketplace rejected the extensions' tags: they're now separated with semicolons, not commas.

## [0.9.1] - 2026-11-17 (GitHub release only)

### Fixed
- The release workflow failed to create the GitHub release.
- The release workflow ignored version tags without a `v` prefix, such as the ones `git flow release finish` creates.

## [0.9.0] - 2026-11-17 (not published)

First Visual Studio Marketplace release, as a preview.

### Changed
- The built-in templates are now the default (`templateSource: "embedded"`), so what the dialog previews is what gets written. Optimizely's `dotnet new` templates are still available with `templateSource: "dotnet-new"`.

### Added
- Marketplace listings for both extensions.
- A release workflow: pushing a version tag builds, tests, creates a GitHub release with both `.vsix` files and publishes them to the Marketplace.

## [0.4.0]

### Changed
- Commerce items moved into a separate add-on extension, Scaffolder for Optimizely Commerce, which depends on Scaffolder for Optimizely. The main extension now only has the **Optimizely CMS** menu.

## [0.3.1]

### Added
- Optimizely Commerce 14 items: Product, Variation, Bundle, Package and Category content types (each with an optional controller, view model and Razor view), Content Controller, Promotion (entry, order or shipping, with a processor), Payment Provider, Shipping Provider and Order Service Decorator.
- An editable controller path on Commerce content types, and a `folders.catalogControllers` setting, detected from existing Commerce controllers.
- Detection of catalog base classes and types, and of where Commerce classes live.

### Changed
- The CMS menu is now called **Optimizely CMS**.

## [0.2.1]

### Added
- Startup Filter (`IStartupFilter`, optional middleware and registration extension).

## [0.2.0]

### Added
- Infrastructure items: Initialization Module, Scheduled Job and Service Collection Extension.
- Editor items: Selection Factory, Editor Descriptor, Visitor Group Criterion, Content Events Handler and Content Validator.
- Folder detection for infrastructure and editor items, with matching `folders.*` settings.

## [0.1.4]

### Added
- CMS 12 content types and rendering: Page Type, Block Type, Media Type, Page Controller, Block Component, Partial Content Component and View Component.
- Project-aware defaults (base classes, groups, namespaces, nullable, layout, view and view model locations), `.optimizely-scaffolder.json` settings with a JSON schema, and per-solution template overrides.
- Editable view and view model paths, and a "Pass view path to View()" option.
- Add New Item templates for the core items.
