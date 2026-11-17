using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Web.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace $rootnamespace$;

// Replace BlockData with your block type. The view goes in Views/Shared/Components/<Name>/Default.cshtml,
// where <Name> is the class name without "ViewComponent".
public class $safeitemname$ : AsyncBlockComponent<BlockData>
{
    protected override async Task<IViewComponentResult> InvokeComponentAsync(BlockData currentBlock)
    {
        return await Task.FromResult(View(currentBlock));
    }
}
