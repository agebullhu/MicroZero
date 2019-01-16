using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controlers
{
    public class FLowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}