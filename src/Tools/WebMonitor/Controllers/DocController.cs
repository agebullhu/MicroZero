using System;
using System.Linq;
using System.Collections.Generic;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
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
            if (Documents.TryGetValue(id, out var doc))
                return View(doc);



            if (!ZeroApplication.Config.TryGetConfig(id,out var config))
            {
                config = ZeroApplication.Config.Stations.FirstOrDefault(p=>p.StationType == ZeroStationType.Api);
            }

            if (config == null)
            {
                doc = new StationDocument
                {
                    Name = id
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

            Documents.TryAdd(id,doc);
            return View(doc);
        }

        private static Dictionary<string, StationDocument> Documents = new Dictionary<string, StationDocument>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Update(string id)
        {
            Documents.Clear();
            return Redirect("/Doc/Index/"+ id);
        }
    }
}