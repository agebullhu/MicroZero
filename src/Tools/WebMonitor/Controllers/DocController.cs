using System.Linq;
using Agebull.Common.ApiDocuments;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Controllers
{
    public class DocController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Index(string id)
        {
            if (!ZeroApplication.Config.TryGetConfig(id,out var config))
            {
                config = ZeroApplication.Config.Stations.FirstOrDefault(p=>p.StationType == ZeroStationType.Api);
            }

            StationDocument doc;
            if (config == null)
            {
                doc = new StationDocument
                {
                    Name = id,
                    Caption = "无文档",
                    Description = "无文档"
                };
            }
            else if (!SystemManager.Instance.LoadDocument(config.StationName, out doc))
            {
                doc = new StationDocument
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