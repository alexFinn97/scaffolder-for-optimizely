using System;

namespace ScaffolderForOptimizely.Extension;

/// <summary>Mirrors the symbols in VSCommandTable.vsct. Keep both in sync.</summary>
internal static class PackageGuids
{
    public const string ScaffolderPackageString = "f1fdcef1-a2ce-45dd-880e-abdaa8c0ee10";
    public const string ScaffolderCmdSetString = "8496ae22-5963-4937-b396-69043c24fe69";

    public static readonly Guid ScaffolderPackage = new(ScaffolderPackageString);
    public static readonly Guid ScaffolderCmdSet = new(ScaffolderCmdSetString);
}

internal static class PackageIds
{
    public const int OptimizelyMenu = 0x1000;
    public const int ContentTypesGroup = 0x1100;
    public const int RenderingGroup = 0x1200;
    public const int AddPageType = 0x0101;
    public const int AddBlockType = 0x0102;
    public const int AddMediaType = 0x0103;
    public const int AddPageController = 0x0104;
    public const int AddBlockComponent = 0x0105;
    public const int AddPartialComponent = 0x0106;
    public const int AddViewComponent = 0x0107;
    public const int InfrastructureGroup = 0x1300;
    public const int EditorGroup = 0x1400;
    public const int AddInitializationModule = 0x0108;
    public const int AddScheduledJob = 0x0109;
    public const int AddServiceCollectionExtension = 0x010A;
    public const int AddSelectionFactory = 0x010B;
    public const int AddEditorDescriptor = 0x010C;
    public const int AddVisitorGroupCriterion = 0x010D;
    public const int AddContentEventsHandler = 0x010E;
    public const int AddContentValidator = 0x010F;
    public const int AddStartupFilter = 0x0110;
}
