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
            if (id == null)
                id = "RemoteLog";
            if(!SystemManager.Instance.LoadDocument(id, out var doc))
            {
                doc = new Agebull.ZeroNet.ZeroApi.StationDocument
                {
                    Name = id,
                    Caption = id,
                    Description = id
                };
            }
            return View(doc);
        }
    }
}