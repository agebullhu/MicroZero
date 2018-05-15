using System;
using System.Configuration;
using System.Threading;
using Agebull.Common.Base;
using Agebull.Common.Logging;
using Agebull.Common.OAuth;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     API调用上下文（流程中使用）
    /// </summary>
    [Serializable]
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
        public static string MyServiceKey { get; set; } = ConfigurationManager.AppSettings["ServiceKey"];

        /// <summary>
        ///     当前服务器的名称
        /// </summary>
        /// <remarks>
        ///     但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyServiceName { get; set; } = ConfigurationManager.AppSettings["ServiceName"];


        /// <summary>
        ///     当前服务器的运行时名称
        /// </summary>
        /// <remarks>
        ///     但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyRealName { get; set; } = ConfigurationManager.AppSettings["ServiceAddress"];

        #endregion

        #region 全局属性
        /// <summary>
        /// 设置LogRecorder的依赖属性
        /// </summary>
        public static void SetLogRecorderDependency()
        {
            LogRecorder.GetMachineNameFunc = () => MyServiceName;
            LogRecorder.GetUserNameFunc = () => Customer?.Account ?? "Unknow";
            LogRecorder.GetRequestIdFunc = () => RequestContext?.RequestId ?? Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 显示式设置配置对象(依赖)
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// 全局
        /// </summary>
        public static IConfiguration Configuration { get;private set; }
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
        ///     当前调用上下文
        /// </summary>
        public static CallContext RequestContext => Current._requestContext;

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        public static ILoginUserInfo Customer => Current._user;

        #endregion


        #region 实例变量
        /// <summary>
        ///     当前调用上下文
        /// </summary>
        [JsonProperty("r")] private CallContext _requestContext;

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        [JsonProperty("u")] private LoginUserInfo _user;


        /// <summary>
        ///     当前调用上下文
        /// </summary>
        public CallContext Request => _requestContext ?? (_requestContext = new CallContext());

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        public ILoginUserInfo LoginUser => _user ?? (_user = new LoginUserInfo { UserId = -1 });


        #endregion

        #region 内容设置

        private ApiContext()
        {
            _requestContext = new CallContext
            {
                requestId = $"{MyRealName}-{RandomOperate.Generate(8)}",
                Bear = "<error>",
                serviceKey = MyServiceKey,
                RequestType = RequestType.None
            };
            _user = LoginUserInfo.CreateAnymouse("<error>", "<error>", "<error>");
        }
        /// <summary>
        ///     设置当前上下文（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="context"></param>
        public static void SetContext(ApiContext context)
        {
            if (_current == context)
                return;
            var local = new AsyncLocal<ApiContext>();
            if (local.Value == context) return;
            local.Value?.Dispose();
            local.Value = context;
            _current = context;
        }
        
        /// <summary>
        ///     设置当前用户（框架内调用，外部误用后果未知）
        /// </summary>
        /// <param name="customer"></param>
        public static void SetCustomer(LoginUserInfo customer)
        {
            Current._user = customer;
        }

        /// <summary>
        ///     检查上下文，如果信息为空，加入系统匿名用户上下文
        /// </summary>
        public static void TryCheckByAnymouse()
        {
            if (Current._requestContext == null)
                Current._requestContext = new CallContext
                {
                    requestId = $"{MyRealName}-{RandomOperate.Generate(8)}",
                    Bear = "<error>",
                    serviceKey = MyServiceKey,
                    RequestType = RequestType.None
                };
            if (Current._user == null)
                Current._user = LoginUserInfo.CreateAnymouse(Current._requestContext.Bear, "<error>", "<error>");
        }

        #endregion
    }
}