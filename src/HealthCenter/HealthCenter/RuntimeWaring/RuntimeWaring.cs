using System;
using System.Collections.Generic;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using Aliyun.Net.SDK.Core;
using Aliyun.Net.SDK.Core.Exceptions;
using Aliyun.Net.SDK.Core.Profile;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     运行时警告
    /// </summary>
    public class RuntimeWaring : SubStation
    {
        /// <summary>
        ///     所有运行时警告
        /// </summary>
        internal static readonly Dictionary<string, RuntimeWaringItem> WaringsTime =
            new Dictionary<string, RuntimeWaringItem>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     构造
        /// </summary>
        public RuntimeWaring()
        {
            Name = "RuntimeWaringService";
            Instance = this;
            StationName = "HealthCenter";
            Subscribe = "RuntimeWaring";
        }

        /// <summary>
        ///     实例
        /// </summary>
        public static RuntimeWaring Instance { get; private set; }

        /// <summary>
        ///     警告短信配置
        /// </summary>
        public SmsConfig SmsConfig { get; set; }

        /// <summary>
        ///     将要开始
        /// </summary>
        protected override bool OnStart()
        {
            try
            {
                SmsConfig = ConfigurationManager.Root.GetSection("RuntimeWaring").Get<SmsConfig>();
                return SmsConfig != null;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("RuntimeWaring", e, "ResetConfig");
                return false;
            }
        }


        /// <inheritdoc />
        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            if (args.SubTitle == "Flush")
            {
                Flush();
                return;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<WaringItem>(args.Content);
                Waring(data.Host, data.Api, data.Message);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("RuntimeWaring", e, args.Content);
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     刷新
        /// </summary>
        internal static void Flush()
        {
            lock (WaringsTime)
            {
                WaringsTime.Clear();
            }
        }

        /// <summary>
        ///     运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public void Waring(string host, string api, string message)
        {
            if (SmsConfig == null || SmsConfig.CycleHours <= 0)
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
                    if (item.MessageTime != DateTime.MinValue &&
                        (DateTime.Now - item.MessageTime).TotalHours > SmsConfig.CycleHours)
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

                if (!item.Apis.ContainsKey(api))
                    item.Apis.Add(api, new List<string> {message});
                else if (!item.Apis[api].Contains(message))
                    item.Apis[api].Add(message);
                //已到最多发送数量阀值
                if (item.SendCount > SmsConfig.CycleSendCount)
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
            if (SmsConfig?.Phones == null)
                return;
            foreach (var phone in SmsConfig.Phones)
            {
                if (!SendSmsByAli(host, phone, api, message, item.WaringCount))
                    continue;
                item.MessageTime = DateTime.Now;
                item.SendCount++;
                item.LastCount = 0;
            }
        }

        /// <summary>
        ///     阿里短信发送
        /// </summary>
        /// <param name="server"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool SendSmsByAli(string server, string phoneNumber, string api, string message, int count)
        {
            IClientProfile profile = DefaultProfile.GetProfile(SmsConfig.AliRegionId, SmsConfig.AliAccessKeyId,
                SmsConfig.AliAccessKeySecret);
            DefaultProfile.AddEndpoint(SmsConfig.AliEndPointName, SmsConfig.AliRegionId, SmsConfig.AliProduct,
                SmsConfig.AliDomain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            var request = new SendSmsRequest
            {
                PhoneNumbers = phoneNumber,
                SignName = SmsConfig.AliSignName,
                TemplateCode = SmsConfig.AliTemplateCode,
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

            return sendSmsResponse.Message == "+ok";
        }
    }
}