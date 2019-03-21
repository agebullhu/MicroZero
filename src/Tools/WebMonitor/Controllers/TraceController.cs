using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controlers
{
    public class TraceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}