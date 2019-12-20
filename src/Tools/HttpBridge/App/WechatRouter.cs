using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Microsoft.AspNetCore.Http;
using Senparc.Weixin.MP;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class WechatRouter : IRouter
    {
        #region 变量

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        ///     Http请求
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public WechatData Data { get; } = new WechatData();

        #endregion

        #region 流程

        /// <summary>
        ///     内部构架
        /// </summary>
        async Task<bool> IRouter.Prepare(HttpContext context)
        {
            LogRecorderX.BeginMonitor("Weixin");
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data.Prepare(HttpContext);
            LogRecorderX.MonitorTrace($"Url:{Data.Uri.PathAndQuery}");
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
                    LogRecorderX.MonitorTrace($"Arguments:{JsonHelper.SerializeObject(Data.Arguments)}");
                }
                if (Request.ContentLength != null)
                {
                    using (var texter = new StreamReader(Request.Body))
                    {
                        Data.Context = await texter.ReadToEndAsync();
                        if (string.IsNullOrEmpty(Data.Context))
                            Data.Context = null;
                        else
                            LogRecorderX.MonitorTrace($"Context:{Data.Context}");
                        texter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e, "读取远程参数");
                LogRecorderX.MonitorTrace($"Exception:{e.Message}");
                return false;
            }


            return true;
        }

        /// <summary>
        ///     调用
        /// </summary>
        async Task IRouter.Call()
        {
            if (!CheckSignature.Check(Data["Signature"], Data["Timestamp"], Data["Nonce"], WechatProcesser.Option.Token))
            {
                Data.ResultMessage = "-argument error";
                return;
            }
            if (Data.HttpMethod == "GET")//GET方式仅为校验之用
            {
                Data.ResultMessage = Data["echostr"]; //返回随机字符串则表示验证通过
                return;
            }
            await WechatProcesser.CallZero(Data);
        }
        string ContentType = "text/plain; charset=utf-8";
        /// <summary>
        ///     写入返回
        /// </summary>
        async Task IRouter.WriteResult()
        {
            Response.ContentType = ContentType;
            await Response.WriteAsync(Data.ResultMessage ?? "");
        }

        /// <summary>
        ///     写入返回
        /// </summary>
        void IRouter.End()
        {
            if (!LogRecorderX.LogMonitor)
                return;
            try
            {
                LogRecorderX.MonitorTrace($"Status : {Data.Status}");
                LogRecorderX.MonitorTrace($"Result：{Data.ResultMessage}");
            }
            catch (Exception e)
            {
                LogRecorderX.MonitorTrace(e.Message);
                LogRecorderX.Exception(e);
            }
            finally
            {
                LogRecorderX.EndMonitor();
            }
        }
        #endregion

        Task IRouter.OnError(Exception e, HttpContext context)
        {
            try
            {
                Data.Status = UserOperatorStateType.LocalException;
                ZeroTrace.WriteException("Route", e);
                //IocHelper.Create<IRuntimeWaring>()?.Waring("Route", Data.Uri.LocalPath, e.Message);
            }
            catch (Exception exception)
            {
                LogRecorderX.Exception(exception);
            }
            try
            {
                context.Response.WriteAsync("", Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorderX.Exception(exception);
            }
            return Task.CompletedTask;
        }

        void IRouter.CheckMap(AshxMapConfig map)
        {

        }
    }
}