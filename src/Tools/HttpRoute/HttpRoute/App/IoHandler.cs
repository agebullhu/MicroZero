using System;
using System.Text;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     Http进站出站的处理类
    /// </summary>
    internal sealed class IoHandler
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        public static void OnBegin(RouteData data)
        {
            data.Start = DateTime.Now;
            BeginMonitor(data);
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        public static void OnEnd(RouteData data)
        {
            EndMonitor(data);
            CountApi(data);
        }

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
                if (!string.IsNullOrWhiteSpace(data.QueryString)) LogRecorder.MonitorTrace($"Query：{data.QueryString}");
                if (!string.IsNullOrWhiteSpace(data.Form)) LogRecorder.MonitorTrace($"Form：{data.Form}");
                if (!string.IsNullOrWhiteSpace(data.Context)) LogRecorder.MonitorTrace("Context:" + data.Context);
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
            if (!LogRecorder.LogMonitor) return;
            try
            {
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
            //Interlocked.Increment(ref count);
            OperatorStatus state;
            switch (data.Status)
            {
                case RouteStatus.FormalError:
                    //Interlocked.Increment(ref error);
                    state = OperatorStatus.FormalError;
                    break;
                case RouteStatus.LocalError:
                    //Interlocked.Increment(ref error);
                    state = OperatorStatus.LocalError;
                    break;
                case RouteStatus.RemoteError:
                    //Interlocked.Increment(ref error);
                    state = OperatorStatus.RemoteError;
                    break;
                case RouteStatus.DenyAccess:
                    //Interlocked.Increment(ref error);
                    state = OperatorStatus.DenyAccess;
                    break;
                default:
                    state = OperatorStatus.Success;
                    //Interlocked.Increment(ref success);
                    break;
            }

            counter.Count(new CountData
            {
                Start = data.Start.Ticks,
                End = DateTime.Now.Ticks,
                HostName = data.HostName,
                ApiName = data.ApiName,
                Status = state,
                Requester = $"http_route={ApiContext.RequestContext.Ip}:{ApiContext.RequestContext.Port}"
            });
            //ZeroTrace.WriteLoop("Run", $"count:{count} success{success} error{error}");
        }
    }
}