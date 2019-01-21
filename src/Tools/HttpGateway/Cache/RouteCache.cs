using System;
using System.Collections.Generic;
using Agebull.ZeroNet.ZeroApi;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     缓存
    /// </summary>
    internal class RouteCache
    {
        /// <summary>
        ///     缓存数据
        /// </summary>
        internal static Dictionary<string, CacheData> Cache =
            new Dictionary<string, CacheData>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     刷新
        /// </summary>
        internal static void Flush()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
        }

        /// <summary>
        ///     检查缓存
        /// </summary>
        /// <returns>取到缓存，可以直接返回</returns>
        internal static bool LoadCache(Uri uri, string bearer, out CacheOption setting, out string key,
            ref string resultMessage)
        {
            if (!CacheMap.TryGetValue(uri.LocalPath, out setting))
            {
                key = null;
                return false;
            }

            if (setting.Feature.HasFlag(CacheFeature.Bear) && bearer.Substring(0, setting.Bear.Length) != setting.Bear)
            {
                setting = null;
                key = null;
                return false;
            }

            CacheData cacheData;

            lock (setting)
            {
                key = setting.OnlyName ? uri.LocalPath : uri.PathAndQuery;
                if (!Cache.TryGetValue(key, out cacheData))
                    return false;
                if (cacheData.UpdateTime <= DateTime.Now)
                {
                    Cache.Remove(key);
                    return false;
                }
            }

            resultMessage = cacheData.Content;
            return true;
        }

        /// <summary>
        ///     缓存返回值
        /// </summary>
        /// <param name="data"></param>
        internal static void CacheResult(RouteData data)
        {
            if (data.CacheSetting == null || !data.IsSucceed)
                return;
            CacheData cacheData;
            if (data.CacheSetting.Feature.HasFlag(CacheFeature.NetError) &&
                data.UserState == UserOperatorStateType.RemoteError)
                cacheData = new CacheData
                {
                    Content = data.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(30)
                };
            else
                cacheData = new CacheData
                {
                    Content = data.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(data.CacheSetting.FlushSecond)
                };

            lock (data.CacheSetting)
            {
                if (!Cache.ContainsKey(data.CacheKey))
                    Cache.Add(data.CacheKey, cacheData);
                else
                    Cache[data.CacheKey] = cacheData;
            }
        }

        #region 数据

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
            if (RouteOption.Option._cacheSettings == null)
                return;
            foreach (var setting in RouteOption.Option._cacheSettings)
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