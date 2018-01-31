using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
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
        public static int OnBegin(HttpRequest request)
        {
            if (AppConfig.Config.SystemConfig.FireZero)
                Task.Factory.StartNew(() =>
                {
                    Agebull.ZeroNet.Core.Publisher.Publish("RouteMonitor", "Call", request.GetUri().PathAndQuery);
                });
            if (!LogRecorder.LogMonitor)
                return 0;
            LogRecorder.BeginMonitor(request.Path.ToString());
            LogRecorder.BeginStepMonitor("HTTP");
            try
            {
                var args = new StringBuilder();
                args.Append("Headers：");
                foreach (var head in request.Headers)
                    args.Append($"【{head.Key}】{head.Value.LinkToString('|')}");
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{request.Method}");
                LogRecorder.MonitorTrace($"Query：{request.QueryString}");
                if (request.ContentLength.HasValue && request.ContentLength > 0)
                    LogRecorder.MonitorTrace($"Form：{request.Form.LinkToString("&")}");
            }
            catch (Exception e)
            {
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
        public static void OnEnd(RouteStatus status, string result)
        {
            if (AppConfig.Config.SystemConfig.FireZero)
                Task.Factory.StartNew(() =>
                {
                    Agebull.ZeroNet.Core.Publisher.Publish("RouteMonitor", "Result", result);
                });
            if (!LogRecorder.LogMonitor)
                return;
            try
            {
                LogRecorder.MonitorTrace($"Result：{result}");
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            finally
            {
                LogRecorder.EndMonitor();
            }
        }

    }
}