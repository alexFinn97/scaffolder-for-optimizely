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

    /// <summary>Custom Razor view path relative to the project; empty means the conventional path.</summary>
    public const string ViewPath = "viewPath";

    /// <summary>Custom view model file path relative to the project; empty means the default.</summary>
    public const string ViewModelPath = "viewModelPath";

    /// <summary>Custom controller file path relative to the project; empty means the default. The file name is the class name.</summary>
    public const string ControllerPath = "controllerPath";

    /// <summary>"true"/"false" to force passing the view path to View(); empty means decide automatically.</summary>
    public const string PassViewPath = "passViewPath";

    // Infrastructure and editor items.
    public const string Configurable = "configurable";
    public const string Stoppable = "stoppable";
    public const string StatusProgress = "statusProgress";
    public const string Logger = "logger";
    public const string Restartable = "restartable";
    public const string MethodName = "methodName";
    public const string Autosuggest = "autosuggest";
    public const string TargetType = "targetType";
    public const string UiHint = "uiHint";
    public const string ClientEditingClass = "clientEditingClass";
    public const string Category = "category";
    public const string Scope = "scope";
    public const string Middleware = "middleware";
    public const string Registration = "registration";

    // Commerce items.
    public const string MetaClassName = "metaClassName";
    public const string Kind = "kind";
    public const string PaymentMethod = "paymentMethod";
    public const string SystemKeyword = "systemKeyword";
    public const string DecorateDefault = "decorateDefault";

    /// <summary>Prefix for content event switches, e.g. <c>event:PublishingContent</c>.</summary>
    public const string EventPrefix = "event:";

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
