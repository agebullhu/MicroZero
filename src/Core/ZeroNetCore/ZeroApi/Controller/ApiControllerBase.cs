using System.Collections.Generic;
using Agebull.Common.Context;
using Agebull.Common.OAuth;
using Agebull.EntityModel.Common;


namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// ZeroApi控制器基类
    /// </summary>
    public class ApiControllerBase
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public ILoginUserInfo UserInfo => GlobalContext.Customer;

        /// <summary>
        /// 调用者（机器名）
        /// </summary>
        public string Caller => GlobalContext.ServiceName;

        /// <summary>
        /// 调用标识
        /// </summary>
        public string RequestId => GlobalContext.RequestInfo.RequestId;

        /// <summary>
        /// HTTP调用时的UserAgent
        /// </summary>
        public string UserAgent => GlobalContext.RequestInfo.UserAgent;

        ApiCallItem _apiCallItem;

        /// <summary>
        /// 原始调用帧消息
        /// </summary>
        public ApiCallItem ApiCallItem => _apiCallItem ?? (_apiCallItem = GlobalContext.Current.DependencyObjects.Dependency<ApiCallItem>());

        /// <summary>
        /// 客户端上传的文件
        /// </summary>
        public List<NameValue<string, byte[]>> Files => ApiCallItem.Files;
    }
}