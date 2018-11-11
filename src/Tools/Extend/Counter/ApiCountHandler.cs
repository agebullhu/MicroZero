using System;
using Agebull.ZeroNet.Core;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    internal sealed class ApiCountHandler : IApiHandler
    {
        private CountData count;
        void IApiHandler.Prepare(ApiStationBase station, ApiCallItem item)
        {
            count = new CountData
            {
                IsInner = true,
                Start = DateTime.Now.Ticks,
                Requester = item.Requester,
                HostName = station.StationName,
                ApiName = item.ApiName
            };
        }

        void IApiHandler.End(ApiStationBase station, ApiCallItem item)
        {
            count.End = DateTime.Now.Ticks;
            count.Status = item.Status;
            //count.FromId = item.CallerGlobalId;
            ApiCounter.Instance.Count(count);
        }
    }
}