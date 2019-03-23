using Agebull.Common.Tson;
using Agebull.MicroZero.Log;

namespace Agebull.MicroZero.ZeroApis
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

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnInitialize()
        {
            HookApi();
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

        protected override bool Prepare()
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
            ApiStationBase.RegistHandlers<ApiCountHandler>();
            ApiClient.RegistHandlers<ApiClientCountHandler>();
            ZeroTrace.SystemLog("ApiCounter", "HookApi");
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <param name="data"></param>
        public void Count(CountData data)
        {
            data.Machine = ZeroApplication.Config.ServiceName;
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