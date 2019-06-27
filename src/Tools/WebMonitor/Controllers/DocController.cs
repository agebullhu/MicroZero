using System;
using System.Linq;
using System.Collections.Generic;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebMonitor.Controllers
{
    public class DocController : Controller
    {
        /// <summary>
        /// 静态文档
        /// </summary>
        private static readonly Dictionary<string, StationDocument> Documents = new Dictionary<string, StationDocument>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Index(string id)
        {
            return View(GetDoc(id));
        }

        StationDocument GetDoc(string station)
        {

            if (Documents.TryGetValue(station, out var doc))
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
            else if (!SystemManager.Instance.LoadDocument(config.StationName, out doc))
            {
                doc = new StationDocument
                {
                    Name = config.Name,
                    Caption = config.Caption,
                    Description = config.Description
                };
            }

            Documents.TryAdd(station, doc);
            return doc;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Update(string id)
        {
            Documents.Clear();
            return Redirect("/Doc/Index/" + id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult Postman(string id)
        {
            var doc = GetDoc(id);
            var postMan = new PostmanScript();
            postMan.Info.Name = doc.Caption ?? doc.Name;
            postMan.Info.Description = doc.Description;
            foreach (var apis in doc.Aips.Values.GroupBy(p=>p.Category ?? "未分类"))
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

    public class PostmanBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

    }
    [JsonObject]
    public class PostmanScript
    {
        [JsonProperty("info")]
        public PostmanInfo Info { get; } = new PostmanInfo();

        [JsonProperty("item")] public List<PostmanFolder> Folder { get; } = new List<PostmanFolder>();

    }

    [JsonObject]
    public class PostmanFolder : PostmanBase
    {
        [JsonProperty("item")] public List<PostmanItem> Items { get; } = new List<PostmanItem>();

    }
    public class PostmanInfo : PostmanBase
    {
        [JsonProperty("_postman_id")]
        public string PostmanId { get; } = Guid.NewGuid().ToString().ToLower();

        [JsonProperty("schema")]
        public string Schema => "https://schema.getpostman.com/json/collection/v2.0.0/collection.json";
    }

    public class PostmanItem : PostmanBase
    {
        [JsonProperty("request")] public PostmanRequest Request { get; } = new PostmanRequest();

        [JsonProperty("response")] public List<string> Response { get; } = new List<string>();
    }

    public class PostmanRequest
    {
        [JsonProperty("auth")] public PostmanRequestAuth Auth { get; } = new PostmanRequestAuth();

        [JsonProperty("method")] public string Method => "POST";

        //[JsonProperty("header")] public PostmanRequestHeader PostmanRequestHeader { get; } = new PostmanRequestHeader();

        [JsonProperty("body")] public PostmanRequestBody Body { get; } = new PostmanRequestBody();

        [JsonProperty("url")] public string Url { get; set; }

    }

    public class PostmanRequestHeader
    {
        [JsonProperty("key")] public string Key => "Content-Type";

        [JsonProperty("name")] public string Name => "Content-Type";

        [JsonProperty("value")] public string Value => "form-data";

        [JsonProperty("type")] public string Type => "text";

    }

    public class PostmanRequestBody
    {
        [JsonProperty("mode")] public string Mode => "formdata";

        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("formdata")]
        public List<PostmanRequestFormData> FormData { get; } = new List<PostmanRequestFormData>();

    }
    public class PostmanRequestFormData
    {
        [JsonProperty("key")] public string Key { get; set; }

        [JsonProperty("value")] public string Value { get; set; } 

        [JsonProperty("type")] public string Type => "text";

        [JsonProperty("description")] public string Description { get; set; }
    }

    public class PostmanRequestAuth
    {
        [JsonProperty("type")] public string Type => "bearer";

        [JsonProperty("bearer")] public PostmanRequestBearer Bearer { get; } = new PostmanRequestBearer();

    }

    public class PostmanRequestBearer
    {
        [JsonProperty("token")] public string Token => "{{token}}";
    }
}