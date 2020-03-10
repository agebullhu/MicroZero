using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router : IRouter
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
        async Task<bool> IRouter.Prepare(HttpContext context)
        {
            LogRecorder.MonitorTrace("ApiRouter");
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData();
            var success = await Data.Prepare(HttpContext);
            IoHandler.OnBegin(Data);
            if (!success)
            {
                Data.UserState = UserOperatorStateType.LogicalError;
                Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                Data.ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
            SecurityChecker = new SecurityChecker { Data = Data };
            return SecurityChecker.PreCheck();
        }

        /// <summary>
        /// 检查并重新映射（如果可以的话）
        /// </summary>
        /// <param name="map"></param>
        void IRouter.CheckMap(AshxMapConfig map)
        {
            CheckMap(map);
        }

        /// <summary>
        /// 检查并重新映射（如果可以的话）
        /// </summary>
        /// <param name="map"></param>
        protected virtual void CheckMap(AshxMapConfig map)
        {
            if (map == null)
                return;
            var aPath = Request.GetUri().AbsolutePath;
            foreach (var model in map.Models)
            {
                foreach (var path in model.Paths)
                {
                    if (path.Name.IndexOf(aPath, StringComparison.OrdinalIgnoreCase) != 0)
                        continue;
                    StringBuilder name = new StringBuilder();
                    name.Append(path.Value);
                    name.Append('/');
                    if (aPath.Length > path.Name.Length)
                    {
                        var paths = aPath.Substring(aPath.Length).Split('/', '\\');
                        int idx = 0;
                        for (; idx < paths.Length - 1; idx++)
                        {
                            name.Append(paths[idx]);
                            name.Append('/');
                        }
                        name.Append(paths[idx].Split('.')[1]);
                        name.Append('/');
                    }
                    Data.ApiName = $"{name}{Data.Arguments[map.Action]}";
                    Data.ApiHost = model.Station;
                    return;
                }
            }
        }

        /// <summary>
        ///     调用
        /// </summary>
        async Task IRouter.Call()
        {
            if (Data.ApiHost.Equals("Zero", StringComparison.OrdinalIgnoreCase))
            {
                var manager = new ZeroManager();
                manager.Command(Data);
                return;
            }
            // 1 初始化路由信息
            if (!FindHost())
            {
                Data.UserState = UserOperatorStateType.NotFind;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                Data.ResultMessage = ApiResultIoc.NoFindJson;

                return;
            }

            // 2 缓存快速处理
            if (await RouteCache.LoadCache(Data))
            {
                //找到并返回缓存
                Data.UserState = UserOperatorStateType.Success;
                Data.ZeroState = ZeroOperatorStateType.Ok;
                return;
            }

            //3 超限熔断
            if (RouteHost.WaitCount > ZeroApplication.Config.MaxWait)
            {
                Data.UserState = UserOperatorStateType.Unavailable;
                Data.ZeroState = ZeroOperatorStateType.Unavailable;
                Data.ResultMessage = ApiResultIoc.UnavailableJson;
                return;
            }
            try
            {
                // 3 安全检查
                Interlocked.Increment(ref RouteHost.WaitCount);
                if (!TokenCheck())
                {
                    Data.UserState = UserOperatorStateType.DenyAccess;
                    Data.ZeroState = ZeroOperatorStateType.DenyAccess;
                    if (Data.ResultMessage == null)
                        Data.ResultMessage = ApiResultIoc.DenyAccessJson;
                    return;
                }

                // 4 远程调用
                if (!Data.RouteHost.ByZero)
                    Data.ResultMessage = await CallHttp();
                else if (ZeroApplication.ZerCenterIsRun)
                    Data.ResultMessage = await CallZero();
                else
                {
                    Data.UserState = UserOperatorStateType.NotFind;
                    Data.ZeroState = ZeroOperatorStateType.NotFind;
                    Data.ResultMessage = ApiResultIoc.NoFindJson;
                    return;
                }
            }
            finally
            {
                Interlocked.Decrement(ref RouteHost.WaitCount);
            }
            // 5 结果检查
            ResultChecker.DoCheck(Data);
            // 6 缓存结果
            RouteCache.CacheResult(Data);
        }


        /// <summary>
        ///     查找站点
        /// </summary>
        private bool FindHost()
        {
            if (!GatewayOption.Option.RouteMaps.TryGetValue(Data.ApiHost, out Data.RouteHost) || Data.RouteHost == null)
            {
                LogRecorder.MonitorTrace(() => $"{Data.ApiHost} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            //if(Data.RouteHost.Failed)
            //{
            //    LogRecorder.MonitorTrace($"{Data.HostName} is failed({Data.RouteHost.Description})");
            //    return false; //Data.RouteHost = HttpHost.DefaultHost;
            //}
            if (!GatewayOption.Option.SystemConfig.CheckApiItem || !(Data.RouteHost is ZeroHost host))
                return true;
            if (host.Apis == null || !host.Apis.TryGetValue(Data.ApiName, out Data.ApiItem))
            {
                LogRecorder.MonitorTrace(() => $"{Data.ApiHost}/{Data.ApiName} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            if (Data.ApiItem.Access == ApiAccessOption.None || Data.ApiItem.Access.HasFlag(ApiAccessOption.Public))
                return true;
            LogRecorder.MonitorTrace(() => $"{Data.ApiHost}/{Data.ApiName} deny access.");
            return false;
        }

        /// <summary>
        ///     安全检查
        /// </summary>
        private bool TokenCheck()
        {
            //if (SecurityChecker.CheckToken())
            //    return true;
            //Data.ResultMessage = GatewayOption.Option.Security.BlockHost;
            //Response.Redirect(GatewayOption.Option.Security.BlockHost, false);
            //Data.Redirect = true;
            return GatewayOption.Option.Security.Auth2
                ? SecurityChecker.CheckToken2()
                : SecurityChecker.CheckToken();
        }


        /// <summary>
        ///     写入返回
        /// </summary>
        Task IRouter.WriteResult()
        {
            return WriteResult();
        }

        /// <summary>
        ///     写入返回
        /// </summary>
        protected virtual async Task WriteResult()
        {
            //if (Data.Redirect)
            //    return;
            //// 缓存
            //RouteCache.CacheResult(Data);
            //if (!string.IsNullOrWhiteSpace(Data.CacheKey))
            //    Response.Headers.Add("Etag", Data.CacheKey);
            if (!Data.IsFile || Data.ResultBinary == null || Data.ResultBinary.Length == 0)
            {
                await Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResultIoc.RemoteEmptyErrorJson), Encoding.UTF8);
                return;
            }

            var file = JsonHelper.DeserializeObject<ApiFileResult>(Data.ResultMessage);
            Response.Headers["ContentLength"] = Data.ResultBinary.Length.ToString();
            Response.ContentType = file.Mime;
            Response.Headers.Add("Content-Disposition", $"attachment;filename={file.FileName}");
            await Response.Body.WriteAsync(Data.ResultBinary);
        }

        /// <summary>
        ///     结束
        /// </summary>
        void IRouter.End() => IoHandler.OnEnd(Data);

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        async Task IRouter.OnError(Exception e, HttpContext context)
        {
            try
            {
                LogRecorder.MonitorTrace(e.Message);
                Data.UserState = UserOperatorStateType.LocalException;
                Data.ZeroState = ZeroOperatorStateType.LocalException;
                ZeroTrace.WriteException("Route", e);
                ////IocHelper.Create<IRuntimeWaring>()?.Waring("Route", Data.Uri.LocalPath, e.Message);
                await context.Response.WriteAsync(ApiResultIoc.LocalErrorJson, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

        #endregion
    }
}