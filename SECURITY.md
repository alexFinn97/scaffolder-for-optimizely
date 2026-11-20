# Security policy

## Supported versions

Only the latest release of Scaffolder for Optimizely and Scaffolder for Optimizely Commerce gets security fixes. Both extensions are released together with the same version number.

## Reporting a vulnerability

Please don't open a public issue. Report it privately through GitHub instead: on the [Security tab](https://github.com/alexFinn97/scaffolder-for-optimizely/security), choose **Report a vulnerability**, or go straight to [the private report form](https://github.com/alexFinn97/scaffolder-for-optimizely/security/advisories/new).

Include:

- the extension and version, and the Visual Studio version
- what you did, what happened, and what an attacker could do with it
- for generated code, the item, the options you picked and the code it produced

This is a personal open-source project, so responses are best effort. You'll hear back once the report has been read. If it's confirmed, the fix and a GitHub security advisory are published together, crediting you unless you'd rather not be named.

## What's in scope

- **The extensions:** anything that lets a project, a `.optimizely-scaffolder.json` file or a template make Visual Studio do something it shouldn't, such as writing files outside the project.
- **Generated code:** templates that produce code with a security flaw, for example missing authorisation or output encoding where Optimizely's own patterns include it.
- **Dependencies:** a vulnerable package that ships inside a `.vsix`. Dependabot and the CI checks should catch these first.

Vulnerabilities in Optimizely CMS or Commerce themselves belong with Optimizely, not here.
