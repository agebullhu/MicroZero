using System;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero.ZeroApis;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Senparc.NeuChar;
using MicroZero.Http.Gateway.Weixin;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("QueueDemo")]
    public class QueueDemo : QueueStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public QueueDemo()
        {
            Name = "QueueDemo";
            StationName = "QueueDemo";
            Subscribe = "";
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [Route("v1/test")]
        public ApiResult CallBack(Argument arg)
        {
            Console.WriteLine($"CallBack:{arg.Value}");
            return ApiResult.Succees();
        }
    }

}
