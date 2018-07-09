using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router
    {
        #region Http

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private async Task<string> CallHttp()
        {
            var host = Data.RouteHost as HttpHost;
            if (host == null)
            {
                LogRecorder.MonitorTrace("Host Type Failed");
                return Data.ResultMessage;
            }
            // 当前请求调用的模型对应的主机名称
            string httpHost;

            // 当前请求调用的Api名称
            var httpApi = host == HttpHost.DefaultHost
                ? Data.Uri.PathAndQuery
                : $"{Data.ApiName}{Data.Uri.Query}";

            // 查找主机
            if (host.Hosts.Length == 1)
                httpHost = host.Hosts[0];
            else
                lock (host)
                {
                    //平均分配
                    httpHost = host.Hosts[host.Next];
                    if (++host.Next >= host.Hosts.Length)
                        host.Next = 0;
                }

            // 远程调用
            using (MonitorScope.CreateScope("CallHttp"))
            {
                var caller = new HttpApiCaller(httpHost)
                {
                    Bearer = $"Bearer {ApiContext.RequestContext.Token}"
                };
                caller.CreateRequest(httpApi, Data.HttpMethod, Request, Data);

                Data.ResultMessage = await caller.Call();
                Data.Status = caller.Status;
                LogRecorder.MonitorTrace(caller.Status.ToString());
                LogRecorder.MonitorTrace(Data.ResultMessage);
            }
            return Data.ResultMessage;
        }

        #endregion
    }
}