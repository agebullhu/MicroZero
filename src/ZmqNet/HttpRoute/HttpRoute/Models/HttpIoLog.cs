using System;
using System.IO;
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
        public static int OnBegin(HttpRequest request, RouteData data)
        {
#if ZERO
            if (AppConfig.Config.SystemConfig.FireZero)
                Task.Factory.StartNew(() =>
                {
                    Agebull.ZeroNet.Core.Publisher.Publish("RouteMonitor", "Call", request.GetUri().PathAndQuery);
                });
#endif
            if (!LogRecorder.LogMonitor)
                return 0;
            try
            {
                LogRecorder.BeginMonitor(request.Path.ToString());
                LogRecorder.BeginStepMonitor("HTTP");
                var args = new StringBuilder();
                args.Append("Headers：");
                foreach (var head in request.Headers)
                    args.Append($"【{head.Key}】{head.Value.LinkToString('|')}");
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{request.Method}");
                if (request.QueryString.HasValue)
                    LogRecorder.MonitorTrace($"Query：{request.QueryString}");
                if (request.HasFormContentType)
                {
                    LogRecorder.MonitorTrace($"Form：{request.Form.LinkToString(p => $"{p.Key}={p.Value}", "&")}");
                }
                else if (request.ContentLength != null)
                {
                    using (var texter = new StreamReader(request.Body))
                    {
                        data.Context = texter.ReadToEnd();
                        LogRecorder.MonitorTrace("Context:" + data.Context);
                    }
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
        public static void OnEnd(RouteStatus status, string result)
        {
#if ZERO
            if (AppConfig.Config.SystemConfig.FireZero)
                Task.Factory.StartNew(() =>
                {
                    Agebull.ZeroNet.Core.Publisher.Publish("RouteMonitor", "Result", result);
                });
#endif
            if (!LogRecorder.LogMonitor)
                return;
            
            try
            {
                LogRecorder.MonitorTrace($"Result：{result}");
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