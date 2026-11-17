using System;

namespace ScaffolderForOptimizely.Commerce.Extension;

/// <summary>
/// Mirrors the symbols in VSCommandTable.vsct. Keep both in sync. Named apart from the main
/// extension's PackageGuids/PackageIds, which are also visible here.
/// </summary>
internal static class CommercePackageGuids
{
    public const string CommercePackageString = "a9dc2109-38e7-4a59-989e-2ef85605b9a5";
    public const string CommerceCmdSetString = "5dd36305-8484-42c4-aa87-187ff291252c";

    public static readonly Guid CommercePackage = new(CommercePackageString);
    public static readonly Guid CommerceCmdSet = new(CommerceCmdSetString);
}

internal static class CommercePackageIds
{
    public const int CommerceMenu = 0x2000;
    public const int CommerceCatalogGroup = 0x2100;
    public const int CommerceRenderingGroup = 0x2200;
    public const int CommerceMarketingGroup = 0x2300;
    public const int CommerceOrdersGroup = 0x2400;
    public const int AddProduct = 0x0111;
    public const int AddVariation = 0x0112;
    public const int AddBundle = 0x0113;
    public const int AddPackage = 0x0114;
    public const int AddCatalogCategory = 0x0115;
    public const int AddContentController = 0x0116;
    public const int AddPromotion = 0x0117;
    public const int AddPaymentProvider = 0x0118;
    public const int AddShippingProvider = 0x0119;
    public const int AddOrderServiceDecorator = 0x011A;
}
