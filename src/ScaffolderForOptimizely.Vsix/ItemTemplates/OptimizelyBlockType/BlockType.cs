using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace $rootnamespace$;

[ContentType(
    DisplayName = "$safeitemname$",
    GUID = "$guid1$",
    Description = "")]
public class $safeitemname$ : BlockData
{
    [CultureSpecific]
    [Display(
        Name = "Heading",
        Description = "The heading shown on the block.",
        GroupName = SystemTabNames.Content,
        Order = 10)]
    public virtual string Heading { get; set; }
}
