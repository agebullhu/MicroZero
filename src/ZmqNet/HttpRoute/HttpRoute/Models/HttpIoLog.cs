using System;
using System.IO;
using System.Linq;
using System.Text;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     Http进站出站的日志记录
    /// </summary>
    internal sealed class HttpIoLog
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        public static int OnBegin(RouteData data)
        {
            if (!LogRecorder.LogMonitor)
                return 0;
            try
            {
                LogRecorder.BeginMonitor(data.Uri.ToString());
                LogRecorder.BeginStepMonitor("HTTP");
                var args = new StringBuilder();
                args.Append("Headers：");
                foreach (var head in data.Headers)
                {
                    args.Append($"【{head.Key}】{head.Value.LinkToString('|')}");
                }
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{data.HttpMethod}");
                if (!string.IsNullOrWhiteSpace(data.QueryString))
                {
                    LogRecorder.MonitorTrace($"Query：{data.QueryString}");
                }
                if (!string.IsNullOrWhiteSpace(data.Form))
                {
                    LogRecorder.MonitorTrace($"Form：{data.Form}");
                }
                if (!string.IsNullOrWhiteSpace(data.Context))
                {
                    LogRecorder.MonitorTrace("Context:" + data.Context);
                }
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
            return 0;
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        public static void OnEnd(RouteData data)
        {
            if (!LogRecorder.LogMonitor)
                return;
            
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

    }
}