namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>Option keys shared by the catalogue, the generator and the templates.</summary>
public static class OptionKeys
{
    public const string DisplayName = "displayName";
    public const string Description = "description";
    public const string BaseClass = "baseClass";
    public const string Group = "group";
    public const string ContentType = "contentType";
    public const string Controller = "controller";
    public const string Component = "component";
    public const string ViewModel = "viewModel";
    public const string View = "view";
    public const string BlockStyle = "blockStyle";
    public const string Extensions = "extensions";
    public const string Async = "async";

    /// <summary>Not shown in the dialog: fixed per dialog session so the preview keeps the same GUID.</summary>
    public const string Guid = "guid";
}

/// <summary>Values for <see cref="OptionKeys.BlockStyle"/>.</summary>
public static class BlockStyles
{
    public const string Async = "async";
    public const string Sync = "sync";
    public const string ViewComponent = "viewComponent";
}
