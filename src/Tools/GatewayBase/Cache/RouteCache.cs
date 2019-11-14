using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroApis;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     缓存
    /// </summary>
    internal class RouteCache
    {
        #region 数据

        /// <summary>
        ///     缓存数据
        /// </summary>
        internal static ConcurrentDictionary<string, CacheData> Cache = new ConcurrentDictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     刷新
        /// </summary>
        internal static void Flush()
        {
            Cache.Clear();
        }

        /// <summary>
        ///     检查缓存
        /// </summary>
        /// <returns>取到缓存，可以直接返回</returns>
        internal static bool LoadCache(RouteData data)
        {
            if (!CacheMap.TryGetValue(data.Uri.LocalPath, out data.CacheSetting))
            {
                data.CacheKey = null;
                return false;
            }
            var kb = new StringBuilder();
            kb.Append(data.Uri.LocalPath);
            if (data.CacheSetting.Feature.HasFlag(CacheFeature.Bear))
            {
                kb.Append(data.Token);
            }
            if (data.CacheSetting.Feature.HasFlag(CacheFeature.QueryString))
            {
                foreach (var kv in data.Arguments)
                {
                    kb.Append($"&{kv.Key}={kv.Value}");
                }
                if (!string.IsNullOrWhiteSpace(data.HttpContext))
                {
                    kb.Append(data.HttpContext);
                }
            }

            data.CacheKey = kb.ToString();
            if (!Cache.TryGetValue(data.CacheKey, out var cacheData))
            {
                Cache.TryAdd(data.CacheKey, new CacheData
                {
                    IsLoading = 1,
                    Content = ApiResultIoc.NoReadyJson
                });
                LogRecorderX.MonitorTrace($"Cache Load {data.CacheKey}");
                return false;
            }
            if (cacheData.Success && (cacheData.UpdateTime > DateTime.Now || cacheData.IsLoading > 0))
            {
                data.ResultMessage = cacheData.Content;
                LogRecorderX.MonitorTrace($"Cache by {data.CacheKey}");
                return true;
            }
            //一个载入，其它的等待调用成功
            if (Interlocked.Increment(ref cacheData.IsLoading) == 0)
            {
                LogRecorderX.MonitorTrace($"Cache update {data.CacheKey}");
                data.CacheKey = null;
                return false;
            }

            //等待调用成功
            int cnt = 0;
            while (++cnt < 10)
            {
                Thread.Sleep(50);
                if (!Cache.TryGetValue(data.CacheKey, out var newData) || cacheData == newData)
                    continue;
                LogRecorderX.MonitorTrace($"Cache wait {data.CacheKey}");
                data.ResultMessage = cacheData.Content;
                return true;
            }
            LogRecorderX.MonitorTrace($"Cache Failed {data.CacheKey}");
            data.CacheKey = null;
            return false;
        }

        /// <summary>
        ///     缓存返回值
        /// </summary>
        /// <param name="data"></param>
        internal static void CacheResult(RouteData data)
        {
            if (data.CacheSetting == null || data.CacheKey == null)
                return;
            if (data.IsSucceed || data.CacheSetting.Feature.HasFlag(CacheFeature.NetError))
            {
                Cache[data.CacheKey]= new CacheData
                {
                    Content = data.ResultMessage,
                    Success = data.IsSucceed,
                    IsLoading = 0,
                    UpdateTime = DateTime.Now.AddSeconds(data.CacheSetting.FlushSecond)
                };
                LogRecorderX.MonitorTrace($"Cache succeed {data.CacheKey}");
            }
        }

        #endregion

        #region 配置

        /// <summary>
        ///     路由配置
        /// </summary>
        public static Dictionary<string, CacheOption> CacheMap { get; set; }


        /// <summary>
        ///     初始化路由
        /// </summary>
        /// <returns></returns>
        public static void InitCache()
        {
            CacheMap = new Dictionary<string, CacheOption>(StringComparer.OrdinalIgnoreCase);
            if (RouteOption.Option.CacheSettings == null)
                return;
            foreach (var setting in RouteOption.Option.CacheSettings)
            {
                setting.Initialize();
                if (!CacheMap.ContainsKey(setting.Api))
                    CacheMap.Add(setting.Api, setting);
                else
                    CacheMap[setting.Api] = setting;
            }
        }

        #endregion
    }
}