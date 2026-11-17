using System.Linq;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>v0.3: Optimizely Commerce 14 items.</summary>
public static partial class ItemCatalog
{
    /// <summary>Values for <see cref="OptionKeys.Kind"/> on the Promotion item.</summary>
    public static class PromotionKinds
    {
        public const string Entry = "entry";
        public const string Order = "order";
        public const string Shipping = "shipping";
    }

    /// <summary>Values for <see cref="OptionKeys.Kind"/> on the Order Service Decorator item.</summary>
    public static class OrderServiceKinds
    {
        public const string LineItemValidator = "lineItemValidator";
        public const string PlacedPriceProcessor = "placedPriceProcessor";
        public const string TaxCalculator = "taxCalculator";
        public const string ShippingCalculator = "shippingCalculator";
    }

    /// <summary>
    /// One catalog content type (product, variation...). Shares the page type's companions: a
    /// ContentController, an optional view model and a Razor view.
    /// </summary>
    private static ItemDefinition CatalogType(string id, string label, string root, string description, string nameExample) => new(
        id,
        Categories.CommerceCatalog,
        label,
        description + " Optionally with a controller, view model and Razor view.",
        OutputRole.Model,
        new[]
        {
            DisplayNameOption(),
            DescriptionOption(),
            BaseClassOption(root),
            new ItemOption(OptionKeys.MetaClassName, "Meta class name", OptionKind.Text)
            {
                IsAutoValue = true,
                Description = "Commerce's internal name for the type; must be unique. Defaults to the class name.",
            },
            Companion(OptionKeys.Controller, "Controller", "companions.controller", true),
            new ItemOption(OptionKeys.ControllerPath, "Controller path", OptionKind.Text)
            {
                Description = "Relative to the project. The file name becomes the class name.",
                EnabledWhenOption = OptionKeys.Controller,
                IsAutoValue = true,
            },
            Companion(OptionKeys.ViewModel, "View model", "companions.viewModel", false),
            ViewModelPathOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        CatalogRenderingOutputs(new OutputDefinition("model", OutputRole.Model, "CatalogContentType.cs.sbn")))
    {
        ContentKind = ContentKind.Catalog,
        NameExample = nameExample,
    };

    private static ItemDefinition ContentController() => new(
        Ids.ContentController,
        Categories.CommerceRendering,
        "Content Controller",
        "A ContentController<T> that renders an existing Commerce content type (product, variation, category...), with an optional view model and Razor view.",
        OutputRole.Controller,
        new[]
        {
            ContentTypeOption(ChoiceSource.CatalogTypes),
            Companion(OptionKeys.ViewModel, "Use a view model", "companions.viewModel", false),
            ViewModelPathOption(),
            Companion(OptionKeys.View, "Razor view", "companions.view", true),
            ViewPathOption(),
            PassViewPathOption(),
        },
        CatalogRenderingOutputs(null))
    {
        // Right-clicking the project puts it with the other Commerce controllers.
        ContentKind = ContentKind.Catalog,
        NameSuffix = "Controller",
        NameExample = "FashionProductController",
        DefaultName = "NewContentController",
        NameFromOption = OptionKeys.ContentType,
    };

    private static OutputDefinition[] CatalogRenderingOutputs(OutputDefinition? model)
    {
        var rendering = new[]
        {
            new OutputDefinition("controller", OutputRole.Controller, "ContentController.cs.sbn")
            {
                EnabledByOption = model is null ? null : OptionKeys.Controller,
            },
            new OutputDefinition("viewModel", OutputRole.ViewModel, "CatalogViewModel.cs.sbn")
            {
                EnabledByOption = OptionKeys.ViewModel,
            },
            new OutputDefinition("view", OutputRole.View, "CatalogView.cshtml.sbn")
            {
                EnabledByOption = OptionKeys.View,
                ViewKind = ViewKind.PageIndex,
                ViewFor = ContentKind.Catalog,
            },
        };

        return model is null ? rendering : new[] { model }.Concat(rendering).ToArray();
    }

    private static ItemDefinition Promotion() => new(
        Ids.Promotion,
        Categories.CommerceMarketing,
        "Promotion",
        "A custom promotion type for the Marketing UI, plus the processor that evaluates it against a cart.",
        OutputRole.Class,
        new[]
        {
            new ItemOption(OptionKeys.Kind, "Promotion type", OptionKind.Choice)
            {
                DefaultValue = PromotionKinds.Entry,
                Choices = new[]
                {
                    new OptionChoice(PromotionKinds.Entry, "Entry: discount on catalog items (EntryPromotion)"),
                    new OptionChoice(PromotionKinds.Order, "Order: discount on the order total (OrderPromotion)"),
                    new OptionChoice(PromotionKinds.Shipping, "Shipping: discount on shipping (ShippingPromotion)"),
                },
            },
            DisplayNameOption(),
            DescriptionOption(),
        },
        new[]
        {
            new OutputDefinition("promotion", OutputRole.Class, "Promotion.cs.sbn"),
            new OutputDefinition("processor", OutputRole.Companion, "PromotionProcessor.cs.sbn") { ClassNameSuffix = "PromotionProcessor" },
        })
    {
        NameSuffix = "Promotion",
        NameExample = "BuyThreeGetDiscountPromotion",
        FolderKey = FolderKeys.Promotions,
        DefaultFolder = "Business/Promotions",
    };

    private static ItemDefinition PaymentProvider() => new(
        Ids.PaymentProvider,
        Categories.CommerceOrders,
        "Payment Provider",
        "A payment gateway for Commerce admin > Payments (IPaymentPlugin and the legacy AbstractPaymentGateway), with an optional checkout payment method.",
        OutputRole.Class,
        new[]
        {
            new ItemOption(OptionKeys.PaymentMethod, "Checkout payment method (IPaymentMethod)", OptionKind.Bool) { DefaultValue = "true" },
            new ItemOption(OptionKeys.SystemKeyword, "System keyword", OptionKind.Text)
            {
                IsAutoValue = true,
                Description = "Must match the payment method's system keyword in Commerce admin.",
                EnabledWhenOption = OptionKeys.PaymentMethod,
            },
        },
        new[]
        {
            new OutputDefinition("gateway", OutputRole.Class, "PaymentGateway.cs.sbn"),
            new OutputDefinition("method", OutputRole.Companion, "PaymentMethod.cs.sbn")
            {
                ClassNameSuffix = "PaymentMethod",
                EnabledByOption = OptionKeys.PaymentMethod,
            },
        })
    {
        NameSuffix = "PaymentGateway",
        NameExample = "InvoicePaymentGateway",
        FolderKey = FolderKeys.Payments,
        DefaultFolder = "Business/Payments",
    };

    private static ItemDefinition ShippingProvider() => new(
        Ids.ShippingProvider,
        Categories.CommerceOrders,
        "Shipping Provider",
        "A shipping gateway for Commerce admin > Shipping (IShippingPlugin and the legacy IShippingGateway) that returns a rate per shipment.",
        OutputRole.Class,
        new ItemOption[0],
        new[]
        {
            new OutputDefinition("gateway", OutputRole.Class, "ShippingGateway.cs.sbn"),
        })
    {
        NameSuffix = "ShippingGateway",
        NameExample = "CourierShippingGateway",
        FolderKey = FolderKeys.Shipping,
        DefaultFolder = "Business/Shipping",
    };

    private static ItemDefinition OrderServiceDecorator() => new(
        Ids.OrderServiceDecorator,
        Categories.CommerceOrders,
        "Order Service Decorator",
        "Wraps one of Commerce's order services (line item validation, placed prices, tax or shipping calculation) so you can change its behaviour and fall back to the default.",
        OutputRole.Class,
        new[]
        {
            new ItemOption(OptionKeys.Kind, "Service", OptionKind.Choice)
            {
                DefaultValue = OrderServiceKinds.LineItemValidator,
                Choices = new[]
                {
                    new OptionChoice(OrderServiceKinds.LineItemValidator, "Line item validator (ILineItemValidator)"),
                    new OptionChoice(OrderServiceKinds.PlacedPriceProcessor, "Placed price processor (IPlacedPriceProcessor)"),
                    new OptionChoice(OrderServiceKinds.TaxCalculator, "Tax calculator (ITaxCalculator)"),
                    new OptionChoice(OrderServiceKinds.ShippingCalculator, "Shipping calculator (IShippingCalculator)"),
                },
            },
            Flag(OptionKeys.Registration, "Registration extension (services.AddX())", true),
        },
        new[]
        {
            new OutputDefinition("decorator", OutputRole.Class, "OrderServiceDecorator.cs.sbn"),
            new OutputDefinition("extensions", OutputRole.Companion, "OrderServiceExtensions.cs.sbn")
            {
                ClassNameSuffix = "Extensions",
                EnabledByOption = OptionKeys.Registration,
            },
        })
    {
        NameExample = "MarketLineItemValidator",
        DefaultName = "CustomLineItemValidator",
        FolderKey = FolderKeys.OrderServices,
        DefaultFolder = "Business/Orders",
    };
}
