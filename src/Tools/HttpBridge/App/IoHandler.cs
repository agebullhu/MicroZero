using System;
using System.Text;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
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
                if (!string.IsNullOrWhiteSpace(data.Form))
                    LogRecorder.MonitorTrace($"Form：{data.Form}");
                if (!string.IsNullOrWhiteSpace(data.Context))
                    LogRecorder.MonitorTrace("Context:" + data.Context);
                LogRecorder.MonitorTrace($"Status : {data.Status}");
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
            counter.Count(new CountData
            {
                Start = data.Start.Ticks,
                End = DateTime.Now.Ticks,
                //HostName = data.HostName,
                //ApiName = data.ApiName,
                Status = data.Status,
                Requester = $"http_route={GlobalContext.RequestInfo.Ip}:{GlobalContext.RequestInfo.Port}"
            });
            //ZeroTrace.WriteLoop("Run", $"count:{count} success{success} error{error}");
        }
    }
}