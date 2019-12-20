using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.ZeroApis;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Models;

namespace WebMonitor.Controler
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(string id)
        {
            if (id == null)
                id = "api";
            await ConfigManager.LoadAllConfig();
            return View(new AnnotationsConfig { Name = id });
        }

        [HttpPost]
        public IActionResult Update([FromForm] StationInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            var old = ZeroApplication.Config[info.Name];
            if (old == null)
                return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            var config = new StationConfig
            {
                Name = info.Name,
                Description = info.Description,
                StationAlias = string.IsNullOrWhiteSpace(info.Alias)
                    ? new List<string>()
                    : info.Alias.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            if (!ZeroApplication.Config.Check(old, config))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "名称存在重复"));

            return Json(ZeroManager.Update(config));
        }

        [HttpPost]
        public async Task<IActionResult> Install([FromForm] StationInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Type))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            ZeroStationType type;
            switch (info.Type.ToLower())
            {
                case "notify":
                    type = ZeroStationType.Notify;
                    break;
                case "queue":
                    type = ZeroStationType.Queue;
                    break;
                case "vote":
                    type = ZeroStationType.Vote;
                    break;
                case "api":
                    type = ZeroStationType.Api;
                    break;
                case "rapi":
                    type = ZeroStationType.RouteApi;
                    break;
                default:
                    return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            }
            var config = new StationConfig
            {
                Name = info.Name.Trim(),
                Description = info.Description,
                StationType = type,
                ShortName = info.ShortName?.Trim() ?? info.Name.Trim(),
                StationAlias = string.IsNullOrWhiteSpace(info.Alias)
                    ? new List<string>()
                    : info.Alias.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            return Json(!ZeroApplication.Config.Check(config, config) ? ApiResult.Error(ErrorCode.LogicalError, "名称存在重复") : await ZeroManager.Install(config));
        }

        [HttpGet]
        public async Task<IActionResult> Pause(string id)
        {
            return Json(await ZeroManager.Command("pause", id));
        }
        [HttpGet]
        public async Task<IActionResult> Remove(string id)
        {
            return Json(await ZeroManager.Command("remove", id));
        }

        [HttpGet]
        public async Task<IActionResult> Resume(string id)
        {
            return Json(await ZeroManager.Command("resume", id));
        }

        [HttpGet]
        public async Task<IActionResult> Stop(string id)
        {
            return Json(await ZeroManager.Command("stop", id));
        }

        [HttpGet]
        public async Task<IActionResult> Recover(string id)
        {
            return Json(await ZeroManager.Command("recover", id));
        }

    }
}