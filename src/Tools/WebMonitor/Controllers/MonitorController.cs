using System.Linq;
using Agebull.MicroZero;
using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controlers
{
    public class MonitorController : Controller
    {
        public IActionResult Index(string id)
        {
            if (!ZeroApplication.Config.TryGetConfig(id, out var config))
            {
                config = ZeroApplication.Config.Stations.FirstOrDefault(p => p.StationType == ZeroStationType.Api) ?? new StationConfig
                {
                    Name = "错误"
                };
            }
            return View(config);
        }

    }
}