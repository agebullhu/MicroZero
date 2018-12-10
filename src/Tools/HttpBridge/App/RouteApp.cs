using System;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.AspNetCore.Http;
using ZeroNet.Http.Route;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class RouteApp
    {
        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroApplication.NotZeroCenter = true;
            ZeroApplication.CheckOption();
            RouteOption.CheckOption();
            ZeroApplication.Initialize();
            BridgeService.Instance.Run();
        }
#endregion

#region 基本调用

        /// <summary>
        ///     POST调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task Call(HttpContext context)
        {
            return Task.Factory.StartNew(CallTask, context);
        }

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static void CallTask(object arg)
        {
            BridgeService.Instance.Check();
            HttpContext context = (HttpContext)arg;
            var router = new Router(context);
            if (!router.Prepare())
            {
                context.Response.WriteAsync("-error", Encoding.UTF8);
                return;
            }
            //跨域支持
            if (router.Data.HttpMethod == "OPTIONS")
            {
                HttpProtocol.Cros(context.Response);
                return;
            }

            IoHandler.OnBegin(router.Data);
            try
            {
                HttpProtocol.FormatResponse(context.Response);
                // 正常调用
                router.Call();
                // 写入返回
                router.WriteResult();
            }
            catch (Exception e)
            {
                OnError(router, e, context);
            }
            finally
            {
                IoHandler.OnEnd(router.Data);
            }
        }

        private static void OnError(Router router, Exception e, HttpContext context)
        {
            try
            {
                router.Data.Status = ZeroOperatorStatus.LocalException;
                ZeroTrace.WriteException("Route", e);
                IocHelper.Create<IRuntimeWaring>()?.Waring("Route", router.Data.Uri.LocalPath, e.Message);
                context.Response.WriteAsync("发生错误", Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

#endregion

    }
}