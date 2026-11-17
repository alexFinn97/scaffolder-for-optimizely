using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ScaffolderForOptimizely.Core.Context;

/// <summary>
/// Reads a Roslyn <see cref="Compilation"/> of the target project to find base classes, content
/// types, group name constants, the CMS version and code style.
/// </summary>
/// <remarks>
/// Compiled against Roslyn 4.0 (Visual Studio 2022 17.0) so it binds to whatever newer Roslyn the
/// running Visual Studio has loaded.
/// </remarks>
public static class CompilationAnalyzer
{
    private const string PageDataName = "EPiServer.Core.PageData";
    private const string BlockDataName = "EPiServer.Core.BlockData";
    private const string MediaDataName = "EPiServer.Core.MediaData";
    private const string GroupDefinitionsName = "EPiServer.DataAnnotations.GroupDefinitionsAttribute";
    private const string ViewLocationExpanderName = "Microsoft.AspNetCore.Mvc.Razor.IViewLocationExpander";

    private sealed class FolderRule
    {
        public FolderRule(string key, Func<INamedTypeSymbol, bool> matches)
        {
            Key = key;
            Matches = matches;
        }

        public string Key { get; }

        public Func<INamedTypeSymbol, bool> Matches { get; }
    }

    /// <summary>
    /// How to recognise existing implementations of each infrastructure/editor item. Rules whose
    /// Optimizely type isn't referenced by the project are left out.
    /// </summary>
    private static List<FolderRule> FolderRules(Compilation compilation)
    {
        INamedTypeSymbol? Type(string metadataName) => compilation.GetTypeByMetadataName(metadataName);

        var rules = new List<FolderRule>();
        void Add(string key, INamedTypeSymbol? symbol, Func<INamedTypeSymbol, INamedTypeSymbol, bool> matches)
        {
            if (symbol is not null)
            {
                rules.Add(new FolderRule(key, t => matches(t, symbol)));
            }
        }

        Add(Catalog.FolderKeys.Initialization, Type("EPiServer.Framework.IInitializableModule"), Implements);
        Add(Catalog.FolderKeys.StartupFilters, Type("Microsoft.AspNetCore.Hosting.IStartupFilter"), Implements);
        Add(Catalog.FolderKeys.ScheduledJobs, Type("EPiServer.Scheduler.ScheduledJobBase"), DerivesFrom);
        Add(Catalog.FolderKeys.SelectionFactories, Type("EPiServer.Shell.ObjectEditing.ISelectionFactory"), Implements);
        Add(Catalog.FolderKeys.SelectionFactories, Type("EPiServer.Shell.ObjectEditing.ISelectionQuery"), Implements);
        Add(Catalog.FolderKeys.EditorDescriptors, Type("EPiServer.Shell.ObjectEditing.EditorDescriptors.EditorDescriptor"), DerivesFrom);
        Add(Catalog.FolderKeys.VisitorGroups, Type("EPiServer.Personalization.VisitorGroups.CriterionBase`1"), DerivesFrom);
        Add(Catalog.FolderKeys.Validators, Type("EPiServer.Validation.IValidate`1"), Implements);
        Add(Catalog.FolderKeys.ServiceExtensions, Type("Microsoft.Extensions.DependencyInjection.IServiceCollection"),
            (t, services) => t.IsStatic && t.GetMembers().OfType<IMethodSymbol>().Any(m =>
                m.IsExtensionMethod && m.Parameters.Length > 0 && SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, services)));
        Add(Catalog.FolderKeys.ContentEvents, Type("EPiServer.ContentEventArgs"),
            (t, args) => t.GetMembers().OfType<IMethodSymbol>().Any(m =>
                m.Parameters.Any(p => p.Type is INamedTypeSymbol pt && (SymbolEqualityComparer.Default.Equals(pt, args) || DerivesFrom(pt, args)))));

        // Commerce: skipped automatically in projects without Commerce, as the types won't resolve.
        Add(Catalog.FolderKeys.Promotions, Type("EPiServer.Commerce.Marketing.PromotionData"), DerivesFrom);
        Add(Catalog.FolderKeys.Payments, Type("EPiServer.Commerce.Order.IPaymentPlugin"), Implements);
        Add(Catalog.FolderKeys.Payments, Type("Mediachase.Commerce.Orders.IPaymentGateway"), Implements);
        Add(Catalog.FolderKeys.Shipping, Type("Mediachase.Commerce.Orders.IShippingPlugin"), Implements);
        Add(Catalog.FolderKeys.Shipping, Type("Mediachase.Commerce.Orders.IShippingGateway"), Implements);
        foreach (var service in OrderServiceInterfaces)
        {
            Add(Catalog.FolderKeys.OrderServices, Type(service), Implements);
        }

        // Controllers whose base class is generic over catalog content, e.g. ContentController<ShirtProduct>
        // or a site's own CatalogControllerBase<T>.
        var catalogRoots = Catalog.CatalogRoots.All.Select(Type).OfType<INamedTypeSymbol>().ToList();
        Add(Catalog.FolderKeys.CatalogControllers, Type("Microsoft.AspNetCore.Mvc.Controller"),
            (t, controller) => !t.IsAbstract && DerivesFrom(t, controller) && BaseTypeArguments(t).Any(arg =>
                catalogRoots.Any(root => SymbolEqualityComparer.Default.Equals(arg, root) || DerivesFrom(arg, root))));

        return rules;
    }

    /// <summary>Order services the Order Service Decorator item can wrap.</summary>
    private static readonly string[] OrderServiceInterfaces =
    {
        "EPiServer.Commerce.Order.ILineItemValidator",
        "EPiServer.Commerce.Order.IPlacedPriceProcessor",
        "EPiServer.Commerce.Order.ITaxCalculator",
        "EPiServer.Commerce.Order.IShippingCalculator",
    };

    /// <summary>Type arguments anywhere in the base class chain, e.g. T in ContentController&lt;T&gt;.</summary>
    private static IEnumerable<INamedTypeSymbol> BaseTypeArguments(INamedTypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            foreach (var argument in current.TypeArguments.OfType<INamedTypeSymbol>())
            {
                yield return argument;
            }
        }
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol @interface) =>
        type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, @interface));

    /// <summary>Most common folder; ties go to the shallower, then alphabetically first.</summary>
    private static string? MostUsed(IEnumerable<string> folders) =>
        folders
            .GroupBy(f => f, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.Count(c => c == '/'))
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()?.Key;

    /// <summary>Folder of the type's first source file, relative to the project; null if outside it.</summary>
    private static string? FolderOf(INamedTypeSymbol type, string projectDirectory)
    {
        var path = type.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var directory = System.IO.Path.GetDirectoryName(path);
        var relative = directory is null ? string.Empty : NamespaceHelper.GetRelativePath(projectDirectory, directory);
        return relative.Length == 0 ? null : relative.Replace('\\', '/');
    }

    public static void Apply(Compilation compilation, ProjectContext context)
    {
        ApplyVersions(compilation, context);

        context.NullableEnabled = compilation.Options.NullableContextOptions != NullableContextOptions.Disable;

        var fileScoped = DetectFileScopedNamespaces(compilation);
        if (fileScoped.HasValue)
        {
            context.FileScopedNamespaces = fileScoped.Value;
        }

        var pageData = compilation.GetTypeByMetadataName(PageDataName);
        var blockData = compilation.GetTypeByMetadataName(BlockDataName);
        var mediaData = compilation.GetTypeByMetadataName(MediaDataName);
        var groupDefinitions = compilation.GetTypeByMetadataName(GroupDefinitionsName);
        var viewLocationExpander = compilation.GetTypeByMetadataName(ViewLocationExpanderName);
        var catalogRoots = Catalog.CatalogRoots.All
            .Select(name => (Name: name, Symbol: compilation.GetTypeByMetadataName(name)))
            .Where(r => r.Symbol is not null)
            .ToList();
        var viewModelFolders = new List<string>();
        var folderRules = FolderRules(compilation);
        var ruleFolders = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var type in GetSourceTypes(compilation.Assembly.GlobalNamespace))
        {
            if (type.TypeKind != TypeKind.Class)
            {
                continue;
            }

            foreach (var rule in folderRules)
            {
                if (rule.Matches(type) && FolderOf(type, context.ProjectDirectory) is { } ruleFolder)
                {
                    if (!ruleFolders.TryGetValue(rule.Key, out var list))
                    {
                        ruleFolders[rule.Key] = list = new List<string>();
                    }

                    list.Add(ruleFolder);
                }
            }

            if (viewLocationExpander is not null
                && type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, viewLocationExpander)))
            {
                context.HasViewLocationExpander = true;
            }

            if (type.ContainingType is null && type.Name.EndsWith("ViewModel", StringComparison.Ordinal)
                && FolderOf(type, context.ProjectDirectory) is { } folder)
            {
                viewModelFolders.Add(folder);
            }

            var fullName = type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);

            if (pageData is not null && DerivesFrom(type, pageData))
            {
                (type.IsAbstract ? context.PageBaseClasses : context.PageTypes).Add(fullName);
            }
            else if (blockData is not null && DerivesFrom(type, blockData))
            {
                (type.IsAbstract ? context.BlockBaseClasses : context.BlockTypes).Add(fullName);
            }
            else if (mediaData is not null && DerivesFrom(type, mediaData) && type.IsAbstract)
            {
                context.MediaBaseClasses.Add(fullName);
            }
            else if (catalogRoots.FirstOrDefault(r => DerivesFrom(type, r.Symbol!)) is { Name: not null } root)
            {
                if (!type.IsAbstract)
                {
                    context.CatalogTypes.Add(fullName);
                }
                else
                {
                    if (!context.CatalogBaseClasses.TryGetValue(root.Name, out var bases))
                    {
                        context.CatalogBaseClasses[root.Name] = bases = new List<string>();
                    }

                    bases.Add(fullName);
                }
            }

            if (IsGroupNameHolder(type, groupDefinitions))
            {
                AddGroupNames(type, context);
            }
        }

        // Where most existing view models live beats a folder that merely has the right name.
        if (MostUsed(viewModelFolders) is { } viewModels)
        {
            context.ViewModelsFolder = viewModels;
        }

        foreach (var pair in ruleFolders)
        {
            if (MostUsed(pair.Value) is { } folder)
            {
                context.DetectedFolders[pair.Key] = folder;
            }
        }

        context.PageBaseClasses.Sort(StringComparer.Ordinal);
        context.BlockBaseClasses.Sort(StringComparer.Ordinal);
        context.MediaBaseClasses.Sort(StringComparer.Ordinal);
        context.PageTypes.Sort(StringComparer.Ordinal);
        context.BlockTypes.Sort(StringComparer.Ordinal);
        context.CatalogTypes.Sort(StringComparer.Ordinal);
        foreach (var bases in context.CatalogBaseClasses.Values)
        {
            bases.Sort(StringComparer.Ordinal);
        }
    }

    private static void ApplyVersions(Compilation compilation, ProjectContext context)
    {
        foreach (var identity in compilation.ReferencedAssemblyNames)
        {
            if (identity.Name == "EPiServer")
            {
                context.CmsMajorVersion = identity.Version.Major;
            }
            else if (identity.Name == "EPiServer.Business.Commerce")
            {
                context.CommerceMajorVersion = identity.Version.Major;
            }
        }

        context.CmsProfile = context.CmsMajorVersion >= 13 ? Profiles.Cms13 : Profiles.Cms12;
    }

    /// <summary>
    /// Majority vote over hand-written files; null when the project has no namespaces yet.
    /// Falls back to the language version so a new C# 10+ project gets file-scoped namespaces.
    /// </summary>
    private static bool? DetectFileScopedNamespaces(Compilation compilation)
    {
        int fileScoped = 0, blockScoped = 0;
        LanguageVersion? languageVersion = null;

        foreach (var tree in compilation.SyntaxTrees)
        {
            if (tree.Options is CSharpParseOptions parseOptions)
            {
                languageVersion ??= parseOptions.LanguageVersion;
            }

            var path = tree.FilePath ?? string.Empty;
            if (path.IndexOf("\\obj\\", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/obj/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            var root = tree.GetRoot();
            if (root.DescendantNodes(n => n is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<FileScopedNamespaceDeclarationSyntax>().Any())
            {
                fileScoped++;
            }
            else if (root.DescendantNodes(n => n is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax).OfType<NamespaceDeclarationSyntax>().Any())
            {
                blockScoped++;
            }
        }

        if (fileScoped + blockScoped > 0)
        {
            return fileScoped >= blockScoped;
        }

        return languageVersion is null ? null : languageVersion.Value >= LanguageVersion.CSharp10;
    }

    private static IEnumerable<INamedTypeSymbol> GetSourceTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol child)
            {
                foreach (var type in GetSourceTypes(child))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol type)
            {
                foreach (var nested in WithNested(type))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
        {
            foreach (var inner in WithNested(nested))
            {
                yield return inner;
            }
        }
    }

    private static bool DerivesFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsGroupNameHolder(INamedTypeSymbol type, INamedTypeSymbol? groupDefinitions)
    {
        if (groupDefinitions is not null
            && type.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, groupDefinitions)))
        {
            return true;
        }

        return type.Name.EndsWith("GroupNames", StringComparison.Ordinal);
    }

    private static void AddGroupNames(INamedTypeSymbol type, ProjectContext context)
    {
        var ns = type.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() : string.Empty;

        var chain = new List<string>();
        for (var t = type; t is not null; t = t.ContainingType)
        {
            chain.Insert(0, t.Name);
        }

        var typeExpression = string.Join(".", chain);

        foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsConst && field.Type.SpecialType == SpecialType.System_String && field.DeclaredAccessibility == Accessibility.Public)
            {
                context.GroupNames.Add(new GroupNameCandidate(ns, $"{typeExpression}.{field.Name}"));
            }
        }
    }
}
