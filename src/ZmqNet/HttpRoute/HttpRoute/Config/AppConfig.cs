using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class AppConfig
    {
#pragma warning disable CS0649
        /// <summary>
        /// 缓存配置
        /// </summary>
        [DataMember, JsonProperty("cache")] private List<CacheSetting> _cacheSettings;

        /// <summary>
        /// 路由配置
        /// </summary>
        [DataMember, JsonProperty("route")] private Dictionary<string, HostConfig> _routeConfig;

        /// <summary>
        /// 系统配置
        /// </summary>
        [DataMember, JsonProperty("sysConfig")] private SystemConfig _systemConfig;

#pragma warning restore CS0649
        /// <summary>
        /// 安全设置
        /// </summary>
        [DataMember, JsonProperty("security")]
        public SecurityConfig Security { get; set; }

        public static AppConfig Config { get; set; }


        /// <summary>
        /// 系统配置
        /// </summary>
        public SystemConfig SystemConfig => _systemConfig ?? (_systemConfig = new SystemConfig());

        /// <summary>
        /// 路由配置
        /// </summary>
        public Dictionary<string, CacheSetting> CacheMap { get; set; }

        /// <summary>
        /// 缓存图
        /// </summary>
        public Dictionary<string, HostConfig> RouteMap { get; private set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public static string FileName { get; private set; }
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static bool Initialize(string file)
        {
            FileName = file;
            return Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            if (!File.Exists(FileName))
                return false;
            Config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(FileName));
            if (Config == null)
                return false;
            Config.InitCache();
            Config.InitRoute();
            Config.InitCheckApis();
            IsInitialized = true;
            return HostConfig.DefaultHost != null;
        }
        /// <summary>
        /// 初始化路由
        /// </summary>
        /// <returns></returns>
        public void InitCheckApis()
        {
            Config.Security.CheckApis = new Dictionary<string, ApiItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var apiItem in Config.Security.checkApis)
            {
                Config.Security.CheckApis.Add(apiItem.Key, apiItem.Value);
            }
            Config.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Config.Security.denyTokens != null)
                foreach (var apiItem in Config.Security.denyTokens)
                {
                    Config.Security.DenyTokens.Add(apiItem, apiItem);
                }
        }

        /// <summary>
        /// 初始化路由
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, CacheSetting> InitCache()
        {
            CacheMap = new Dictionary<string, CacheSetting>(StringComparer.OrdinalIgnoreCase);
            if (_cacheSettings == null)
                return CacheMap;
            foreach (var setting in _cacheSettings)
            {
                setting.Initialize();
                if (!CacheMap.ContainsKey(setting.Api))
                    CacheMap.Add(setting.Api, setting);
                else
                    CacheMap[setting.Api] = setting;
            }
            return CacheMap;
        }

        /// <summary>
        /// 初始化路由
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, HostConfig> InitRoute()
        {
            RouteMap = new Dictionary<string, HostConfig>(StringComparer.OrdinalIgnoreCase);
            if (_routeConfig == null)
                return RouteMap;

            foreach (var kv in _routeConfig)
            {
                if (String.Equals(kv.Key, "Default", StringComparison.OrdinalIgnoreCase))
                {
                    HostConfig.DefaultHost = kv.Value;
                    continue;
                }
                if (!kv.Value.ByZero && (kv.Value.Hosts == null || kv.Value.Hosts.Length == 0))
                    continue;
                //Http负载
                if (!RouteMap.ContainsKey(kv.Key))
                    RouteMap.Add(kv.Key, kv.Value);
                else
                    RouteMap[kv.Key] = kv.Value;
                //别名
                if (kv.Value.Alias == null)
                    continue;
                foreach (var name in kv.Value.Alias)
                {
                    if (!RouteMap.ContainsKey(name))
                        RouteMap.Add(name, kv.Value);
                    else
                        RouteMap[name] = kv.Value;
                }
            }

            return RouteMap;
        }

    }
}