using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controllers
{
    public class DocController : Controller
    {
        public IActionResult Index(string id)
        {
            if (!ZeroApplication.Config.TryGetConfig(id,out var config))
            {
                config = ZeroApplication.Config.Stations.FirstOrDefault(p=>p.StationType == ZeroStationType.Api);
            }
            Agebull.ZeroNet.ZeroApi.StationDocument doc;
            if (config == null)
            {
                doc = new Agebull.ZeroNet.ZeroApi.StationDocument
                {
                    Name = id,
                    Caption = "无文档",
                    Description = "无文档"
                };
            }
            else if (!SystemManager.Instance.LoadDocument(config.StationName, out doc))
            {
                doc = new Agebull.ZeroNet.ZeroApi.StationDocument
                {
                    Name = config.Name,
                    Caption = config.Caption,
                    Description = config.Description
                };
            }
            return View(doc);
        }
    }
}