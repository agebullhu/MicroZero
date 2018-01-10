using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ExternalStation.Models
{
    /// <summary>
    /// 路由主机
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    internal class RouteHost
    {
        /// <summary>
        /// 使用ZeroNet通讯吗
        /// </summary>
        [DataMember, JsonProperty("zero", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ByZero { get; set; }

        /// <summary>
        /// 下一次命中的主机
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public int Next { get; set; }

        /// <summary>
        /// 主机列表
        /// </summary>
        [DataMember, JsonProperty("hosts", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Hosts { get; set; }
        /// <summary>
        /// 别名
        /// </summary>
        [DataMember, JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Alias { get; set; }
        
    }

    /// <summary>
    /// 路由相关的数据
    /// </summary>
    public class RouteData
    {

        /// <summary>
        /// 当前服务器Key
        /// </summary>
        internal static string ServiceKey;
        /// <summary>
        /// 主机查找图
        /// </summary>
        internal static Dictionary<string, RouteHost> HostMap;

        /// <summary>
        /// 缓存图
        /// </summary>
        internal static Dictionary<string, CacheSetting> CacheMap;

        /// <summary>
        /// 默认主机
        /// </summary>
        internal static RouteHost DefaultHostData;
        /// <summary>
        /// 缓存数据
        /// </summary>
        internal static Dictionary<string, CacheData> Cache;


        /// <summary>
        /// 刷新
        /// </summary>
        public static void Flush()
        {
            ServiceKey = ConfigurationManager.AppSettings["ServiceKey"] ?? "HttpRouter";

            var rootPath = Startup.Configuration["contentRoot"];
            var file = Path.Combine(rootPath, "machine.json");
            var json = File.ReadAllText(file);
            var map1 = JsonConvert.DeserializeObject<Dictionary<string, RouteHost>>(json);
            HostMap = new Dictionary<string, RouteHost>(StringComparer.OrdinalIgnoreCase);
            if (map1 != null)
            {
                foreach (var kv in map1)
                {
                    if (kv.Key == "Default")
                    {
                        DefaultHostData = kv.Value;
                    }
                    else
                    {
                        if (!kv.Value.ByZero && (kv.Value.Hosts == null || kv.Value.Hosts.Length == 0))
                            continue;
                        HostMap.Add(kv.Key, kv.Value);
                        if (kv.Value.Alias != null)
                        {
                            foreach(var name in kv.Value.Alias)
                                HostMap.Add(name, kv.Value);
                        }
                    }
                }
            }
            file = Path.Combine(rootPath, "cache.json");
            json = File.ReadAllText(file);
            var cs = JsonConvert.DeserializeObject<List<CacheSetting>>(json);
            CacheMap = new Dictionary<string, CacheSetting>();
            if (cs != null)
            {
                foreach (var c in cs)
                {
                    c.Initialize();
                    CacheMap.Add(c.Api, c);
                }
            }
            Cache = new Dictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);
        }
    }
}