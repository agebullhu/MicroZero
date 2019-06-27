using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Context;
using Agebull.Common.Logging;
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
        protected override IZmqPool PrepareLoop(byte[] identity, out ZSocket socket)
        {
            socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, identity);
            var pSocket = ZeroApplication.Config.ApiRouterModel
                    ? ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.DEALER, identity)
                    : ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, identity);

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
            if (option.SpeedLimitModel == SpeedLimitType.None)
                option.SpeedLimitModel = SpeedLimitType.ThreadCount;
            return option;
        }



    }
}