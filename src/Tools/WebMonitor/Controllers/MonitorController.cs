using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Models;

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