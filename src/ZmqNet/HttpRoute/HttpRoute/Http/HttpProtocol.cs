using Microsoft.AspNetCore.Http;

namespace ZeroNet.Http.Route
{
    public class HttpProtocol
    {
        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void Cros(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", new[] {"GET", "POST"});
            response.Headers.Add("Access-Control-Allow-Headers",
                new[] {"x-requested-with", "content-type", "authorization", "*"});
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        ///     跨域支持
        /// </summary>
        internal static void FormatResponse(HttpResponse response)
        {
            response.ContentType = "text/plain; charset=utf-8";
            response.Headers["Content-Type"] = "text/plain; charset=utf-8";
        }
    }
}