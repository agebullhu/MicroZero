using Microsoft.AspNetCore.Mvc;
using ZeroNet.Http.Route;

namespace WebMonitor.Controlers
{
    public class FlowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult Query(string id)
        {
            return Json(FlowTracer.Query(id));
        }
    }
}