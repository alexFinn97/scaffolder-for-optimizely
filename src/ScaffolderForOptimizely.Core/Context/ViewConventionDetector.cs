using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScaffolderForOptimizely.Core.Context;

/// <summary>
/// Learns where the project keeps page and block views by matching existing views' <c>@model</c>
/// to known content types, e.g. <c>Views/Shared/Blocks/TeaserBlock.cshtml</c> with
/// <c>@model TeaserBlock</c> gives the block pattern <c>Views/Shared/Blocks/{name}.cshtml</c>.
/// Run after <see cref="ProjectFileScanner"/> and <see cref="CompilationAnalyzer"/>.
/// </summary>
public static class ViewConventionDetector
{
    public const string NamePlaceholder = "{name}";

    private static readonly Regex Identifier = new("[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

    public static void Apply(ProjectContext context)
    {
        if (context.UsesFeatureFolders)
        {
            // Views sit next to their classes; the layout rule already covers that.
            return;
        }

        context.PageViewPattern = Detect(context.RazorViews, context.PageTypes);
        context.BlockViewPattern = Detect(context.RazorViews, context.BlockTypes);
    }

    /// <summary>
    /// The most common pattern among views whose model is one of <paramref name="contentTypes"/>
    /// (directly, as <c>XViewModel</c>, or as a generic argument). Needs two views to agree, unless
    /// only one view matched at all.
    /// </summary>
    public static string? Detect(IEnumerable<RazorViewInfo> views, IEnumerable<string> contentTypes)
    {
        var shortNames = new HashSet<string>(contentTypes.Select(ShortName), StringComparer.Ordinal);
        if (shortNames.Count == 0)
        {
            return null;
        }

        var patterns = new List<string>();
        foreach (var view in views)
        {
            var match = Identifier.Matches(view.ModelType).Cast<Match>()
                .Select(m => m.Value)
                .Select(id => id.EndsWith("ViewModel", StringComparison.Ordinal) ? id.Substring(0, id.Length - "ViewModel".Length) : id)
                .FirstOrDefault(shortNames.Contains);

            if (match is null || view.RelativePath.IndexOf(match, StringComparison.Ordinal) < 0)
            {
                continue;
            }

            patterns.Add(view.RelativePath.Replace(match, NamePlaceholder));
        }

        var ranked = patterns
            .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ranked.Count == 0)
        {
            return null;
        }

        return ranked[0].Count() >= 2 || patterns.Count == 1 ? ranked[0].Key : null;
    }

    private static string ShortName(string fullName)
    {
        var index = fullName.LastIndexOf('.');
        return index < 0 ? fullName : fullName.Substring(index + 1);
    }
}
