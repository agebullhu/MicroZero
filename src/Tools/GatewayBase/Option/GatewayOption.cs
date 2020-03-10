using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Microsoft.Extensions.Configuration;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由配置
    /// </summary>
    public class GatewayOption
    {
        #region 配置

        /// <summary>
        /// 实例配置
        /// </summary>
        public static GatewayOption Option { get; set; }


        /// <summary>
        ///     安全设置
        /// </summary>
        public SecurityConfig Security { get; set; }


        /// <summary>
        ///     路径映射
        /// </summary>
        public Dictionary<string, AshxMapConfig> UrlMaps { get; set; }

        /// <summary>
        ///     缓存图
        /// </summary>
        public ConcurrentDictionary<string, RouteHost> RouteMaps { get; internal set; }

        /// <summary>
        ///     系统配置
        /// </summary>
        public SystemConfig SystemConfig { get; set; }

        /// <summary>
        ///     系统配置
        /// </summary>
        public Dictionary<string, int> HostPaths { get; set; }

        /// <summary>
        ///     缓存配置
        /// </summary>
        public CacheOption CacheSettings { get; set; }

        #endregion

        #region 初始化

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
            ConfigurationManager.Load(Path.Combine(ZeroApplication.Config.ConfigFolder, "httpGateway.json"), true);
            Option = new GatewayOption();
            ConfigurationManager.RegistOnChange(Option.Load, true);
            RouteCache.InitCache();
            ZeroApplication.RegistZeroObject(new CacheEventProcess());
            IsInitialized = true;
            return true;
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <returns></returns>
        private void Load()
        {
            IConfigurationSection section = ConfigurationManager.Root.GetSection("httpGateway");

            CacheSettings = section.GetSection("cache").Get<CacheOption>();

            SystemConfig = section.GetSection("sysConfig").Get<SystemConfig>();
            if (string.IsNullOrWhiteSpace(Option.SystemConfig.ContentType))
                Option.SystemConfig.ContentType = "text/plain; charset=UTF-8";

            Option.CheckUrlMap(section);
            Option.CheckRouteMap(section);
            Option.CheckSecurity(section);

            RouteCache.Flush();
        }

        /// <summary>
        ///     初始化安全检查
        /// </summary>
        /// <returns></returns>
        private void CheckSecurity(IConfigurationSection section)
        {
            Security = section.GetSection("security").Get<SecurityConfig>();
            Option.Security.DenyTokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Option.Security.denyTokens != null)
            {
                foreach (var apiItem in Option.Security.denyTokens)
                    Option.Security.DenyTokens.Add(apiItem, apiItem);
            }
        }

        /// <summary>
        ///     初始化路径映射
        /// </summary>
        /// <returns></returns>
        private void CheckUrlMap(IConfigurationSection section)
        {
            var urlMap = section.GetSection("urlMap").Get<Dictionary<string, AshxMapConfig>>();
            UrlMaps = new Dictionary<string, AshxMapConfig>(StringComparer.OrdinalIgnoreCase);
            if (urlMap != null)
            {
                foreach (var kv in urlMap)
                {
                    UrlMaps.Add(kv.Key, kv.Value);
                }
            }
            var hostPath = section.GetSection("hostPath").Get<Dictionary<string, int>>();
            HostPaths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (hostPath != null)
            {
                foreach (var kv in hostPath)
                    Option.HostPaths.Add(kv.Key, kv.Value);
            }
        }

        #endregion

        #region 站点信息

        /// <summary>
        ///     初始化路由
        /// </summary>
        /// <returns></returns>
        private void CheckRouteMap(IConfigurationSection section)
        {
            RouteMaps = new ConcurrentDictionary<string, RouteHost>(StringComparer.OrdinalIgnoreCase);
            var route = section.GetSection("route").Get<Dictionary<string, HostConfig>>();
            if (route != null)
                foreach (var kv in route)
                {
                    var host = new HttpHost(kv.Value);
                    if (string.Equals(kv.Key, "Default", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpHost.DefaultHost = host;
                        continue;
                    }

                    if (!kv.Value.Zero && (kv.Value.Hosts == null || kv.Value.Hosts.Length == 0))
                        continue;
                    //Http负载
                    if (!RouteMaps.ContainsKey(kv.Key))
                        RouteMaps.TryAdd(kv.Key, host);
                    else
                        RouteMaps[kv.Key] = host;
                    //别名
                    if (kv.Value.Alias == null)
                        continue;
                    foreach (var name in kv.Value.Alias)
                        if (!RouteMaps.ContainsKey(name))
                            RouteMaps.TryAdd(name, host);
                        else
                            RouteMaps[name] = host;
                }
            Task.Run(MapStation);
        }

        /// <summary>
        /// MapStation
        /// </summary>
        internal void MapStation()
        {
            foreach (var config in ZeroApplication.Config.GetConfigs())
                MapStation(config);
        }

        /// <summary>
        /// MapStation
        /// </summary>
        internal void MapStation(StationConfig station)
        {
            if (station.IsBaseStation)
                return;
            ZeroHost zeroHost;
            if (RouteMaps.TryGetValue(station.StationName, out var h))
            {
                zeroHost = h as ZeroHost;
                if (zeroHost == null)
                    RouteMaps[station.StationName] = zeroHost = new ZeroHost();
            }
            else
            {
                RouteMaps.TryAdd(station.StationName, zeroHost = new ZeroHost());
            }

            zeroHost.Config = station;
            zeroHost.Description = null;
            zeroHost.ByZero = true;
            zeroHost.Failed = false;
            zeroHost.Station = station.StationName;

            if (SystemConfig.CheckApiItem)
                UpdateApiItems(zeroHost);

            if (!string.IsNullOrWhiteSpace(station.ShortName))
            {
                if (RouteMaps.ContainsKey(station.ShortName))
                    RouteMaps[station.ShortName] = zeroHost;
                else
                    RouteMaps.TryAdd(station.ShortName, zeroHost);
            }
            if (station.StationAlias == null)
                return;
            foreach (var alia in station.StationAlias)
                if (RouteMaps.ContainsKey(alia))
                    RouteMaps[alia] = zeroHost;
                else
                    RouteMaps.TryAdd(alia, zeroHost);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zeroHost"></param>
        internal void UpdateApiItems(ZeroHost zeroHost)
        {
            if (zeroHost.Config.IsBaseStation || !zeroHost.Config.IsApi || string.IsNullOrWhiteSpace(zeroHost.Config.StationName))
                return;
            var center = ZeroApplication.Config.ZeroGroup.FirstOrDefault(p => p.Name == zeroHost.Config.Group);
            if (center == null)
            {
                zeroHost.Apis = null;
                return;
            }
            var mg = new ConfigManager(center);
            var doc = mg.LoadDocument(zeroHost.Station);
            if (doc == null)
            {
                zeroHost.Apis = null;
                return;
            }

            if (zeroHost.Apis == null)
                zeroHost.Apis = new Dictionary<string, ApiItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var api in doc.Aips.Values)
                if (zeroHost.Apis.TryGetValue(api.RouteName, out var item))
                {
                    item.App = zeroHost.AppName;
                    item.Access = api.AccessOption;
                }
                else
                {
                    zeroHost.Apis.Add(api.RouteName, new ApiItem
                    {
                        Name = api.RouteName,
                        App = zeroHost.AppName,
                        Access = api.AccessOption
                    });
                }
        }

        internal void OnZeroNetClose()
        {
            foreach (var host in RouteMaps.Where(p => p.Value.ByZero).ToArray())
            {
                host.Value.Failed = true;
                host.Value.Description = "OnZeroNetClose";
            }
        }

        internal void StationLeft(StationConfig station)
        {
            if (RouteMaps.TryGetValue(station.StationName, out var host))
            {
                host.Failed = true;
                host.Description = "StationLeft";
            }
        }

        #endregion
    }
}