using Agebull.Common.OAuth;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public class ZeroApiController
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public ILoginUserInfo UserInfo => ApiContext.Customer;
        /// <summary>
        /// 调用者（机器名）
        /// </summary>
        public string Caller => ApiContext.RequestInfo.ServiceKey;
        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => ApiContext.RequestInfo.RequestId;
        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => ApiContext.RequestInfo.UserAgent;
        
    }
}