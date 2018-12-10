using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ZeroNet.Devops.Monitor.Controllers
{
    public class WeixinController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}