using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.ApiDocuments;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Models;

namespace WebMonitor.Controler
{
    public class HomeController : Controller
    {
        public IActionResult Index(string id)
        {
            if (id == null)
                id = "api";
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
        public IActionResult Install([FromForm] StationInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Type))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            ZeroStationType type;
            switch (info.Type.ToLower())
            {
                case "pub":
                    type = ZeroStationType.Publish;
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
                Name = info.Name,
                Description = info.Description,
                StationType = type,
                ShortName = info.short_name ?? info.Name,
                StationAlias = string.IsNullOrWhiteSpace(info.Alias)
                    ? new List<string>()
                    : info.Alias.Trim().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            if (!ZeroApplication.Config.Check(config, config))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "名称存在重复"));

            return Json(ZeroManager.Install(config));
        }

        [HttpGet]
        public IActionResult Pause(string id)
        {
            return Json(ZeroManager.Command("pause", id));
        }
        [HttpGet]
        public IActionResult Remove(string id)
        {
            return Json(ZeroManager.Command("remove", id));
        }

        [HttpGet]
        public IActionResult Resume(string id)
        {
            return Json(ZeroManager.Command("resume", id));
        }

        [HttpGet]
        public IActionResult Stop(string id)
        {
            return Json(ZeroManager.Command("stop", id));
        }

        [HttpGet]
        public IActionResult Recover(string id)
        {
            return Json(ZeroManager.Command("recover", id));
        }

    }
}