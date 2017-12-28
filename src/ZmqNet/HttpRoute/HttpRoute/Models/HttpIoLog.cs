using System;
using System.Linq;
using System.Text;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Http;

namespace Yizuan.Service.Host
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
            if (!LogRecorder.LogMonitor)
                return 0;
            LogRecorder.BeginMonitor(request.Path.ToString());
            try
            {
                var args = new StringBuilder();
                args.Append("Headers：");
                foreach (var head in request.Headers)
                    args.Append($"【{head.Key}】{head.Value.LinkToString('|')}");
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{request.Method}");
                LogRecorder.MonitorTrace($"QueryString：{request.QueryString}");
                if (request.ContentLength.HasValue && request.ContentLength > 0)
                    LogRecorder.MonitorTrace($"Form：{request.Form}");

                //RecordRequestToCode(request);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            return 0;
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        public static void OnEnd(string result)
        {
            if (!LogRecorder.LogMonitor)
                return;
            try
            {
                LogRecorder.MonitorTrace($"Result：{result}");
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Result：{e.Message}");
            }
            LogRecorder.EndMonitor();
        }

    }
}