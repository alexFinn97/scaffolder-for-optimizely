using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ScaffolderForOptimizely.Core.Generation;

/// <summary>
/// Tidies rendered C# whichever template produced it: adds missing usings, switches to
/// block-scoped namespaces when the project uses them, and normalises line endings.
/// </summary>
public static class CSharpPostProcessor
{
    private static readonly Regex UsingLine = new(@"^using\s+(?<ns>[A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$", RegexOptions.Compiled);
    private static readonly Regex FileScopedNamespace = new(@"^namespace\s+(?<ns>[A-Za-z_][A-Za-z0-9_.]*)\s*;\s*$", RegexOptions.Compiled);

    public static string Process(string text, IEnumerable<string> requiredUsings, bool fileScopedNamespaces, string newLine = "\r\n")
    {
        var lines = SplitLines(text);
        lines = EnsureUsings(lines, requiredUsings);
        if (!fileScopedNamespaces)
        {
            lines = ToBlockScoped(lines);
        }

        return Join(RemoveBlankLinesInsideBraces(lines), newLine);
    }

    /// <summary>Drops blank lines straight after "{" or straight before "}" left by optional template sections.</summary>
    private static List<string> RemoveBlankLinesInsideBraces(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().Length == 0)
            {
                var previous = result.Count > 0 ? result[result.Count - 1].Trim() : string.Empty;
                var next = lines.Skip(i + 1).FirstOrDefault(l => l.Trim().Length > 0)?.Trim() ?? string.Empty;
                if (previous == "{" || next.StartsWith("}", StringComparison.Ordinal))
                {
                    continue;
                }
            }

            result.Add(lines[i]);
        }

        return result;
    }

    /// <summary>Normalises line endings for non-C# files (views).</summary>
    public static string NormalizeText(string text, string newLine = "\r\n") => Join(SplitLines(text), newLine);

    private static List<string> EnsureUsings(List<string> lines, IEnumerable<string> requiredUsings)
    {
        var existing = new List<string>();
        var firstUsing = -1;
        var lastUsing = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            var match = UsingLine.Match(trimmed);
            if (match.Success)
            {
                existing.Add(match.Groups["ns"].Value);
                if (firstUsing < 0)
                {
                    firstUsing = i;
                }

                lastUsing = i;
            }
            else if (trimmed.Length > 0 && !trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                break;
            }
        }

        var all = existing.Concat(requiredUsings.Where(u => !string.IsNullOrWhiteSpace(u)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(u => u == "System" || u.StartsWith("System.", StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(u => u, StringComparer.Ordinal)
            .Select(u => $"using {u};")
            .ToList();

        if (all.Count == existing.Count)
        {
            return lines;
        }

        var result = new List<string>();
        if (firstUsing < 0)
        {
            result.AddRange(all);
            result.Add(string.Empty);
            result.AddRange(lines);
        }
        else
        {
            result.AddRange(lines.Take(firstUsing));
            result.AddRange(all);
            result.AddRange(lines.Skip(lastUsing + 1));
        }

        return result;
    }

    private static List<string> ToBlockScoped(List<string> lines)
    {
        var index = lines.FindIndex(l => FileScopedNamespace.IsMatch(l.Trim()));
        if (index < 0)
        {
            return lines;
        }

        var ns = FileScopedNamespace.Match(lines[index].Trim()).Groups["ns"].Value;
        var result = new List<string>(lines.Take(index))
        {
            $"namespace {ns}",
            "{",
        };

        var body = lines.Skip(index + 1).ToList();
        while (body.Count > 0 && body[0].Trim().Length == 0)
        {
            body.RemoveAt(0);
        }

        while (body.Count > 0 && body[body.Count - 1].Trim().Length == 0)
        {
            body.RemoveAt(body.Count - 1);
        }

        result.AddRange(body.Select(l => l.Trim().Length == 0 ? string.Empty : "    " + l));
        result.Add("}");
        return result;
    }

    private static List<string> SplitLines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Select(l => l.TrimEnd()).ToList();

    private static string Join(List<string> lines, string newLine)
    {
        // Collapse runs of blank lines left behind by template conditionals.
        var sb = new StringBuilder();
        var blank = 0;
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                blank++;
                if (blank > 1)
                {
                    continue;
                }
            }
            else
            {
                blank = 0;
            }

            sb.Append(line).Append(newLine);
        }

        var result = sb.ToString().TrimEnd('\r', '\n', ' ');
        return result.TrimStart('\r', '\n') + newLine;
    }
}
