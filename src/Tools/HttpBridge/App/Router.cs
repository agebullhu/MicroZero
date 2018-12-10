using System;
using System.IO;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MvcExtension;
using Senparc.Weixin.MP.Sample.CommonService.CustomMessageHandler;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router
    {
        #region 变量

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        ///     Http请求
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public HttpResponse Response { get; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public RouteData Data { get; }

        #endregion

        #region 流程

        /// <summary>
        ///     内部构架
        /// </summary>
        /// <param name="context"></param>
        internal Router(HttpContext context)
        {
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData();
        }

        /// <summary>
        ///     内部构架
        /// </summary>
        internal bool Prepare()
        {
            Data.Prepare(HttpContext);
            try
            {
                if (Request.QueryString.HasValue)
                {
                    foreach (var key in Request.Query.Keys)
                        Data.Arguments.TryAdd(key, Request.Query[key]);
                }
                if (Request.HasFormContentType)
                {
                    foreach (var key in Request.Form.Keys)
                        Data.Arguments.TryAdd(key, Request.Form[key]);
                }

                if (Data.Arguments.Count > 0)
                {
                    Data.Form = JsonConvert.SerializeObject(Data.Arguments);
                    LogRecorder.MonitorTrace($"Form:\n{Data.Form}");
                }
                if (Request.ContentLength != null)
                {
                    using (var texter = new StreamReader(Request.Body))
                    {
                        Data.Context = texter.ReadToEnd();
                        if (string.IsNullOrEmpty(Data.Context))
                            Data.Context = null;
                        else
                            LogRecorder.MonitorTrace($"Context:\n{Data.Context}");
                        texter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "读取远程参数");
                return false;
            }


            return true;
        }
        /// <summary>
        ///     调用
        /// </summary>
        internal void Call()
        {
            bool su = CheckSignature.Check(Data["Signature"], Data["Timestamp"], Data["Nonce"], RouteOption.Option.Token);
            if (!su)
            {
                Data.ResultMessage = "-argument error";
                return;
            }
            if (Data.HttpMethod == "GET")//GET方式仅为校验之用
            {
                Data.ResultMessage = Data["echostr"]; //返回随机字符串则表示验证通过
                return;
            }

            #region 打包 PostModel 信息

            PostModel postModel = new PostModel
            {
                Signature = Data["Signature"],
                Timestamp = Data["Timestamp"],
                Nonce = Data["Nonce"],
                Token = RouteOption.Option.Token,
                AppId = RouteOption.Option.WeixinAppId,
                EncodingAESKey = RouteOption.Option.EncodingAESKey
            };

            #endregion

            try
            {
                using (var stream = new MemoryStream(Data.Context.ToUtf8Bytes()))
                {
                    //自定义MessageHandler，对微信请求的详细判断操作都在这里面。
                    var handler = new CustomMessageHandler(stream, postModel, 10);

                    //执行微信处理过程
                    handler.Execute();

                    Data.ResultMessage = new FixWeixinBugWeixinResult(handler.ResultXml).Content;//为了解决官方微信5.0软件换行bug暂时添加的方法，平时用下面一个方法即可
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                Data.ResultMessage = "";
            }
        }

        /// <summary>
        ///     写入返回
        /// </summary>
        internal void WriteResult()
        {
            Response.WriteAsync(Data.ResultMessage ?? "");
        }

        #endregion
    }
}