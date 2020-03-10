using System.Linq;
using Agebull.Common.Configuration;
using Microsoft.AspNetCore.Http;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// HTTP协议相关的支持
    /// </summary>
    public class HttpProtocol
    {
        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosOption(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST" });
            response.Headers.Add("Access-Control-Allow-Headers", new[] { "x-requested-with", "content-type", "authorization", "*" });
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void CrosCall(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     返回类型
        /// </summary>
        internal static void FormatResponse(HttpRequest request, HttpResponse response)
        {
            if (GatewayOption.Option.SystemConfig.IsTest && request.Headers["USER-AGENT"].LinkToString("|")?.IndexOf("PostmanRuntime") == 0)
                response.Headers["Content-Type"] = response.ContentType = "application/json; charset=UTF-8";
            else
                response.Headers["Content-Type"] = response.ContentType = GatewayOption.Option.SystemConfig.ContentType;
        }
    }
}