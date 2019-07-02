using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebMonitor.Controllers
{
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