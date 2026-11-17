# Publishing to the Visual Studio Marketplace

One-time setup, then each release is a tag push. Both extensions are published under the same publisher.

## 1. Create a publisher

1. Go to [marketplace.visualstudio.com/manage](https://marketplace.visualstudio.com/manage) and sign in with the Microsoft account that should own the extensions.
2. Choose **Create publisher** and fill in:
   - **Name:** shown on the listings, e.g. `Alex Finnigan`.
   - **ID:** part of every listing's URL (`itemName=<ID>.ScaffolderForOptimizely`). It can't be changed later.
   - Optionally a description, logo and the GitHub repository as the website.
3. Put the ID in both files in [`marketplace/`](../marketplace), replacing `YOUR-PUBLISHER-ID`. The release tests check both files use the same ID.

The `Publisher` attribute in the `.vsixmanifest` files is the display name and stays `Alex Finnigan`.

## 2. Choose how releases get to the Marketplace

### Option A: automatic, with a personal access token

The release workflow publishes with `VsixPublisher.exe`, which needs an Azure DevOps personal access token (PAT).

1. Sign in to [dev.azure.com](https://dev.azure.com) with the same Microsoft account. Create an organisation if you don't have one; any free one works.
2. Under **User settings > Personal access tokens**, create a token with:
   - **Organization:** All accessible organizations
   - **Scopes:** Custom defined, **Marketplace > Manage**
   - **Expiration:** up to a year. Note the date: publishing stops working when it expires.
3. In the GitHub repository, add it under **Settings > Secrets and variables > Actions** as `VS_MARKETPLACE_PAT`.

> **Heads-up (November 2026):** Azure DevOps is retiring "All accessible organizations" (global) tokens. Microsoft says they can't be *created* after **1 December 2026**. It isn't clear yet whether existing ones keep working, or what replaces them for Visual Studio extensions. Create the token before that date. If it stops working, or you can't create one, use option B: the workflow still builds and attaches both `.vsix` files to the GitHub release.

### Option B: manual upload

Without the `VS_MARKETPLACE_PAT` secret, the release workflow skips publishing. Download both `.vsix` files from the GitHub release, then at [marketplace.visualstudio.com/manage](https://marketplace.visualstudio.com/manage):

1. **New extension > Visual Studio** for the first release, or **Update** on the listing afterwards.
2. Upload **Scaffolder for Optimizely** first, then **Scaffolder for Optimizely Commerce**, which depends on it.
3. For the first upload, copy the overview text from the matching `.md` file in `marketplace/`.

## 3. Release

1. Make sure `CHANGELOG.md` has a `## [x.y.z]` section for the version in `src/Directory.Build.props`.
2. Commit, then tag and push:

   ```bash
   git tag v0.9.0
   git push origin v0.9.0
   ```

3. The **Release** workflow checks the tag matches the version, runs every test, builds both extensions, and creates a GitHub release with the changelog section as its notes. It then publishes to the Marketplace if option A is set up.

## Changing the version

The version appears in several files, and the release tests fail if any disagree:

- `src/Directory.Build.props`
- each VSIX project's `source.extension.vsixmanifest`, `Extension/source.extension.cs` and `Properties/AssemblyInfo.cs`
- the Commerce add-on's dependency range on the main extension, `[x.y.0,x.(y+1).0)`
- `CHANGELOG.md`

Dropping the Marketplace's Preview badge means setting `<Preview>` to `false` in both manifests.
