using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.AspNetCore.Mvc;
using ZeroNet.Http.Route;

namespace WebMonitor.Controllers
{
    public class PlanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Active()
        {
            var result = IocHelper.Create<PlanManage>().Active();
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult History()
        {
            var result = IocHelper.Create<PlanManage>().History();
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Station(string id)
        {
            var result = IocHelper.Create<PlanManage>().Station(id);
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult Filter(string id)
        {
            var result = IocHelper.Create<PlanManage>().Filter(id);
            return new JsonResult(result);
        }

        [HttpPost]
        public IActionResult Add([FromForm] ClientPlan plan)
        {
            try
            {
                var result = IocHelper.Create<PlanManage>().NewPlan(new ClientPlan
                {
                    plan_type = (plan_date_type)int.Parse(Request.Form["plan_type"]),
                    plan_value = short.Parse(Request.Form["plan_value"]),
                    plan_repet = int.Parse(Request.Form["plan_repet"]),
                    description = Request.Form["description"],
                    command = Request.Form["command"],
                    station = Request.Form["station"],
                    context = Request.Form["context"],
                    argument = Request.Form["argument"],
                    no_skip = int.Parse(Request.Form["plan_type"] ) == 1,
                    plan_time = string.IsNullOrWhiteSpace(Request.Form["plan_time"]) ? DateTime .MinValue: DateTime.Parse(Request.Form["plan_time"]),
                    skip_set = int.Parse(Request.Form["skip_set"]),
                });
                return new JsonResult(result);

            }
            catch
            {
                return new JsonResult(ApiResult.Error(ErrorCode.LocalException));
            }
        }

        [HttpGet]
        public IActionResult Flush()
        {
            var result = IocHelper.Create<PlanManage>().FlushList();
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Pause(string id)
        {
            var result = IocHelper.Create<PlanManage>().Pause(id);
            return new JsonResult(result);
        }
        [HttpGet]
        public IActionResult Reset(string id)
        {
            var result = IocHelper.Create<PlanManage>().Reset(id);
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Remove(string id)
        {
            var result = IocHelper.Create<PlanManage>().Remove(id);
            return new JsonResult(result);
        }

        [HttpGet]
        public IActionResult Close(string id)
        {
            var result = IocHelper.Create<PlanManage>().Close(id);
            return new JsonResult(result);
        }
    }
}