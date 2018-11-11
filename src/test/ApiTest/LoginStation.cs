using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Agebull.ZeroNet.PubSub;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("QUEUE")]
    public class TestQueue : QueueStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        public TestQueue()
        {
            StationName = "QUEUE";
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        [Route("test")]
        [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiPageResult<LoginArg> Login(LoginArg arg)
        {
            return new ApiPageResult<LoginArg>
            {
                Success = true,
                Status = new ApiStatusResult
                {
                    ErrorCode = 0,
                    //ClientMessage = $"Wecome {user.MobilePhone}!"
                }
            };
        }
    }

}
