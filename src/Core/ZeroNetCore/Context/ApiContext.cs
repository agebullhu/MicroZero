using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using Agebull.Common.Base;
using Agebull.Common.OAuth;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     API调用上下文（流程中使用）
    /// </summary>
    [DataContract, Category("上下文")]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiContext : ScopeBase
    {
        #region 服务器标识

        /// <summary>
        ///     当前服务器的标识
        /// </summary>
        /// <remarks>
        ///     服务注册时自动分配
        /// </remarks>
        public static string MyServiceKey { get; set; }

        /// <summary>
        ///     当前服务器的名称
        /// </summary>
        /// <remarks>
        ///     但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyServiceName { get; set; }


        /// <summary>
        ///     当前服务器的运行时名称
        /// </summary>
        /// <remarks>
        ///     但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyRealName { get; set; }

        #endregion

        #region 全局属性

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        [ThreadStatic] private static ApiContext _current;

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static ApiContext Current
        {
            get
            {
                if (_current != null)
                    return _current;
                var local = new AsyncLocal<ApiContext>();
                if (local.Value != null)
                    return _current = local.Value;
                local.Value = _current = new ApiContext();
                return _current;
            }
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            _current = null;
            var local = new AsyncLocal<ApiContext>();
            if (local.Value != null)
                local.Value = null;
        }

        /// <summary>
        /// 最后操作的错误码
        /// </summary>
        public int LastError { get; set; }

        /// <summary>
        /// 最后操作的操作状态
        /// </summary>
        public int LastState { get; set; }

        #endregion


        #region 实例变量
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        [JsonProperty("user")] private ILoginUserInfo _user;
        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        public static ILoginUserInfo Customer => Current._user;
        
        /// <summary>
        ///     当前调用的组织信息
        /// </summary>
        [JsonProperty("org")] private IOrganizational _organizational;

        /// <summary>
        ///     当前调用的组织信息
        /// </summary>
        public static IOrganizational Organizational => Current._organizational;
        

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        [JsonProperty("req")] private RequestInfo _requestInfo;

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        public static RequestInfo RequestInfo => Current._requestInfo ?? (Current._requestInfo = new RequestInfo());

        #endregion

        #region 内容设置

        /// <summary>
        /// 内部构造
        /// </summary>
        private ApiContext()
        {
            _requestInfo = new RequestInfo(MyServiceKey, $"{MyServiceKey}-{Guid.NewGuid():N}");
            _user = LoginUserInfo.CreateAnymouse("<error>", "<error>", "<error>");
            _organizational = OrganizationalInfo.System;
        }

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="context"></param>
        public static void SetContext(ApiContext context)
        {
            if (Current == context)
                return;
            if (context._user != null)
                Current._user = context._user;
            if (context._requestInfo != null)
                Current._requestInfo = context._requestInfo;
        }

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        public static void SetRequestContext(string globalId, string serviceKey, string requestId)
        {
            Current._requestInfo = new RequestInfo(globalId, serviceKey, requestId);
        }


        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        public static void SetRequestContext(string serviceKey, string requestId)
        {
            Current._requestInfo = new RequestInfo(serviceKey, requestId);
        }

        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="context"></param>
        public static void SetRequestContext(RequestInfo context)
        {
            Current._requestInfo = context;
        }

        /// <summary>
        ///     设置当前组织（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="org"></param>
        public static void SetOrganizational(IOrganizational org)
        {
            Current._organizational = org;
        }

        /// <summary>
        ///     设置当前用户（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="user"></param>
        public static void SetUser(ILoginUserInfo user)
        {
            Current._user = user;
        }

        #endregion
    }
}