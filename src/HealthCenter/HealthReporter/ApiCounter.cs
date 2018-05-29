using System;
using System.Collections.Generic;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using ZeroNet.Http.Route;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public class ApiCounter : Publisher<CountData>
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ApiCounter Instance = new ApiCounter
        {
            Name = "ApiCounter",
            StationName = "HealthCenter"
        };

        protected override void OnSend(CountData data)
        {
            data.Title = "ApiCounter";
        }

        /// <summary>
        /// 设置Api调用注入
        /// </summary>
        public void HookApi()
        {
            ApiStation.PreActions.Add(OnPre);
            ApiStation.EndActions.Add(OnEnd);
        }

        private readonly Dictionary<string, CountData> _handlers = new Dictionary<string, CountData>();

        private void OnPre(ApiStation station, ApiCallItem item)
        {
            var count = new CountData
            {
                Start = DateTime.Now,
                Machine = ZeroApplication.Config.StationName,
                User = ApiContext.Customer?.Account ?? "Unknow",
                ToId = item.GlobalId,
                RequestId = ApiContext.RequestContext.RequestId,
                Requester = item.Caller,
                HostName = station.StationName,
                ApiName = item.ApiName
            };
            lock (_handlers)
                _handlers.Add(item.GlobalId, count);
        }

        private void OnEnd(ApiStation station,ApiCallItem  item)
        {
            CountData count;
            lock (_handlers)
            {
                if (!_handlers.TryGetValue(item.GlobalId, out count))
                    return;
                _handlers.Remove(item.GlobalId);
            }
            count.End = DateTime.Now;
            count.Status = item.Status;
            count.FromId = item.CallerGlobalId;
            Publish(count);
        }
    }
}