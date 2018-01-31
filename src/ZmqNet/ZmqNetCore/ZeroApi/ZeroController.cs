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
        public string Caller => ApiContext.RequestContext.ServiceKey;
        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => ApiContext.RequestContext.RequestId;
        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => ApiContext.RequestContext.UserAgent;

        ///// <summary>
        ///// 原始调用
        ///// </summary>
        ///// <param name="function">方法</param>
        ///// <param name="argument">参数</param>
        ///// <returns></returns>
        //public abstract IApiResult Call(string function, string argument);
    }
}