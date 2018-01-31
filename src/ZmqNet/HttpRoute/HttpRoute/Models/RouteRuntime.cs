using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using Aliyun.Net.SDK.Core;
using Aliyun.Net.SDK.Core.Exceptions;
using Aliyun.Net.SDK.Core.Profile;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     路由运行时数据相关的数据
    /// </summary>
    public class RouteRuntime
    {
        #region 默认返回内容

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        internal static readonly string DenyAccess = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.DenyAccess));

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        internal static readonly string DeviceUnknow =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Auth_Device_Unknow));

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        internal static readonly string TokenUnknow =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Auth_AccessToken_Unknow));

        /// <summary>
        ///     操作成功的字符串
        /// </summary>
        internal static readonly string DefaultSuccees = JsonConvert.SerializeObject(ApiResult.Succees());

        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        internal static readonly string RemoteEmptyError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.UnknowError, "服务器无返回值"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        internal static readonly string NetworkError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, "服务器访问异常"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        internal static readonly string InnerError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "系统内部错误"));

        #endregion

        #region 缓存数据

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
            lock (WaringsTime)
                WaringsTime.Clear();
        }

        /// <summary>
        /// 检查缓存
        /// </summary>
        /// <returns>取到缓存，可以直接返回</returns>
        internal static bool CheckCache(Uri uri, string bearer, out CacheSetting setting, out string key, ref string resultMessage)
        {
            if (!AppConfig.Config.CacheMap.TryGetValue(uri.LocalPath, out setting))
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
        /// 缓存数据
        /// </summary>
        internal static void CacheData(RouteData resultData)
        {
            if (resultData.Status == RouteStatus.None)
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<ApiResult>(resultData.ResultMessage);
                    if (!result.Result)//错误数据不缓存
                    {
                        if (result.Status != null && result.Status.ErrorCode >= ErrorCode.Auth_User_Unknow)
                            return;
                        RuntimeWaring(resultData.HostName, resultData.ApiName, result?.Status?.Message ?? "处理错误但无消息");
                        return;
                    }
                }
                catch
                {
                    RuntimeWaring(resultData.HostName, resultData.ApiName, "返回值非法(不是Json格式)");
                    return;
                }
            }
            if (resultData.CacheSetting == null)
                return;
            CacheData cacheData;
            if (resultData.CacheSetting.Feature.HasFlag(CacheFeature.NetError) && resultData.Status == RouteStatus.RemoteError)
            {
                cacheData = new CacheData
                {
                    Content = resultData.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(30)
                };
            }
            else
            {
                cacheData = new CacheData
                {
                    Content = resultData.ResultMessage,
                    UpdateTime = DateTime.Now.AddSeconds(resultData.CacheSetting.FlushSecond)
                };
            }

            lock (resultData.CacheSetting)
            {
                if (!Cache.ContainsKey(resultData.CacheKey))
                    Cache.Add(resultData.CacheKey, cacheData);
                else
                    Cache[resultData.CacheKey] = cacheData;
            }
        }
        #endregion

        #region 运维需求

        /// <summary>
        /// 运行时警告节点
        /// </summary>
        public class RuntimeWaringItem
        {
            /*起止时间*/
            public DateTime StartTime, LastTime;
            /// <summary>
            /// 最后一次发短信时间
            /// </summary>
            public DateTime MessageTime;
            /// <summary>
            /// 发生次数，发送次数，发送后发生次数
            /// </summary>
            public int WaringCount, SendCount, LastCount;
            /// <summary>
            /// 发生问题的API
            /// </summary>
            public List<string> Apis = new List<string>();
        }

        /// <summary>
        /// 所有运行时警告
        /// </summary>
        private static readonly Dictionary<string, RuntimeWaringItem> WaringsTime = new Dictionary<string, RuntimeWaringItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public static void RuntimeWaring(string host, string api, string message)
        {
            if (AppConfig.Config.SmsConfig == null || AppConfig.Config.SmsConfig.CycleHours <= 0)
                return;
            RuntimeWaringItem item;
            lock (WaringsTime)
            {
                if (!WaringsTime.TryGetValue(host, out item))
                {
                    WaringsTime.Add(host, item = new RuntimeWaringItem
                    {
                        StartTime = DateTime.Now,
                        LastTime = DateTime.Now,
                        WaringCount = 1,
                        LastCount = 1
                    });
                }
                else
                {
                    item.LastTime = DateTime.Now;
                    if ((DateTime.Now - item.MessageTime).TotalHours > AppConfig.Config.SmsConfig.CycleHours)
                    {
                        item.SendCount = 0;
                        item.WaringCount = 1;
                        item.LastCount = 1;
                    }
                    else
                    {
                        item.WaringCount += 1;
                        item.LastCount += 1;
                    }
                }

                if (!item.Apis.Contains(api))
                    item.Apis.Add(api);
                //已到最多发送数量阀值
                if (item.SendCount > AppConfig.Config.SmsConfig.CycleSendCount)
                    return;
                //发送频率设置
                if (item.SendCount > 0)
                {
                    if (item.LastCount <= 10)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 30.0)
                            return;
                    }
                    else if (item.LastCount <= 50)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 20.0)
                            return;
                    }
                    else if (item.LastCount <= 200)
                    {
                        if ((DateTime.Now - item.MessageTime).TotalMinutes < 10.0)
                            return;
                    }
                    else if ((DateTime.Now - item.MessageTime).TotalMinutes < 5.0)
                    {
                        return;
                    }
                }
            }
            if (api.Length >= 20)
                api = api.Substring(api.Length - 19, 19);
            if (message.Length >= 20)
                message = message.Substring(20);
            //发送短信
            Console.WriteLine($"服务器{host}的{api}发生${message}错误{item.LastCount}次，请立即处理");
            foreach (var phone in AppConfig.Config.SmsConfig.Phones)
            {
                if (!SendSmsByAli(host, phone, api, message, item.WaringCount))
                    continue;
                item.MessageTime = DateTime.Now;
                item.SendCount++;
                item.LastCount = 0;
            }
        }

        /// <summary>
        /// 阿里短信发送
        /// </summary>
        /// <param name="server"></param>
        /// <param name="PhoneNumber"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        static bool SendSmsByAli(string server, string PhoneNumber, string api, string message, int count)
        {
            IClientProfile profile = DefaultProfile.GetProfile(AppConfig.Config.SmsConfig.AliRegionId, AppConfig.Config.SmsConfig.AliAccessKeyId, AppConfig.Config.SmsConfig.AliAccessKeySecret);
            DefaultProfile.AddEndpoint(AppConfig.Config.SmsConfig.AliEndPointName, AppConfig.Config.SmsConfig.AliRegionId, AppConfig.Config.SmsConfig.AliProduct, AppConfig.Config.SmsConfig.AliDomain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            var request = new SendSmsRequest
            {
                PhoneNumbers = PhoneNumber,
                SignName = AppConfig.Config.SmsConfig.AliSignName,
                TemplateCode = AppConfig.Config.SmsConfig.AliTemplateCode,
                //服务器${server}的${url}发生${message}错误${count}次，请立即处理
                TemplateParam = JsonConvert.SerializeObject(new
                {
                    server,
                    api,
                    message,
                    count
                }),
                OutId = server
            };
            SendSmsResponse sendSmsResponse;
            try
            {
                //请求失败这里会抛ClientException异常
                sendSmsResponse = acsClient.GetAcsResponse(request);
            }
            catch (ServerException e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            catch (ClientException e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return false;
            }
            return sendSmsResponse.Message == "OK";
        }


        #endregion
    }
}