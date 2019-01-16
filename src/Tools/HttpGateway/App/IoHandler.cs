using System;
using System.Text;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using ZeroMQ;

namespace ZeroNet.Http.Gateway
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
            EndMonitor(data);
            //CountApi(data);
            EndZeroTrace(data);
        }

        #region LogMonitor

        /// <summary>
        ///     开始日志监测
        /// </summary>
        /// <param name="data"></param>
        private static void BeginMonitor(RouteData data)
        {
            if (!LogRecorder.LogMonitor)
                return;
            try
            {
                LogRecorder.BeginMonitor(data.Uri.ToString());
                LogRecorder.BeginStepMonitor("HTTP");
                var args = new StringBuilder();
                args.Append("Headers：");
                args.Append(JsonConvert.SerializeObject(data.Headers));
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{data.HttpMethod}");
                LogRecorder.MonitorTrace($"RequestId：{GlobalContext.RequestInfo.RequestId }");
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace(e.Message);
                LogRecorder.Exception(e);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
        }

        /// <summary>
        ///     开始计数
        /// </summary>
        /// <returns></returns>
        private static void EndMonitor(RouteData data)
        {
            if (!LogRecorder.LogMonitor)
                return;
            try
            {
                if (!string.IsNullOrWhiteSpace(data.Uri.Query))
                    LogRecorder.MonitorTrace($"Query：{data.Uri.Query}");
                if (!string.IsNullOrWhiteSpace(data.HttpForm))
                    LogRecorder.MonitorTrace($"Form：{data.HttpForm}");
                if (!string.IsNullOrWhiteSpace(data.HttpContext))
                    LogRecorder.MonitorTrace("Context:" + data.HttpContext);
                LogRecorder.MonitorTrace($"UserState : {data.UserState}");
                LogRecorder.MonitorTrace($"ZeroState : {data.ZeroState}");
                LogRecorder.MonitorTrace($"Result：{data.ResultMessage}");
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace(e.Message);
                LogRecorder.Exception(e);
            }
            finally
            {
                LogRecorder.EndMonitor();
            }
        }

        #endregion

        #region ZeroTrace


        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        private static readonly byte[] Description =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Station,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End,
            1
        };

        private static readonly byte[] begin = "Call".ToZeroBytes();
        private static readonly byte[] web = "WebClient".ToZeroBytes();
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <returns></returns>
        static void BeginZeroTrace(RouteData data)
        {
            var socket = ZeroConnectionPool.GetSocket("TraceDispatcher", RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return;
            }
            using (socket)
            {
                var result = ZeroPublishExtend.Publish(socket.Socket, "Http", Description,
                    begin,
                    data.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.AppName.ToZeroBytes(),
                    web,
                    GlobalContext.ServiceKey.ToZeroBytes());
                if (result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok)
                {
                    GlobalContext.Current.Request.LocalGlobalId = result.GlobalId;
                }
            }
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <returns></returns>
        static void EndZeroTrace(RouteData data)
        {
            var socket = ZeroConnectionPool.GetSocket("TraceDispatcher", RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return;
            }

            var desc = new byte[]
            {
                9,
                (byte)data.ZeroState,
                ZeroFrameType.Command,
                ZeroFrameType.TextContent,
                ZeroFrameType.TextContent,
                ZeroFrameType.RequestId,
                ZeroFrameType.Station,
                ZeroFrameType.Requester,
                ZeroFrameType.GlobalId,
                ZeroFrameType.CallId,
                ZeroFrameType.SerivceKey,
                3
            };
            using (socket)
            {
                ZeroPublishExtend.Publish(socket.Socket, "Http", desc,
                    begin,
                    data.ResultMessage.ToZeroBytes(),
                    data.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.AppName.ToZeroBytes(),
                    web,
                    GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                    GlobalContext.RequestInfo.CallGlobalId.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes());
            }
        }
        #endregion

        //private static int count, error, success;
        /// <summary>
        ///     开始计数
        /// </summary>
        /// <returns></returns>
        private static void CountApi(RouteData data)
        {
            var counter = IocHelper.Create<IApiCounter>();
            if (counter == null || !counter.IsEnable)
                return;
            counter.Count(new CountData
            {
                Start = data.Start.Ticks,
                End = DateTime.Now.Ticks,
                HostName = data.HostName,
                ApiName = data.ApiName,
                Status = data.UserState,
                Requester = $"http_route={GlobalContext.RequestInfo.Ip}:{GlobalContext.RequestInfo.Port}"
            });
            //ZeroTrace.WriteLoop("Run", $"count:{count} success{success} error{error}");
        }
    }
}