using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;

namespace Agebull.MicroZero.ZeroApis
{

    /// <summary>
    /// 延用MicroZero的GlobalContext与LogMonitor功能的 中间件
    /// </summary>
    public class MicroZeroMiddleware
    {
        /// <summary>
        /// 下一个请求中间件
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="next"></param>
        public MicroZeroMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            using (IocScope.CreateScope(context.Request.Path.Value))
            {
                GlobalContext.SetContext(IocHelper.Create<GlobalContext>());

                var data = new RouteData();
                await data.Prepare(context);
                GlobalContext.Current.DependencyObjects.Annex(data);

                GlobalContext.SetUser(IocHelper.Create<IToken2User>().UserInfo(data.Token));
                GlobalContext.Current.Request.RequestId =
                    context.TraceIdentifier = Guid.NewGuid().ToString("N");

                if (!LogRecorder.LogMonitor)
                {
                    await _next(context);
                    return;
                }
                using (MonitorScope.CreateScope(context.Request.Path.Value))
                {
                    LogRecorder.MonitorTrace(() => data.Arguments.Count == 0 ? null : $"Argument:\r\n{JsonConvert.SerializeObject(data.Arguments)}");
                    LogRecorder.MonitorTrace(() => string.IsNullOrWhiteSpace(data.HttpContent) ? null : $"Content:\r\n{JsonConvert.SerializeObject(data.HttpContent)}");

                    ResponseEnableRewindAsync(context.Response);

                    //context.Response.OnCompleted(ResponseCompletedCallback, context);
                    await _next(context);
                    //var response = await ResponseReadStreamAsync(context.Response);            //记录日志
                    //LogRecorder.MonitorTrace($"Result:\r\n{response}");
                }
            }
        }

        //private async Task ResponseCompletedCallback(object obj)
        //{
        //    if (obj is HttpContext context)
        //    {
        //        var response = await ResponseReadStreamAsync(context.Response);            //记录日志

        //        LogRecorder.MonitorTrace(response);
        //        scope.Dispose();
        //    }

        //}
        private async Task<string> ResponseReadStreamAsync(HttpResponse response)
        {
            if (response.Body.Length <= 0)
            {
                return null;
            }
            var encoding = GetEncoding(response.ContentType);
            string content;
            //这里注意Body部分不能随StreamReader一起释放
            var pos = response.Body.Position;
            response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(response.Body, encoding, true, 1024, true))
            {
                content = await sr.ReadToEndAsync();
            }
            response.Body.Seek(pos, SeekOrigin.Begin);
            return content;
        }

        /// <summary>
        /// 替换response.Body
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private void ResponseEnableRewindAsync(HttpResponse response)
        {
            if (!response.Body.CanRead || !response.Body.CanSeek)
            {
                response.Body = new MemoryWrappedHttpResponseStream(response.Body);
            }
        }

        private Encoding GetEncoding(string contentType)
        {
            var mediaType = contentType == null ? default : new MediaType(contentType);
            var encoding = mediaType.Encoding;
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding;
        }

    }
}