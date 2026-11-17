# Scaffolder for Optimizely Commerce

Right-click scaffolding for **Optimizely Commerce 14** in Visual Studio 2022 and 2026. An add-on for **Scaffolder for Optimizely**, which it needs installed first.

> Preview: this extension is new. Please report anything that generates the wrong code in [GitHub issues](https://github.com/alexFinn97/scaffolder-for-optimizely/issues).

## How it works

In a project that references Optimizely Commerce, right-click the project or a folder in Solution Explorer and choose **Add > Optimizely Commerce**. Each item opens a dialog with its options and a live preview of every file it will create.

## What you can add

**Catalog**
- Product, Variation, Bundle, Package and Category content types (`[CatalogContentType]`), each with an optional controller, view model and Razor view
- Content Controller (`ContentController<T>`) for an existing catalog type

**Marketing**
- Promotion: entry, order or shipping, with a working promotion processor

**Orders**
- Payment Provider: one class implementing `IPaymentPlugin` and the legacy `AbstractPaymentGateway`, plus an optional checkout `IPaymentMethod`
- Shipping Provider: `IShippingPlugin` and the legacy `IShippingGateway`
- Order Service Decorator: wraps the default line item validator, placed price processor, tax calculator or shipping calculator, so you change only what you need

## Fits your project

The add-on uses the same project detection as Scaffolder for Optimizely, plus Commerce specifics:

- your abstract catalog base classes, per kind (e.g. a `BaseProduct` for products)
- where your Commerce controllers, promotions, payment and shipping providers and order services already live

Every generated file compiles against Optimizely Commerce 14. See the [documentation](https://github.com/alexFinn97/scaffolder-for-optimizely#readme).

---

Free and open source under the MIT licence. Not affiliated with or endorsed by Optimizely.
