using System;
using System.Collections.Generic;
using Agebull.Common.Configuration;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.Log;
using Agebull.ZeroNet.PubSub;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    internal sealed class ApiCounter : BatchPublisher<CountData>, IApiCounter
    {
        private ApiCounter()
        {
            Name = "ApiCounter";
            StationName = "HealthCenter";
            ZeroApplication.RegistZeroObject(this);
            TsonOperator = new CountDataTsonOperator();
        }

        protected override void Initialize()
        {
            base.Initialize();
            if (ConfigurationManager.AppSettings.GetBool("HookApi", true))
            {
                HookApi();
            }
        }

        /// <summary>
        /// 实例
        /// </summary>
        public static readonly ApiCounter Instance = new ApiCounter();

        /// <summary>
        /// 数据进入的处理
        /// </summary>
        protected override void OnSend(CountData[] datas)
        {
            foreach (var data in datas)
            {
                data.Title = "ApiCounter";
            }
        }

        protected override bool OnStart()
        {
            return _isEnable;
        }
        /// <summary>
        /// 设置Api调用注入
        /// </summary>
        public void HookApi()
        {
            if (_isEnable)
                return;
            _isEnable = true;
            ApiStation.RegistHandlers<ApiCountHandler>();
            ApiClient.RegistHandlers<ApiClientCountHandler>();
            ZeroTrace.WriteInfo("ApiCounter", "HookApi");
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="data"></param>
        public void Count(CountData data)
        {
            data.Machine = ZeroApplication.Config.ServiceKey ??  ZeroApplication.Config.ServiceName;
            data.Station = ZeroApplication.Config.StationName;
            //data.User = ApiContext.Customer?.Account ?? "Unknow";
            //data.RequestId = ApiContext.RequestContext.RequestId;
            Publish(data);
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnable;

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IApiCounter.IsEnable => _isEnable;

    }
}