#if !NETSTANDARD2_0
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yizuan.Service.Api.WebApi;

namespace Yizuan2.Service.Api.WebApi
{
    /// <summary>
    /// 跨域支持
    /// </summary>
    internal sealed class CorsHandler : IHttpSystemHandler
    {
        /// <summary>
        ///     开始时的处理
        /// </summary>
        /// <returns>如果返回内容不为空，直接返回,后续的处理不再继续</returns>
        Task<HttpResponseMessage> IHttpSystemHandler.OnBegin(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (HttpMethod.Options != request.Method)
                return null;

            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                //list = request.Headers.GetValues("Origin");
                //result.Headers.Add("Access-Control-Allow-Origin", "*");
                result.Headers.Add("Access-Control-Allow-Methods", new[] {"GET", "POST" });
                result.Headers.Add("Access-Control-Allow-Headers",
                    new[] {"x-requested-with", "content-type", "authorization", "*"});
                return result;
            }, cancellationToken);
        }

        /// <summary>
        ///     结束时的处理
        /// </summary>
        void IHttpSystemHandler.OnEnd(HttpRequestMessage request, CancellationToken cancellationToken,HttpResponseMessage response)
        {
            //list = request.Headers.GetValues("Origin");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            //result.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST", "OPTION" });
            //result.Headers.Add("Access-Control-Allow-Headers", new[] { "x-requested-with", "content-type", "authorization", "*" });
        }
    }
}
# endif