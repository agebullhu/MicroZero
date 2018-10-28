using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controllers
{
    public class MachineEventController : Controller
    { 
        public IActionResult Index()
        {
            return View();
        }
    }
}