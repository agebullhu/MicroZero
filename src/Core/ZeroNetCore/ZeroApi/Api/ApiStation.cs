using Agebull.EntityModel.Common;
using ZeroMQ;


namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiStation : ApiStationBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ApiStation() : base(ZeroStationType.Api, true)
        {

        }

        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected override IZmqPool PrepareLoop()
        {
            var pSocket = ZeroApplication.Config.ApiRouterModel
                    ? ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.DEALER, Identity)
                    : ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.PULL, Identity);
            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In,
                ZSocketEx.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, Identity),
                pSocket,
                ZSocketEx.CreateServiceSocket(InprocAddress, null, ZSocketType.ROUTER));
            return pool;
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            if (option.SpeedLimitModel == SpeedLimitType.None)
                option.SpeedLimitModel = SpeedLimitType.ThreadCount;
            return option;
        }



    }
}