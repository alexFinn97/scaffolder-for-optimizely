using Community.VisualStudio.Toolkit;
using ScaffolderForOptimizely.Commands;
using ScaffolderForOptimizely.Commerce.Extension;
using ScaffolderForOptimizely.Core.Catalog;

namespace ScaffolderForOptimizely.Commerce.Commands;

// Each command opens the main extension's scaffold dialog for a Commerce item. ScaffoldCommand<T>
// shows them only in projects that reference EPiServer.Commerce.

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddProduct)]
internal sealed class AddProductCommand : ScaffoldCommand<AddProductCommand>
{
    protected override string ItemId => ItemCatalog.Ids.Product;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddVariation)]
internal sealed class AddVariationCommand : ScaffoldCommand<AddVariationCommand>
{
    protected override string ItemId => ItemCatalog.Ids.Variation;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddBundle)]
internal sealed class AddBundleCommand : ScaffoldCommand<AddBundleCommand>
{
    protected override string ItemId => ItemCatalog.Ids.Bundle;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddPackage)]
internal sealed class AddPackageCommand : ScaffoldCommand<AddPackageCommand>
{
    protected override string ItemId => ItemCatalog.Ids.Package;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddCatalogCategory)]
internal sealed class AddCatalogCategoryCommand : ScaffoldCommand<AddCatalogCategoryCommand>
{
    protected override string ItemId => ItemCatalog.Ids.CatalogCategory;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddContentController)]
internal sealed class AddContentControllerCommand : ScaffoldCommand<AddContentControllerCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ContentController;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddPromotion)]
internal sealed class AddPromotionCommand : ScaffoldCommand<AddPromotionCommand>
{
    protected override string ItemId => ItemCatalog.Ids.Promotion;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddPaymentProvider)]
internal sealed class AddPaymentProviderCommand : ScaffoldCommand<AddPaymentProviderCommand>
{
    protected override string ItemId => ItemCatalog.Ids.PaymentProvider;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddShippingProvider)]
internal sealed class AddShippingProviderCommand : ScaffoldCommand<AddShippingProviderCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ShippingProvider;
}

[Command(CommercePackageGuids.CommerceCmdSetString, CommercePackageIds.AddOrderServiceDecorator)]
internal sealed class AddOrderServiceDecoratorCommand : ScaffoldCommand<AddOrderServiceDecoratorCommand>
{
    protected override string ItemId => ItemCatalog.Ids.OrderServiceDecorator;
}
