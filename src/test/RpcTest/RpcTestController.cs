using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;

namespace RpcTest
{
    /// <summary>
    /// Weixin服务
    /// </summary>
    public class RpcTestController : ApiController
    {
        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        [Route("v1/msg/text")]
        public ApiResult OnTextRequest()
        {

            return ApiResult.Ok;
        }
    }
}