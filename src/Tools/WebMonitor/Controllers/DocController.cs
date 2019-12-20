using System.Linq;
using System.Threading.Tasks;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Microsoft.AspNetCore.Mvc;
using WebMonitor.Models;

namespace WebMonitor.Controllers
{
    public class DocController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(string id)
        {
            return View(await GetDoc(id));
        }

        async Task<StationDocument> GetDoc(string station)
        {

            if (GlobalValue.Documents.TryGetValue(station, out var doc))
                return doc;
            if (!ZeroApplication.Config.TryGetConfig(station, out var config))
            {
                config = ZeroApplication.Config.Stations.FirstOrDefault(p => p.StationType == ZeroStationType.Api);
            }

            if (config == null)
            {
                doc = new StationDocument
                {
                    Name = station
                };
            }
            else
            {
                doc = await SystemManager.Instance.LoadDocument(config.StationName) ?? new StationDocument
                {
                    Name = config.Name,
                    Caption = config.Caption,
                    Description = config.Description
                };
            }

            GlobalValue.Documents.TryAdd(station, doc);
            return doc;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Update(string id)
        {
            GlobalValue.Documents.Clear();
            return Redirect("/Doc/Index/" + id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<JsonResult> Postman(string id)
        {
            var doc = await GetDoc(id);
            var postMan = new PostmanScript();
            postMan.Info.Name = doc.Caption ?? doc.Name;
            postMan.Info.Description = doc.Description;
            foreach (var apis in doc.Aips.Values.GroupBy(p => p.Category ?? "未分类"))
            {
                var folder = new PostmanFolder
                {
                    Name = apis.Key
                };
                postMan.Folder.Add(folder);
                foreach (var api in apis)
                {
                    var item = new PostmanItem
                    {
                        Name = api.Caption ?? api.Name,
                        Description = api.Description
                    };
                    folder.Items.Add(item);
                    item.Request.Url = $"http://{{{{url}}}}/{doc.Name}/{api.ApiName}";
                    if (api.ArgumentInfo?.fields == null || api.ArgumentInfo.fields.Count == 0)
                        continue;
                    foreach (var arg in api.ArgumentInfo.Fields.Values)
                    {
                        item.Request.Body.FormData.Add(new PostmanRequestFormData
                        {
                            Key = arg.JsonName ?? arg.Name,
                            Value = arg.Example?.Trim(),
                            Description = $"{arg.Caption}。{arg.Value}"
                        });
                    }
                }
            }
            return Json(postMan);
        }
    }

}