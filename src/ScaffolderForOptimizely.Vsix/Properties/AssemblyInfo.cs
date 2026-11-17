using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Scaffolder for Optimizely")]
[assembly: AssemblyDescription("Right-click scaffolding for Optimizely CMS 12.")]
[assembly: AssemblyCompany("Alex Finnigan")]
[assembly: AssemblyProduct("Scaffolder for Optimizely")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Alex Finnigan")]
[assembly: ComVisible(false)]

// The Commerce add-on derives its commands from ScaffoldCommand<T> and reuses the dialog and services.
[assembly: InternalsVisibleTo("ScaffolderForOptimizely.Commerce")]
[assembly: AssemblyVersion("0.9.0.0")]
[assembly: AssemblyFileVersion("0.9.0.0")]
