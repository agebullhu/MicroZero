using System;
using Agebull.ZeroNet.Core;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    internal sealed class ApiClientCountHandler : ApiClient.IHandler
    {
        private CountData count;
        void ApiClient.IHandler.Prepare(ApiClient item)
        {
            count = new CountData
            {
                Start = DateTime.Now.Ticks,
                FromId = ApiContext.RequestContext.CallGlobalId,
                Requester = ZeroApplication.Config.RealName,
                HostName = item.Station,
                ApiName = item.Commmand
            };
        }

        void ApiClient.IHandler.End(ApiClient item)
        {
            count.ToId = ApiContext.RequestContext.LocalGlobalId;
            count.End = DateTime.Now.Ticks;
            count.Status = item.State.ToOperatorStatus();
            ApiCounter.Instance.Count(count);
        }
    }
}