using System.Linq;
using ScaffolderForOptimizely.Core.Catalog;
using ScaffolderForOptimizely.Core.Context;
using ScaffolderForOptimizely.Core.Providers;
using Xunit;

namespace ScaffolderForOptimizely.Core.Tests;

public class CatalogTests
{
    [Fact]
    public void Every_output_has_an_embedded_cms12_template()
    {
        var missing = ItemCatalog.All
            .SelectMany(i => i.Outputs.Select(o => (i.Id, o.TemplateName)))
            .Where(x => EmbeddedTemplateStore.Instance.TryGet(Profiles.Cms12, x.TemplateName) is null)
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void Item_ids_are_unique()
    {
        Assert.Equal(ItemCatalog.All.Count, ItemCatalog.All.Select(i => i.Id).Distinct().Count());
    }

    [Fact]
    public void Enabling_options_exist_on_their_item()
    {
        foreach (var item in ItemCatalog.All)
        {
            foreach (var output in item.Outputs.Where(o => o.EnabledByOption is not null))
            {
                Assert.Contains(item.Options, o => o.Key == output.EnabledByOption && o.Kind == OptionKind.Bool);
            }
        }
    }

    [Fact]
    public void Cms13_falls_back_to_cms12_templates()
    {
        Assert.NotNull(EmbeddedTemplateStore.Instance.TryGet(Profiles.Cms13, "PageType.cs.sbn"));
    }
}
