using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebMonitor.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            //ZeroApplication.Config.Foreach(p => p.Count = 0);
        }
        
    }
}
