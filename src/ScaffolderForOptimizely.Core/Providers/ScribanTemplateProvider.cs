using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Scriban;
using Scriban.Runtime;
using ScaffolderForOptimizely.Core.Generation;

namespace ScaffolderForOptimizely.Core.Providers;

/// <summary>Renders Scriban (<c>.sbn</c>) templates from a store (embedded or per-solution override).</summary>
public sealed class ScribanTemplateProvider : ITemplateProvider
{
    private readonly ITemplateStore _store;

    public ScribanTemplateProvider(ITemplateStore store, string source)
    {
        _store = store;
        Source = source;
    }

    public string Source { get; }

    public Task<string?> TryRenderAsync(PlannedOutput output, GenerationRequest request, ProviderLog log, CancellationToken cancellationToken)
    {
        var found = _store.TryGet(request.Context.CmsProfile, output.Definition.TemplateName);
        if (found is null)
        {
            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>(Render(found.Value.Text, found.Value.Path, output));
    }

    public static string Render(string templateText, string templatePath, PlannedOutput output)
    {
        var template = Template.Parse(templateText, templatePath);
        if (template.HasErrors)
        {
            throw new TemplateRenderException(templatePath, string.Join(Environment.NewLine, template.Messages.Select(m => m.ToString())));
        }

        var globals = new ScriptObject();
        foreach (var pair in output.Model)
        {
            globals.Add(pair.Key, pair.Value);
        }

        var context = new TemplateContext
        {
            StrictVariables = false,
            EnableRelaxedMemberAccess = true,
        };
        context.PushGlobal(globals);

        try
        {
            return template.Render(context);
        }
        catch (Scriban.Syntax.ScriptRuntimeException ex)
        {
            throw new TemplateRenderException(templatePath, ex.Message);
        }
    }
}

public sealed class TemplateRenderException : Exception
{
    public TemplateRenderException(string templatePath, string message)
        : base($"Template '{templatePath}' failed: {message}")
    {
        TemplatePath = templatePath;
    }

    public string TemplatePath { get; }
}
