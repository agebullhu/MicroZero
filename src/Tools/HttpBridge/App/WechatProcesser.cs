using System;
using System.IO;
using System.Net;
using System.Threading;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;

namespace ZeroNet.Http.Gateway
{
    public class WechatProcesser
    {
        public static WechatConfig Option { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <returns></returns>
        static WechatProcesser()
        {
            var sec = ConfigurationManager.Root.GetSection("Weixin");
            if (!sec.Exists())
                throw new Exception("无法找到配置节点Weixin,在appsettings.json中设置");
            Option = sec.Get<WechatConfig>();
        }

        /// <summary>
        ///     Http返回
        /// </summary>
        private WechatData _data;

        /// <summary>
        ///     异步操作
        /// </summary>
        internal static void Call(WechatData data)
        {
            var process = new WechatProcesser
            {
                _data = data
            };
            process.CallInner();
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal static void CallZero(WechatData data)
        {
            var msg = CustomMessageHandler.GetRequestEntity(data.Context, new PostModel
            {
                Nonce = data["Nonce"],
                Signature = data["Signature"],
                Timestamp = data["Timestamp"],
                Token = Option.Token,
                AppId = Option.WeixinAppId,
                EncodingAESKey = Option.EncodingAESKey,
            });
            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                var caller = new ApiClient
                {
                    Station = "WechatCallBack",
                    Title = msg.MsgType.ToString(),
                    Commmand = "CallBack",
                    Argument = JsonConvert.SerializeObject(msg),
                    ExtendArgument = JsonConvert.SerializeObject(data.Arguments)
                };
                int cnt = 0;
                while (++cnt < 3)
                {
                    caller.CallCommand();
                    LogRecorder.MonitorTrace($"{caller.State } : {caller.Result}");
                    if (caller.State == ZeroOperatorStateType.Ok)
                        return;
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        ///     操作内部
        /// </summary>
        void CallInner()
        {
            try
            {
                var msg = CustomMessageHandler.GetRequestEntity(_data.Context, new PostModel
                {
                    Nonce = _data["Nonce"],
                    Signature = _data["Signature"],
                    Timestamp = _data["Timestamp"],
                    Token = Option.Token,
                    AppId = Option.WeixinAppId,
                    EncodingAESKey = Option.EncodingAESKey,
                });
                //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                var handler = new CustomMessageHandler();
                var res =handler.Handler(msg);
                LogRecorder.MonitorTrace($"Success : {res}");
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex, "Router Call");
                LogRecorder.MonitorTrace($"Exception : {ex.Message }");
            }
        }

        #region WXHttp

        const string wxUrl = "https://api.weixin.qq.com/sns/oauth2/";
        /// <summary>
        ///     写入返回
        /// </summary>
        bool WxLogin(string code)
        {
            try
            {
                var url = $"access_token?appid={Option.WeixinAppId}&secret={Option.WeixinAppSecret}&code={code}&grant_type=authorization_code";
                string json = GoWeixin(url);
                if (string.IsNullOrWhiteSpace(json))
                    return false;
                var auth = JsonConvert.DeserializeObject<WxAuth>(json);
                json = GoWeixin($"userinfo?access_token={auth.AccessToken}&openid={auth.Openid}");
                if (string.IsNullOrWhiteSpace(json))
                    return false;
                var user = JsonConvert.DeserializeObject<WxUser>(json);

                var re = ApiClient.CallApi("UserCenter", "v1/account/registByOpenId", new Argument { Value = auth.Openid });


            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                return false;
            }

            return true;
        }
        /// <summary>
        ///     写入返回
        /// </summary>
        string GoWeixin(string url, int cnt = 0)
        {
            if (cnt >= 3)
                return null;
            try
            {
                var client = new WebClient();
                using (var stream = client.OpenRead(wxUrl + url))
                {
                    using (var txt = new StreamReader(stream))
                    {
                        return txt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                return GoWeixin(url, cnt++);
            }
        }

        #endregion
    }
}