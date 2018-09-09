using Agebull.Common.OAuth;
using Agebull.Common.Rpc;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public class ApiController
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public ILoginUserInfo UserInfo => GlobalContext.Customer;
        /// <summary>
        /// 调用者（机器名）
        /// </summary>
        public string Caller => GlobalContext.RequestInfo.ServiceKey;
        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => GlobalContext.RequestInfo.RequestId;
        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => GlobalContext.RequestInfo.UserAgent;

    }
    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public class ZeroApiController : ApiController
    {
    }
}