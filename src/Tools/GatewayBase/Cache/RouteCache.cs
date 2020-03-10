using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.MicroZero;
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
        internal static async Task<bool> LoadCache(RouteData data)
        {
            var api = $"{data.ApiHost}/{data.ApiName}";
            if (!CacheMap.TryGetValue(api, out data.CacheSetting))
            {
                data.CacheKey = null;
                return false;
            }
            var kb = new StringBuilder();
            kb.Append(api);
            kb.Append('?');
            if (data.CacheSetting.Feature.HasFlag(CacheFeature.Keys))
            {
                foreach (var key in data.CacheSetting.Keys)
                {
                    if (data.Arguments.TryGetValue(key, out var value))
                        kb.Append($"{key}={value}&");
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        var dic = JsonHelper.DeserializeObject<Dictionary<string, string>>(data.HttpContent);

                        foreach (var key in data.CacheSetting.Keys)
                        {
                            if (data.Arguments.TryGetValue(key, out var value))
                                kb.Append($"{key}={value}&");
                        }
                    }
                }
                catch// (Exception)
                {
                }
            }
            else
            {
                if (data.CacheSetting.Feature.HasFlag(CacheFeature.Bear))
                {
                    kb.Append($"token={data.Token}&");
                }
                if (data.CacheSetting.Feature.HasFlag(CacheFeature.QueryString))
                {
                    foreach (var kv in data.Arguments)
                    {
                        kb.Append($"{kv.Key}={kv.Value}&");
                    }
                    if (!string.IsNullOrWhiteSpace(data.HttpContent))
                    {
                        kb.Append(data.HttpContent);
                    }
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
                LogRecorder.MonitorTrace(() => $"Cache Load {data.CacheKey}");
                return false;
            }
            if (cacheData.Success && (cacheData.UpdateTime > DateTime.Now || cacheData.IsLoading > 0))
            {
                data.ResultMessage = cacheData.Content;
                LogRecorder.MonitorTrace(() => $"Cache by {data.CacheKey}");
                return true;
            }
            //一个载入，其它的等待调用成功
            if (Interlocked.Increment(ref cacheData.IsLoading) == 1)
            {
                LogRecorder.MonitorTrace(() => $"Cache update {data.CacheKey}");
                return false;
            }
            Interlocked.Decrement(ref cacheData.IsLoading);
            //等待调用成功
            LogRecorder.MonitorTrace(() => $"Cache wait {data.CacheKey}");
            var task = new TaskCompletionSource<string>();
            cacheData.Waits.Add(task);
            data.ResultMessage = await task.Task;
            return true;
        }

        /// <summary>
        ///     缓存返回值
        /// </summary>
        /// <param name="data"></param>
        internal static void CacheResult(RouteData data)
        {
            if (data.CacheSetting != null && data.CacheKey != null)
            {
                if (Cache.TryGetValue(data.CacheKey, out var cd))
                {
                    var res = data.IsSucceed ? data.ResultMessage : cd.Content;
                    foreach (var task in cd.Waits.ToArray())
                    {
                        task.TrySetResult(res);
                    }
                }
                if (!data.IsSucceed && !data.CacheSetting.Feature.HasFlag(CacheFeature.NetError))
                    return;
                Cache[data.CacheKey] = new CacheData
                {
                    Content = data.ResultMessage,
                    Success = data.IsSucceed,
                    IsLoading = 0,
                    UpdateTime = DateTime.Now.AddSeconds(data.CacheSetting.FlushSecond)
                };
                LogRecorder.MonitorTrace($"Cache succeed {data.CacheKey}");
            }
            if (data.IsSucceed && UpdateMap.TryGetValue($"{data.ApiHost}/{data.ApiName}", out var uc))
            {
                var kb = new StringBuilder();
                kb.Append(uc.CacheApi);
                kb.Append('?');

                foreach (var map in uc.Map)
                {
                    if (data.Arguments.TryGetValue(map.Key, out var value))
                        kb.Append($"{map.Value}={value}&");
                }

                //var jObject = (JObject)JsonConvert.DeserializeObject(data.ResultMessage);

                //foreach (var map in uc.Map)
                //{
                //    if (jObject.TryGetValue(map.Key, out var value))
                //        kb.Append($"{map.Value}={value}&");
                //}
                RemoveCache(kb.ToString());
            }
        }
        public static void RemoveCache(string key)
        {
            if (Cache.TryRemove(key, out var data))
            {
                LogRecorder.MonitorTrace($"Cache remove {key}");
                foreach (var task in data.Waits.ToArray())
                {
                    task.TrySetResult(data.Content);
                }
            }
        }
        #endregion

        #region 配置

        /// <summary>
        ///     路由配置
        /// </summary>
        public static Dictionary<string, ApiCacheOption> CacheMap { get; set; }

        /// <summary>
        ///     路由配置
        /// </summary>
        public static Dictionary<string, CacheFlushOption> UpdateMap { get; set; }


        /// <summary>
        ///     初始化路由
        /// </summary>
        /// <returns></returns>
        public static void InitCache()
        {
            Cache.Clear();
            CacheMap = new Dictionary<string, ApiCacheOption>(StringComparer.OrdinalIgnoreCase);
            if (GatewayOption.Option.CacheSettings?.Api != null)
                foreach (var setting in GatewayOption.Option.CacheSettings.Api)
                {
                    setting.Initialize();
                    if (!CacheMap.ContainsKey(setting.Api))
                        CacheMap.Add(setting.Api, setting);
                    else
                        CacheMap[setting.Api] = setting;
                }

            UpdateMap = new Dictionary<string, CacheFlushOption>(StringComparer.OrdinalIgnoreCase);
            if (GatewayOption.Option.CacheSettings?.Trigger != null)
                foreach (var setting in GatewayOption.Option.CacheSettings.Trigger)
                {
                    if (!UpdateMap.ContainsKey(setting.TriggerApi))
                        UpdateMap.Add(setting.TriggerApi, setting);
                    else
                        UpdateMap[setting.TriggerApi] = setting;
                }
        }

        #endregion
    }
}