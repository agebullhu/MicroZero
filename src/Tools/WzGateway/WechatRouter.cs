using System;
using System.IO;
using System.Text;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZeroNet.Http.Gateway;
using ZeroNet.Http.Route;

namespace Xuhui.Internetpro.WzHealthCardService.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class Router : IRouter
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
        public RouteData Data { get; set; }

        /// <summary>
        ///     安全检查器
        /// </summary>
        public SecurityChecker SecurityChecker { get; set; }

        #endregion

        #region 流程

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        bool IRouter.Prepare(HttpContext context)
        {
            LogRecorder.MonitorTrace("ApiRouter");
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData();
            var success = Data.Prepare(HttpContext);
            IoHandler.OnBegin(Data);
            if (!success)
            {
                Data.UserState = UserOperatorStateType.LogicalError;
                Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }

            ApiArgument argument;
            try
            {
                if (string.IsNullOrWhiteSpace(Data.HttpContext))
                {
                    Data.UserState = UserOperatorStateType.LogicalError;
                    Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    Data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                    return false;
                }
                argument= JsonConvert.DeserializeObject<ApiArgument>(Data.HttpContext);
                if (argument?.Header == null)
                {
                    Data.UserState = UserOperatorStateType.LogicalError;
                    Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    Data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.MonitorTrace(e.Message);

                Data.UserState = UserOperatorStateType.LogicalError;
                Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }

            Data.Token = argument.Header.Token;
            SecurityChecker = new SecurityChecker
            {
                Data = Data,
                Argument = argument
            };
            return SecurityChecker.PreCheck();
        }

        /// <inheritdoc />
        /// <summary>
        ///     调用
        /// </summary>
        void IRouter.Call()
        {
            // 1 初始化路由信息
            if (!FindHost())
            {
                Data.UserState = UserOperatorStateType.NotFind;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                Data.ResultMessage = ApiResultIoc.NoFindJson;
                return;
            }
            // 2 安全检查
            if (!TokenCheck())
            {
                Data.UserState = UserOperatorStateType.DenyAccess;
                Data.ZeroState = ZeroOperatorStateType.DenyAccess;
                if (Data.ResultMessage == null)
                    Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                return;
            }
            // 4 远程调用
            Data.ResultMessage = CallZero();
            // 5 结果检查
            if (RouteOption.Option.SystemConfig.CheckResult)
                SecurityChecker.CheckResult(Data);
        }


        /// <summary>
        ///     查找站点
        /// </summary>
        private bool FindHost()
        {
            if (!RouteOption.RouteMap.TryGetValue(Data.HostName, out Data.RouteHost) || Data.RouteHost == null)
            {
                LogRecorder.MonitorTrace($"{Data.HostName} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            //if(Data.RouteHost.Failed)
            //{
            //    LogRecorder.MonitorTrace($"{Data.HostName} is failed({Data.RouteHost.Description})");
            //    return false; //Data.RouteHost = HttpHost.DefaultHost;
            //}
            if (!RouteOption.Option.SystemConfig.CheckApiItem || !(Data.RouteHost is ZeroHost host))
                return true;
            if (host.Apis == null || !host.Apis.TryGetValue(Data.ApiName, out Data.ApiItem))
            {
                LogRecorder.MonitorTrace($"{Data.HostName}/{Data.ApiName} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            if (Data.ApiItem.Access == ApiAccessOption.None || Data.ApiItem.Access.HasFlag(ApiAccessOption.Public))
                return true;
            LogRecorder.MonitorTrace($"{Data.HostName}/{Data.ApiName} deny access.");
            return false;
        }

        /// <summary>
        ///     安全检查
        /// </summary>
        private bool TokenCheck()
        {
            //if (SecurityChecker.CheckToken())
            //    return true;
            //Data.ResultMessage = RouteOption.Option.Security.BlockHost;
            //Response.Redirect(RouteOption.Option.Security.BlockHost, false);
            //Data.Redirect = true;
            return SecurityChecker.CheckToken();
        }


        /// <summary>
        ///     写入返回
        /// </summary>
        void IRouter.WriteResult()
        {
            //if (Data.Redirect)
            //    return;
            //// 缓存
            //RouteCache.CacheResult(Data);
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResultIoc.RemoteEmptyErrorJson), Encoding.UTF8);
        }

        /// <summary>
        ///     结束
        /// </summary>
        void IRouter.End()
        {
            IoHandler.OnEnd(Data);
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        void IRouter.OnError(Exception e, HttpContext context)
        {
            try
            {
                LogRecorder.MonitorTrace(e.Message);
                Data.UserState = UserOperatorStateType.LocalException;
                Data.ZeroState = ZeroOperatorStateType.LocalException;
                ZeroTrace.WriteException("Route", e);
                IocHelper.Create<IRuntimeWaring>()?.Waring("Route", Data.Uri.LocalPath, e.Message);
                context.Response.WriteAsync(ApiResultIoc.LocalErrorJson, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

        #endregion

        #region 服务调用


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private string CallZero()
        {
            if (!(Data.RouteHost is ZeroHost host))
            {
                LogRecorder.MonitorTrace("Host Type Failed");
                return Data.ResultMessage;
            }

            // 远程调用
            using (MonitorScope.CreateScope("CallZero"))
            {
                return CallApi(host);
            }
        }

        private string CallApi(ZeroHost zeroHost)
        {
            var form = JsonConvert.SerializeObject(Data.Arguments);
            var caller = new ApiClient
            {
                Station = zeroHost.Station,
                Commmand = Data.ApiName,
                Argument = Data.HttpContext ?? form,
                ExtendArgument = form,
                ContextJson = Data.GlobalContextJson
            };
            caller.CallCommand();
            Data.ZeroState = caller.State;
            Data.UserState = caller.State.ToOperatorStatus(true);
            caller.CheckStateResult();
            return Data.ResultMessage = caller.Result;
        }

        #endregion
    }
}