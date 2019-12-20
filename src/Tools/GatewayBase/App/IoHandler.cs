using System;
using System.Text;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;

using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;
using Agebull.Common.Context;
using Agebull.Common.Ioc;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    public sealed class IoHandler
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        public static void OnBegin(RouteData data)
        {
            data.Start = DateTime.Now;
            BeginZeroTrace(data);
            BeginMonitor(data);
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        public static void OnEnd(RouteData data)
        {
            data.End = DateTime.Now;
            EndMonitor(data);
            //ApiCollect(data);
            EndZeroTrace(data);
        }

        #region LogMonitor

        /// <summary>
        ///     开始日志监测
        /// </summary>
        /// <param name="data"></param>
        private static void BeginMonitor(RouteData data)
        {
            if (!LogRecorderX.LogMonitor)
                return;
            try
            {
                LogRecorderX.BeginMonitor(data.Uri.ToString());
                LogRecorderX.BeginStepMonitor("HTTP");
                LogRecorderX.MonitorTrace($"Token     ：{GlobalContext.RequestInfo.Token}");
                LogRecorderX.MonitorTrace($"RequestId ：{GlobalContext.RequestInfo.RequestId }");
                LogRecorderX.MonitorTrace($"Method    ：{data.HttpMethod}");
                LogRecorderX.MonitorTrace($"Command   ：{data.ApiHost}/{data.ApiName}");
                if (data.Headers.Count > 0)
                    LogRecorderX.MonitorTrace($"Headers   ：{JsonHelper.SerializeObject(data.Headers)}");
                if (data.Arguments.Count > 0)
                    LogRecorderX.MonitorTrace($"Arguments ：{JsonHelper.SerializeObject(data.Arguments)}");
                if (!string.IsNullOrWhiteSpace(data.HttpContext))
                    LogRecorderX.MonitorTrace($"Context   ：{data.HttpContext}");
            }
            catch (Exception e)
            {
                LogRecorderX.MonitorTrace(e.Message);
                LogRecorderX.Exception(e);
            }
            finally
            {
                LogRecorderX.EndStepMonitor();
            }
        }

        /// <summary>
        ///     开始计数
        /// </summary>
        /// <returns></returns>
        private static void EndMonitor(RouteData data)
        {
            if (!LogRecorderX.LogMonitor)
                return;
            try
            {
                LogRecorderX.MonitorTrace($"UserState ：{data.UserState}");
                LogRecorderX.MonitorTrace($"ZeroState ：{data.ZeroState}");
                LogRecorderX.MonitorTrace($"Result    ：{data.ResultMessage}");
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

        #region ZeroTrace


        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        private static readonly byte[] Description =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.Station,
            ZeroFrameType.CallId,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End,
            1
        };

        private static readonly byte[] WebNameBytes = "WebClient".ToZeroBytes();


        private static readonly byte[] HttpBytes = "Http".ToZeroBytes();




        /// <summary>
        /// 发送广播
        /// </summary>
        /// <returns></returns>
        static void BeginZeroTrace(RouteData data)
        {
            if (!RouteOption.Option.SystemConfig.EnableLinkTrace)
                return;
            var result = ZeroPublisher.Send("TraceDispatcher", "Http", $"{data.ApiHost}/{data.ApiName}", data);
            if (result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok)
            {
                GlobalContext.Current.Request.LocalGlobalId = result.GlobalId;
            }
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <returns></returns>
        static void EndZeroTrace(RouteData data)
        {
            if (!RouteOption.Option.SystemConfig.EnableLinkTrace)
                return;
            ZeroPublisher.Publish("TraceDispatcher", "Http", $"{data.ApiHost}/{data.ApiName}", data.ResultMessage);
        }


        /*/private static int count, error, success;
        /// <summary>
        ///     开始计数
        /// </summary>
        /// <returns></returns>
        private static void ApiCollect(RouteData data)
        {
            if (!RouteOption.Option.SystemConfig.EnableApiCollect)
                return;
            var counter = IocHelper.Create<IApiCounter>();
            if (counter == null)
                return;
            counter.Count(new CountData
            {
                Start = data.Start.Ticks,
                End = DateTime.Now.Ticks,
                HostName = data.ApiHost,
                ApiName = data.ApiName,
                Status = data.UserState,
                Requester = $"http_route={GlobalContext.RequestInfo.Ip}:{GlobalContext.RequestInfo.Port}"
            });
            //ZeroTrace.WriteLoop("Run", $"count:{count} success{success} error{error}");
        }*/
        #endregion
    }
}