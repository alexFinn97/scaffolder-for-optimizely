using System.Collections.Generic;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>
/// Commerce catalog content base types. Each catalog item derives from one of these, and detection
/// groups the project's abstract catalog classes by the root they derive from.
/// </summary>
public static class CatalogRoots
{
    public const string Product = "EPiServer.Commerce.Catalog.ContentTypes.ProductContent";
    public const string Variation = "EPiServer.Commerce.Catalog.ContentTypes.VariationContent";
    public const string Bundle = "EPiServer.Commerce.Catalog.ContentTypes.BundleContent";
    public const string Package = "EPiServer.Commerce.Catalog.ContentTypes.PackageContent";
    public const string Category = "EPiServer.Commerce.Catalog.ContentTypes.NodeContent";

    public static IReadOnlyList<string> All { get; } = new[] { Product, Variation, Bundle, Package, Category };
}
