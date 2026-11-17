using EPiServer.Core;
using EPiServer.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace $rootnamespace$;

// Replace PageData with your page type.
public class $safeitemname$ : PageController<PageData>
{
    public IActionResult Index(PageData currentPage)
    {
        return View(currentPage);
    }
}
