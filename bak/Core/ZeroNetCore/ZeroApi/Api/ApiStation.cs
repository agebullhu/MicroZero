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
        protected override IZmqPool PrepareLoop(byte[] identity, out ZSocketEx socket)
        {
            socket = ZSocketEx.CreatePoolSocket(Config.WorkerResultAddress, Config.ServiceKey, ZSocketType.DEALER, identity);
            var pSocket = ZeroApplication.Config.ApiRouterModel
                    ? ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.DEALER, identity)
                    : ZSocketEx.CreatePoolSocket(Config.WorkerCallAddress, Config.ServiceKey, ZSocketType.PULL, identity);

            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, pSocket, socket);
            return pool;
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            if (option.SpeedLimitModel != SpeedLimitType.WaitCount)
                option.SpeedLimitModel = SpeedLimitType.Single;
            return option;
        }



    }
}