using System.Collections.Generic;
using System.Linq;

namespace ScaffolderForOptimizely.Core.Catalog;

/// <summary>v0.2: infrastructure and editor/UI items.</summary>
public static partial class ItemCatalog
{
    /// <summary>Content events offered by the content events handler, with their default state.</summary>
    private static readonly (string Name, bool On)[] ContentEvents =
    {
        ("SavingContent", false),
        ("SavedContent", false),
        ("PublishingContent", true),
        ("PublishedContent", true),
        ("MovingContent", false),
        ("MovedContent", false),
        ("DeletingContent", false),
        ("DeletedContent", false),
    };

    private static ItemDefinition InitializationModule() => new(
        Ids.InitializationModule,
        Categories.CmsInfrastructure,
        "Initialization Module",
        "An IInitializableModule that runs once when the site starts, optionally also configuring services.",
        OutputRole.Class,
        new[]
        {
            Flag(OptionKeys.Configurable, "Configure services (IConfigurableModule)", false),
        },
        new[]
        {
            new OutputDefinition("module", OutputRole.Class, "InitializationModule.cs.sbn")
            {
                DotnetNew = new DotnetNewMapping("epi-cms-initializationmodule")
                {
                    BoolArguments = new Dictionary<string, string> { [OptionKeys.Configurable] = "--configurable" },
                },
            },
        })
    {
        NameExample = "DisplayRegistryInitialization",
        FolderKey = FolderKeys.Initialization,
        DefaultFolder = "Business/Initialization",
    };

    // The ASP.NET Core way to run startup code, which Optimizely's CMS 12 docs prefer over initialization modules.
    // No dotnet new mapping: EPiServer.Templates has no startup filter template.
    private static ItemDefinition StartupFilter() => new(
        Ids.StartupFilter,
        Categories.CmsInfrastructure,
        "Startup Filter",
        "An ASP.NET Core IStartupFilter that runs code once at startup, when the request pipeline is built. The modern alternative to an initialization module.",
        OutputRole.Class,
        new[]
        {
            Flag(OptionKeys.Scope, "Run startup code in a service scope", true),
            Flag(OptionKeys.Logger, "Inject ILogger", true),
            Flag(OptionKeys.Middleware, "Add middleware at the start of the pipeline", false),
            Flag(OptionKeys.Registration, "Registration extension (services.AddX())", true),
        },
        new[]
        {
            new OutputDefinition("filter", OutputRole.Class, "StartupFilter.cs.sbn"),
            new OutputDefinition("extensions", OutputRole.Companion, "StartupFilterExtensions.cs.sbn")
            {
                ClassNameSuffix = "StartupFilterExtensions",
                EnabledByOption = OptionKeys.Registration,
            },
        })
    {
        NameSuffix = "StartupFilter",
        NameExample = "SiteStartupFilter",
        FolderKey = FolderKeys.StartupFilters,
        DefaultFolder = "Business/Initialization",
    };

    // No dotnet new mapping: Optimizely's job template schedules itself (DefaultEnabled, every 12 hours)
    // and ignores these options, so it can't produce what the dialog asks for.
    private static ItemDefinition ScheduledJob() => new(
        Ids.ScheduledJob,
        Categories.CmsInfrastructure,
        "Scheduled Job",
        "A ScheduledJobBase job, shown in Admin > Scheduled Jobs. Not scheduled until an admin sets an interval.",
        OutputRole.Class,
        new[]
        {
            DisplayNameOption(),
            DescriptionOption(),
            Flag(OptionKeys.Stoppable, "Stoppable", true),
            Flag(OptionKeys.StatusProgress, "Report progress (OnStatusChanged)", true),
            Flag(OptionKeys.Logger, "Inject ILogger", true),
            Flag(OptionKeys.Restartable, "Restart if the site shuts down mid-run", false),
        },
        new[]
        {
            new OutputDefinition("job", OutputRole.Class, "ScheduledJob.cs.sbn"),
        })
    {
        NameSuffix = "Job",
        NameExample = "ImportProductsJob",
        FolderKey = FolderKeys.ScheduledJobs,
        DefaultFolder = "Business/ScheduledJobs",
    };

    private static ItemDefinition ServiceCollectionExtension() => new(
        Ids.ServiceCollectionExtension,
        Categories.CmsInfrastructure,
        "Service Collection Extension",
        "A static IServiceCollection extension for registering services; call it from Startup.ConfigureServices.",
        OutputRole.Class,
        new[]
        {
            new ItemOption(OptionKeys.MethodName, "Method name", OptionKind.Text) { IsAutoValue = true },
        },
        new[]
        {
            new OutputDefinition("extensions", OutputRole.Class, "ServiceCollectionExtensions.cs.sbn"),
        })
    {
        NameSuffix = "ServiceCollectionExtensions",
        NameExample = "SiteServiceCollectionExtensions",
        FolderKey = FolderKeys.ServiceExtensions,
        DefaultFolder = "Extensions",
    };

    private static ItemDefinition SelectionFactory() => new(
        Ids.SelectionFactory,
        Categories.CmsEditor,
        "Selection Factory",
        "Options for a dropdown ([SelectOne]/[SelectMany]), or an autosuggest list ([AutoSuggestSelection]).",
        OutputRole.Class,
        new[]
        {
            Flag(OptionKeys.Autosuggest, "Autosuggest (ISelectionQuery) instead of a fixed list", false),
        },
        new[]
        {
            new OutputDefinition("factory", OutputRole.Class, "SelectionFactory.cs.sbn"),
        })
    {
        NameExample = "ColourSelectionFactory",
        FolderKey = FolderKeys.SelectionFactories,
        DefaultFolder = "Business/SelectionFactories",
    };

    private static ItemDefinition EditorDescriptor() => new(
        Ids.EditorDescriptor,
        Categories.CmsEditor,
        "Editor Descriptor",
        "Changes how properties with a UI hint are edited, optionally with a custom Dojo editor.",
        OutputRole.Class,
        new[]
        {
            new ItemOption(OptionKeys.TargetType, "Property type", OptionKind.Choice)
            {
                DefaultValue = "string",
                AllowCustomValue = true,
                Choices = new[] { "string", "int", "bool", "DateTime", "ContentReference", "PageReference", "XhtmlString", "ContentArea", "IList<string>" }
                    .Select(t => new OptionChoice(t, t)).ToList(),
            },
            new ItemOption(OptionKeys.UiHint, "UI hint", OptionKind.Text)
            {
                IsAutoValue = true,
                Description = "Use it on properties as [UIHint(\"...\")].",
            },
            new ItemOption(OptionKeys.ClientEditingClass, "Client editing class", OptionKind.Text)
            {
                Description = "Optional Dojo module, e.g. app/editors/ColourPicker. Leave empty to only change metadata.",
            },
        },
        new[]
        {
            new OutputDefinition("descriptor", OutputRole.Class, "EditorDescriptor.cs.sbn"),
        })
    {
        NameSuffix = "EditorDescriptor",
        NameExample = "ColourPickerEditorDescriptor",
        FolderKey = FolderKeys.EditorDescriptors,
        DefaultFolder = "Business/EditorDescriptors",
    };

    private static ItemDefinition VisitorGroupCriterion() => new(
        Ids.VisitorGroupCriterion,
        Categories.CmsEditor,
        "Visitor Group Criterion",
        "A custom visitor group criterion and its settings model.",
        OutputRole.Class,
        new[]
        {
            DisplayNameOption(),
            new ItemOption(OptionKeys.Category, "Category", OptionKind.Text) { DefaultValue = "Custom" },
            DescriptionOption(),
        },
        new[]
        {
            new OutputDefinition("criterion", OutputRole.Class, "VisitorGroupCriterion.cs.sbn"),
            new OutputDefinition("model", OutputRole.Companion, "VisitorGroupCriterionModel.cs.sbn") { ClassNameSuffix = "CriterionModel" },
        })
    {
        NameSuffix = "Criterion",
        NameExample = "CountryCriterion",
        FolderKey = FolderKeys.VisitorGroups,
        DefaultFolder = "Business/VisitorGroups",
    };

    private static ItemDefinition ContentEventsHandler() => new(
        Ids.ContentEventsHandler,
        Categories.CmsEditor,
        "Content Events Handler",
        "A DI-registered handler for content events, plus an initialization module that registers and subscribes it.",
        OutputRole.Class,
        ContentEvents.Select(e => Flag(OptionKeys.EventPrefix + e.Name, e.Name, e.On)).ToList(),
        new[]
        {
            new OutputDefinition("handler", OutputRole.Class, "ContentEventsHandler.cs.sbn"),
            new OutputDefinition("initialization", OutputRole.Companion, "ContentEventsInitialization.cs.sbn") { ClassNameSuffix = "Initialization" },
        })
    {
        NameSuffix = "Handler",
        NameExample = "ContentPublishingHandler",
        FolderKey = FolderKeys.ContentEvents,
        DefaultFolder = "Business/ContentEvents",
    };

    private static ItemDefinition ContentValidator() => new(
        Ids.ContentValidator,
        Categories.CmsEditor,
        "Content Validator",
        "An IValidate<T> that checks content before it's saved. Picked up automatically.",
        OutputRole.Class,
        new[]
        {
            ContentTypeOption(ChoiceSource.ContentTypes),
        },
        new[]
        {
            new OutputDefinition("validator", OutputRole.Class, "ContentValidator.cs.sbn"),
        })
    {
        NameSuffix = "Validator",
        NameExample = "ArticlePageValidator",
        DefaultName = "NewValidator",
        NameFromOption = OptionKeys.ContentType,
        FolderKey = FolderKeys.Validators,
        DefaultFolder = "Business/Validation",
    };

    private static ItemOption Flag(string key, string label, bool defaultValue) => new(key, label, OptionKind.Bool)
    {
        DefaultValue = defaultValue ? "true" : "false",
    };
}
