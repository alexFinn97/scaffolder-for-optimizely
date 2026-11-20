# To do

Things that still need checking or sorting out. Tick them off, or delete them once they're done or no longer apply.

## Check

### Installing

- [ ] Install **Scaffolder for Optimizely Commerce** on a Visual Studio that doesn't have **Scaffolder for Optimizely**, and note what happens. The add-on's manifest declares the dependency, so the installer should refuse and name the missing extension, but this hasn't been tried.
- [ ] Uninstall the main extension while the add-on is installed, and note what happens.

### In a running Commerce 14 site

The generated code compiles against Commerce 14, but none of it has run in a site yet.

- [ ] A new **Promotion** appears in the right group of the Marketing UI's "create promotion" list. The generated `GroupName` (`entrypromotion`, `orderpromotion`, `shippingpromotion`) copies Optimizely's built-in promotions and hasn't been confirmed.
- [ ] A generated promotion of each kind (entry, order, shipping) applies to a cart as expected.
- [ ] A **Payment Provider** can be picked as a payment method's gateway in Commerce admin > Payments, and its checkout `IPaymentMethod` creates a payment that gets processed.
- [ ] A **Shipping Provider** can be picked in Commerce admin > Shipping and returns a rate. Only a parameterless constructor is generated, and how Commerce creates shipping gateways hasn't been confirmed.
- [ ] An **Order Service Decorator** registered with its `services.AddX()` extension takes effect, for each of the four services.
- [ ] A catalog type's generated **controller and view** render a product, variation and category.

### In a running CMS 12 site

The menus and dialogs have been tried in Visual Studio, but not all generated code has been run in a site.

- [ ] **Scheduled Job:** appears in Admin > Scheduled Jobs, runs, stops when asked and reports progress.
- [ ] **Content Events Handler:** the subscribed events fire.
- [ ] **Startup Filter:** runs at startup, including the optional middleware.
- [ ] **Visitor Group Criterion:** appears in the visitor group editor and matches.
- [ ] **Editor Descriptor** and **Selection Factory:** change the editing UI for properties that use them.
- [ ] **Content Validator:** blocks saving invalid content.

### Templates

- [ ] With `"templateSource": "dotnet-new"`, check the four items that use Optimizely's templates still produce code that works on CMS 12, now that `EPiServer.Templates` 2.x targets CMS 13. If not, consider removing the option.

## Sort out

- [ ] **Marketplace token:** note when the `VS_MARKETPLACE_PAT` token expires. Azure DevOps stops creating the "All accessible organizations" tokens the Marketplace publisher needs on 1 December 2026. Find out what replaces them, or plan to upload by hand from the GitHub release (see [docs/publishing.md](docs/publishing.md)).
- [ ] **GitHub Actions on Node 20:** `actions/checkout@v4`, `actions/setup-dotnet@v4` and `microsoft/setup-msbuild@v2` show deprecation warnings and are forced onto Node 24. Move `ci.yml` and `release.yml` to versions built for Node 24 once they're available. Dependabot's weekly GitHub Actions pull request now raises these bumps, so this is done once that's merged.
- [ ] **VsixPublisher telemetry crash:** the publisher in `Microsoft.VSSDK.BuildTools` 17.14.2094 crashes in its own telemetry (an assembly version mismatch) and exits with an error even when the upload succeeds. `release.yml` works around it by checking for the "Uploaded" message. When updating that package, check whether it's fixed and remove the workaround if so.
- [ ] **Marketplace category:** both listings show under "Tools". Check whether `"coding"` in `marketplace/*.json` is the best category.
- [ ] **Add New Item templates:** only the five original items (page type, block type, media type, page controller, block component) have them. Decide whether to add the rest or drop them, since the right-click menus cover everything.

## Later

- [ ] **1.0:** once other people have used it, set `<Preview>` to `false` in both manifests and release 1.0.0. The release tests check both manifests agree.
- [ ] **CMS 13 and Commerce 15:** add `cms13` templates (the template lookup already falls back from `cms13` to `cms12`) and compile tests against those packages.
- [ ] **Solution format:** consider moving to `.slnx`, once it's clear the classic VSIX projects work with it.
