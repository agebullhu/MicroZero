using System.ComponentModel;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("MachineEvent")]
    public class MachineEventStation : ZeroApiController
    {
        /// <summary>
        /// 事件上载
        /// </summary>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        [Route("event/put"), Category("事件上载")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult MachineEvent(MachineEventArg arg)
        {
            ZeroPublisher.Publish("MachineEventMQ",arg.EventName, "Event", arg.JsonStr);
            return new ApiResult
            {
                Success = true,
                Status = new ApiStatusResult
                {
                    ErrorCode = 0,
                    ClientMessage = $"{arg.MachineId} : {arg.EventName } IsPublish"
                }
            };
        }
    }
}