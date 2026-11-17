using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;
using EPiServer.Framework.DataAnnotations;

namespace $rootnamespace$;

[ContentType(
    DisplayName = "$safeitemname$",
    GUID = "$guid1$",
    Description = "")]
[MediaDescriptor(ExtensionString = "jpg,jpeg,png,gif,webp")]
public class $safeitemname$ : ImageData
{
    [Display(
        Name = "Copyright",
        Order = 10)]
    public virtual string Copyright { get; set; }
}
