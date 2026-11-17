using System.Runtime.CompilerServices;
using VerifyTests;

namespace ScaffolderForOptimizely.Core.Tests.Generation;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // The scenarios use a fixed GUID on purpose; keep it visible in the snapshots.
        VerifierSettings.DontScrubGuids();
        VerifierSettings.DontScrubDateTimes();
    }
}
