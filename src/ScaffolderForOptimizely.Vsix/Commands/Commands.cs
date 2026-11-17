using Community.VisualStudio.Toolkit;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Extension;

namespace ScaffolderForOptimizely.Commands;

// One command per catalogue item; the behaviour lives in ScaffoldCommand<T>.

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddPageType)]
internal sealed class AddPageTypeCommand : ScaffoldCommand<AddPageTypeCommand>
{
    protected override string ItemId => ItemCatalog.Ids.PageType;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddBlockType)]
internal sealed class AddBlockTypeCommand : ScaffoldCommand<AddBlockTypeCommand>
{
    protected override string ItemId => ItemCatalog.Ids.BlockType;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddMediaType)]
internal sealed class AddMediaTypeCommand : ScaffoldCommand<AddMediaTypeCommand>
{
    protected override string ItemId => ItemCatalog.Ids.MediaType;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddPageController)]
internal sealed class AddPageControllerCommand : ScaffoldCommand<AddPageControllerCommand>
{
    protected override string ItemId => ItemCatalog.Ids.PageController;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddBlockComponent)]
internal sealed class AddBlockComponentCommand : ScaffoldCommand<AddBlockComponentCommand>
{
    protected override string ItemId => ItemCatalog.Ids.BlockComponent;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddPartialComponent)]
internal sealed class AddPartialComponentCommand : ScaffoldCommand<AddPartialComponentCommand>
{
    protected override string ItemId => ItemCatalog.Ids.PartialComponent;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddViewComponent)]
internal sealed class AddViewComponentCommand : ScaffoldCommand<AddViewComponentCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ViewComponent;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddInitializationModule)]
internal sealed class AddInitializationModuleCommand : ScaffoldCommand<AddInitializationModuleCommand>
{
    protected override string ItemId => ItemCatalog.Ids.InitializationModule;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddStartupFilter)]
internal sealed class AddStartupFilterCommand : ScaffoldCommand<AddStartupFilterCommand>
{
    protected override string ItemId => ItemCatalog.Ids.StartupFilter;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddScheduledJob)]
internal sealed class AddScheduledJobCommand : ScaffoldCommand<AddScheduledJobCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ScheduledJob;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddServiceCollectionExtension)]
internal sealed class AddServiceCollectionExtensionCommand : ScaffoldCommand<AddServiceCollectionExtensionCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ServiceCollectionExtension;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddSelectionFactory)]
internal sealed class AddSelectionFactoryCommand : ScaffoldCommand<AddSelectionFactoryCommand>
{
    protected override string ItemId => ItemCatalog.Ids.SelectionFactory;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddEditorDescriptor)]
internal sealed class AddEditorDescriptorCommand : ScaffoldCommand<AddEditorDescriptorCommand>
{
    protected override string ItemId => ItemCatalog.Ids.EditorDescriptor;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddVisitorGroupCriterion)]
internal sealed class AddVisitorGroupCriterionCommand : ScaffoldCommand<AddVisitorGroupCriterionCommand>
{
    protected override string ItemId => ItemCatalog.Ids.VisitorGroupCriterion;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddContentEventsHandler)]
internal sealed class AddContentEventsHandlerCommand : ScaffoldCommand<AddContentEventsHandlerCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ContentEventsHandler;
}

[Command(PackageGuids.ScaffolderCmdSetString, PackageIds.AddContentValidator)]
internal sealed class AddContentValidatorCommand : ScaffoldCommand<AddContentValidatorCommand>
{
    protected override string ItemId => ItemCatalog.Ids.ContentValidator;
}
