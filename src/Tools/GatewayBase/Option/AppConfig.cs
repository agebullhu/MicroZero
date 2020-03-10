using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RouteOption
    {
        #region 配置

        /// <summary>
        /// 实例配置
        /// </summary>
        public static RouteOption Option { get; set; }


        /// <summary>
        ///     安全设置
        /// </summary>
        [JsonProperty("security")]
        public SecurityConfig Security { get; set; }


        /// <summary>
        ///     路径映射
        /// </summary>
        public Dictionary<string, AshxMapConfig> UrlMap { get; set; }

#pragma warning disable CS0649

        /// <summary>
        ///     路径映射
        /// </summary>
        [JsonProperty("urlMap")] internal Dictionary<string, AshxMapConfig> UrlMaps;

        /// <summary>
        ///     缓存配置
        /// </summary>
        [DataMember] [JsonProperty("cache")] internal List<ApiCacheOption> CacheSettings;

        /// <summary>
        ///     缓存配置
        /// </summary>
        [DataMember] [JsonProperty("cacheUpdate")] internal List<CacheFlushOption> CacheUpdateSettings;
        
        /// <summary>
        ///     路由配置
        /// </summary>
        [DataMember] [JsonProperty("route")] internal Dictionary<string, HostConfig> RouteConfig;

        /// <summary>
        ///     系统配置
        /// </summary>
        [DataMember]
        [JsonProperty("hostPath")]
        public Dictionary<string, int> HostPath { get; set; }

        /// <summary>
        ///     系统配置
        /// </summary>
        [DataMember]
        [JsonProperty("sysConfig")]
        private SystemConfig _systemConfig;

        /// <summary>
        ///     系统配置
        /// </summary>
        public SystemConfig SystemConfig => _systemConfig ??= new SystemConfig();

#pragma warning restore CS0649

        #endregion

        #region 初始化

        /// <summary>
        ///     文件名称
        /// </summary>
        public static string ConfigFileName { get; set; }

        /// <summary>
        ///     是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <returns></returns>
        public static bool CheckOption()
        {
            ConfigFileName = Path.Combine(ZeroApplication.Config.ConfigFolder, "route_config.json");
            ZeroTrace.SystemLog("HttpGateway", ConfigFileName);
            if (!File.Exists(ConfigFileName))
                throw new Exception($"路由配置文件{ConfigFileName}不存在");
            try
            {
                Option = JsonConvert.DeserializeObject<RouteOption>(File.ReadAllText(ConfigFileName));

            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                throw new Exception($"路由配置文件{ConfigFileName}解析错误");
            }
            if (Option == null)
                throw new Exception($"路由配置文件{ConfigFileName}内容错误");

            if (string.IsNullOrWhiteSpace(Option.SystemConfig.ContentType))
                Option.SystemConfig.ContentType = "text/plain; charset=UTF-8";
            Option.CheckUrlMap();
            Option.CheckRouteMap();
            Option.CheckSecurity();
            IsInitialized = true;

            if (Option.HostPath == null)
            {
                Option.HostPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                var tmp = Option.HostPath;
                Option.HostPath = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in tmp)
                    Option.HostPath.Add(kv.Key, kv.Value);
            }

            if (Option.Security == null)
            {
                Option.Security = new SecurityConfig
                {
                    DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            }
            else if (Option.Security.DenyTokens == null)
            {
                Option.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            return true;
        }

        /// <summary>
        ///     初始化安全检查
        /// </summary>
        /// <returns></returns>
        private void CheckSecurity()
        {
            Option.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Option.Security.denyTokens == null) 
                return;
            foreach (var apiItem in Option.Security.denyTokens)
                Option.Security.DenyTokens.Add(apiItem, apiItem);
        }

        /// <summary>
        ///     缓存图
        /// </summary>
        public static ConcurrentDictionary<string, RouteHost> RouteMap { get; internal set; }

        /// <summary>
        ///     初始化路由
        /// </summary>
        /// <returns></returns>
        private void CheckRouteMap()
        {
            RouteMap = new ConcurrentDictionary<string, RouteHost>(StringComparer.OrdinalIgnoreCase);
            if (RouteConfig == null)
                return;

            foreach (var kv in RouteConfig)
            {
                var host = new HttpHost(kv.Value);
                if (string.Equals(kv.Key, "Default", StringComparison.OrdinalIgnoreCase))
                {
                    HttpHost.DefaultHost = host;
                    continue;
                }

                if (!kv.Value.ByZero && (kv.Value.Hosts == null || kv.Value.Hosts.Length == 0))
                    continue;
                //Http负载
                if (!RouteMap.ContainsKey(kv.Key))
                    RouteMap.TryAdd(kv.Key, host);
                else
                    RouteMap[kv.Key] = host;
                //别名
                if (kv.Value.Alias == null)
                    continue;
                foreach (var name in kv.Value.Alias)
                    if (!RouteMap.ContainsKey(name))
                        RouteMap.TryAdd(name, host);
                    else
                        RouteMap[name] = host;
            }
        }

        /// <summary>
        ///     初始化路径映射
        /// </summary>
        /// <returns></returns>
        private void CheckUrlMap()
        {
            UrlMap = new Dictionary<string, AshxMapConfig>(StringComparer.OrdinalIgnoreCase);
            if (UrlMaps == null)
                return;
            foreach (var kv in UrlMaps)
            {
                UrlMap.Add(kv.Key, kv.Value);
            }
        }

        #endregion
    }
}