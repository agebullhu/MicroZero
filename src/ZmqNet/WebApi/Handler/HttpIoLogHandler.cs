#if !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;

namespace Agebull.ZeroNet.ZeroApi.WebApi
{
    /// <summary>
    ///     Http进站出站的日志记录
    /// </summary>
    internal sealed class HttpIoLogHandler : IHttpSystemHandler
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        Task<HttpResponseMessage> IHttpSystemHandler.OnBegin(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!LogRecorder.LogMonitor)
                return null;
            LogRecorder.BeginMonitor(request.RequestUri.ToString());
            try
            {
                var args = new StringBuilder();
                args.Append("Headers：");
                foreach (var head in request.Headers)
                    args.Append($"【{head.Key}】{head.Value.LinkToString('|')}");
                LogRecorder.MonitorTrace(args.ToString());
                LogRecorder.MonitorTrace($"Method：{request.Method}");

                LogRecorder.MonitorTrace($"QueryString：{request.RequestUri.Query}");

                //RecordRequestToCode(request);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            return null;
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        void IHttpSystemHandler.OnEnd(HttpRequestMessage request, CancellationToken cancellationToken, HttpResponseMessage response)
        {
            if (!LogRecorder.LogMonitor)
                return;
            try
            {
                var task = response.Content.ReadAsStringAsync();
                task.Wait(cancellationToken);
                LogRecorder.MonitorTrace($"Result：{task.Result}");
            }
            catch (Exception e)
            {
                LogRecorder.MonitorTrace($"Result：{e.Message}");
            }
            LogRecorder.EndMonitor();
        }

        /// <summary>
        ///     请求注册为代码
        /// </summary>
        private void RecordRequestToCode(HttpRequestMessage request)
        {
            var code = new StringBuilder();
            if (request.Method == HttpMethod.Get)
            {
                code.Append($@"
                {{
                    caller.Bear = ""{ExtractToken(request)}"";
                    var result = caller.Get/*<>*/(""{request.RequestUri}"");
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }}");
            }
            else
            {
                var task = request.Content.ReadAsStringAsync();
                task.Wait();
                LogRecorder.MonitorTrace($"Content：{task.Result}");
                code.Append($@"
                {{
                    caller.Bear = ""{ExtractToken(request)}"";
                    var result = caller.Post/*<>*/(""{request.RequestUri}"", new Dictionary<string, string>
                    {{");
                var di = FormatParams(task.Result);
                foreach (var item in di)
                    code.Append($@"
                        {{""{item.Key}"",""{item.Value}""}},");
                code.Append(@"
                    });
                    Console.WriteLine(JsonConvert.SerializeObject(result));
                }");
            }
            LogRecorder.Record(code.ToString());
        }


        /// <summary>
        ///     参数格式化
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private Dictionary<string, string> FormatParams(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                return new Dictionary<string, string>();
            var result = new Dictionary<string, string>();
            var kw = args.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            if (kw.Length == 0)
                return result;
            foreach (var item in kw)
            {
                var words = item.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                switch (words.Length)
                {
                    case 0:
                        continue;
                    case 1:
                        result.Add(words[0], null);
                        continue;
                    default:
                        result.Add(words[0], words[1]);
                        continue;
                }
            }
            return result;
        }

        /// <summary>
        ///     取请求头的身份验证令牌
        /// </summary>
        /// <returns></returns>
        private string ExtractToken(HttpRequestMessage request)
        {
            const string bearer = "Bearer";
            var authz = request.Headers.Authorization;
            if (authz != null)
                return string.Equals(authz.Scheme, bearer, StringComparison.OrdinalIgnoreCase) ? authz.Parameter : null;
            if (!request.Headers.Contains("Authorization"))
                return null;
            var au = request.Headers.GetValues("Authorization").FirstOrDefault();
            if (au == null)
                return null;
            var aus = au.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (aus.Length < 2 || aus[0] != bearer)
                return null;
            return aus[1];
        }
    }
}
# endif