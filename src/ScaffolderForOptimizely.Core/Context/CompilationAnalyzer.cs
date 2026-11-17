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

        foreach (var type in GetSourceTypes(compilation.Assembly.GlobalNamespace))
        {
            if (type.TypeKind != TypeKind.Class)
            {
                continue;
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

            if (IsGroupNameHolder(type, groupDefinitions))
            {
                AddGroupNames(type, context);
            }
        }

        context.PageBaseClasses.Sort(StringComparer.Ordinal);
        context.BlockBaseClasses.Sort(StringComparer.Ordinal);
        context.MediaBaseClasses.Sort(StringComparer.Ordinal);
        context.PageTypes.Sort(StringComparer.Ordinal);
        context.BlockTypes.Sort(StringComparer.Ordinal);
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
