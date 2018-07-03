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
    public class StationController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
                    : info.Alias.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            if (!ZeroApplication.Config.Check(old,config))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "名称存在重复"));
            
            return Json(ZeroManager.Update(config));
        }

        [HttpPost]
        public IActionResult Install([FromForm] StationInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Type))
                return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            int type = 3;
            switch (info.Type.ToLower())
            {
                case "pub":
                    type = ZeroStation.StationTypePublish;
                    break;
                case "vote":
                    type = ZeroStation.StationTypeVote;
                    break;
                case "api":
                    type = ZeroStation.StationTypeApi;
                    break;
                default:
                    return Json(ApiResult.Error(ErrorCode.LogicalError, "参数错误"));
            }
            var config = new StationConfig
            {
                Name = info.Name,
                Description = info.Description,
                StationType = type,
                ShortName = info.ShortName ?? info.Name,
                StationAlias = string.IsNullOrWhiteSpace(info.Alias)
                    ? new List<string>()
                    : info.Alias.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
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
        public IActionResult Resume(string id)
        {
            return Json(ZeroManager.Command("resume", id));
        }

        [HttpGet]
        public IActionResult Uninstall(string id)
        {
            return Json(ZeroManager.Command("uninstall", id));
        }

        [HttpGet]
        public IActionResult Reset(string id)
        {
            return Json(ZeroManager.Command("install", "*", id, "*"));
        }

    }
}