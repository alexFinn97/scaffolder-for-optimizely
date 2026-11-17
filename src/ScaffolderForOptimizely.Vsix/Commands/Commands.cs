using Community.VisualStudio.Toolkit;
using ScaffolderForOptimizely.Core.Catalog;

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
