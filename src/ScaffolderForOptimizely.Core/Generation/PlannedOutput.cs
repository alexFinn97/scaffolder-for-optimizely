using System.Collections.Generic;
using ScaffolderForOptimizely.Core.Catalog;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>One file about to be generated: where it goes, its names and its template variables.</summary>
public sealed class PlannedOutput
{
    public PlannedOutput(OutputDefinition definition, string fullPath, string relativePath, string className, string @namespace)
    {
        Definition = definition;
        FullPath = fullPath;
        RelativePath = relativePath;
        ClassName = className;
        Namespace = @namespace;
    }

    public OutputDefinition Definition { get; }

    public string FullPath { get; }

    public string RelativePath { get; }

    /// <summary>Class declared by this file (empty for views).</summary>
    public string ClassName { get; }

    public string Namespace { get; }

    public bool IsPrimary { get; set; }

    /// <summary>Namespaces the file needs imported (added by the post-processor).</summary>
    public List<string> RequiredUsings { get; } = new();

    /// <summary>Variables passed to the Scriban template.</summary>
    public Dictionary<string, object> Model { get; } = new();

    public bool IsCSharp => FullPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase);
}
