namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>
/// Folder settings for infrastructure and editor items. Each is configurable in
/// <c>folders</c>, detected from existing implementations, or falls back to an Alloy-style folder.
/// </summary>
public static class FolderKeys
{
    public const string Initialization = "initialization";
    public const string StartupFilters = "startupFilters";
    public const string ScheduledJobs = "scheduledJobs";
    public const string ServiceExtensions = "serviceExtensions";
    public const string SelectionFactories = "selectionFactories";
    public const string EditorDescriptors = "editorDescriptors";
    public const string VisitorGroups = "visitorGroups";
    public const string ContentEvents = "contentEvents";
    public const string Validators = "validators";
    public const string CatalogControllers = "catalogControllers";
    public const string Promotions = "promotions";
    public const string Payments = "payments";
    public const string Shipping = "shipping";
    public const string OrderServices = "orderServices";
}
