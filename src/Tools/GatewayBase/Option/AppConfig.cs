using System;
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
    [DataContract]
    public class RouteOption
    {
        #region 配置

        public static RouteOption Option { get; set; }


        /// <summary>
        ///     安全设置
        /// </summary>
        [DataMember]
        [JsonProperty("security")]
        public SecurityConfig Security { get; set; }

#pragma warning disable CS0649
        /// <summary>
        ///     缓存配置
        /// </summary>
        [DataMember] [JsonProperty("cache")] internal List<CacheOption> _cacheSettings;

        /// <summary>
        ///     路由配置
        /// </summary>
        [DataMember] [JsonProperty("route")] internal Dictionary<string, HostConfig> _routeConfig;

        /// <summary>
        ///     系统配置
        /// </summary>
        [DataMember]
        [JsonProperty("sysConfig")]
        private SystemConfig _systemConfig;

        /// <summary>
        ///     系统配置
        /// </summary>
        public SystemConfig SystemConfig => _systemConfig ?? (_systemConfig = new SystemConfig());

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

            Option.CheckRouteMap();
            Option.CheckSecurity();
            IsInitialized = true;

            if (RouteMap == null)
                RouteMap = new Dictionary<string, RouteHost>(StringComparer.OrdinalIgnoreCase);
            if (Option.Security == null)
                Option.Security = new SecurityConfig
                {
                    DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                };
            else if (Option.Security.DenyTokens == null)
                Option.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return true;
        }

        /// <summary>
        ///     初始化安全检查
        /// </summary>
        /// <returns></returns>
        private void CheckSecurity()
        {
            Option.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Option.Security.denyTokens != null)
                foreach (var apiItem in Option.Security.denyTokens)
                    Option.Security.DenyTokens.Add(apiItem, apiItem);
        }

        /// <summary>
        ///     缓存图
        /// </summary>
        public static Dictionary<string, RouteHost> RouteMap { get; internal set; }

        /// <summary>
        ///     初始化路由
        /// </summary>
        /// <returns></returns>
        private void CheckRouteMap()
        {
            RouteMap = new Dictionary<string, RouteHost>(StringComparer.OrdinalIgnoreCase);
            if (_routeConfig == null)
                return;

            foreach (var kv in _routeConfig)
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
                    RouteMap.Add(kv.Key, host);
                else
                    RouteMap[kv.Key] = host;
                //别名
                if (kv.Value.Alias == null)
                    continue;
                foreach (var name in kv.Value.Alias)
                    if (!RouteMap.ContainsKey(name))
                        RouteMap.Add(name, host);
                    else
                        RouteMap[name] = host;
            }
        }

        #endregion
    }
}