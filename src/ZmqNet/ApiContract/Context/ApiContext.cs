using System;
using System.Configuration;
using System.Diagnostics;
using Agebull.Common.DataModel.Redis;
using GoodLin.Common.Redis;
using Newtonsoft.Json;
using Yizuan.Service.Api.OAuth;

namespace Yizuan.Service.Api
{
    /// <summary>
    ///     API调用上下文（流程中使用）
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiContext
    {
        #region 服务器标识


        /// <summary>
        /// 当前服务器的标识
        /// </summary>
        /// <remarks>
        /// 服务注册时自动分配
        /// </remarks>
        public static string MyServiceKey { get; set; } = ConfigurationManager.AppSettings["ServiceKey"];

        /// <summary>
        /// 当前服务器的名称
        /// </summary>
        /// <remarks>
        /// 但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyServiceName { get; set; } = ConfigurationManager.AppSettings["ServiceName"];


        /// <summary>
        /// 当前服务器的名称
        /// </summary>
        /// <remarks>
        /// 但实际名称，会以服务器返回为准。
        /// </remarks>
        public static string MyAddress { get; set; } = ConfigurationManager.AppSettings["ServiceAddress"];

        #endregion

        #region 全局属性


        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        [ThreadStatic] private static ApiContext _current;

        /// <summary>
        ///     当前线程的调用上下文
        /// </summary>
        public static ApiContext Current => _current ?? (_current = new ApiContext());

        #endregion


        #region 缓存

        /// <summary>
        ///     缓存当前上下文
        /// </summary>
        public void Cache()
        {
            using (var proxy = new RedisProxy(RedisProxy.DbContext))
            {
                proxy.Set(GetCacheKey(Request.RequestId), this, new TimeSpan(0, 5, 0));
            }
        }

        /// <summary>
        ///     还原当前上下文
        /// </summary>
        public static void Restore(string requestId)
        {
            using (var proxy = new RedisProxy(RedisProxy.DbContext))
            {
                _current = proxy.Get<ApiContext>(GetCacheKey(requestId));
            }
        }

        /// <summary>
        ///     得到缓存的键
        /// </summary>
        public static string GetCacheKey(Guid requestId)
        {
            var key = RedisKeyBuilder.ToSystemKey("api", "ctx", requestId.ToString().ToUpper());
            Debug.WriteLine(key);
            return key;
        }

        /// <summary>
        ///     得到缓存的键
        /// </summary>
        public static string GetCacheKey(string requestId)
        {
            var key = RedisKeyBuilder.ToSystemKey("api", "ctx", requestId.Trim('$').ToUpper());
            Debug.WriteLine(key);
            return key;
        }

        #endregion

        #region 调用上下文

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        public static InternalCallContext RequestContext => Current._requestContext;

        /// <summary>
        ///     设置当前上下文
        /// </summary>
        /// <param name="context"></param>
        public static void SetContext(ApiContext context)
        {
            _current = context;
        }

        /// <summary>
        ///     设置当前请求上下文
        /// </summary>
        /// <param name="context"></param>
        public static void SetRequestContext(InternalCallContext context)
        {
            Current._requestContext = context;
        }

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        public InternalCallContext Request => _requestContext;

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        [JsonProperty] private InternalCallContext _requestContext;

        #endregion


        #region 用户校验支持

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        public static ILoginUserInfo Customer => Current._user;

        /// <summary>
        ///     设置当前用户
        /// </summary>
        /// <param name="customer"></param>
        public static void SetCustomer(LoginUserInfo customer)
        {
            Current._user = customer;
        }

        /// <summary>
        ///     当前调用的客户信息
        /// </summary>
        public ILoginUserInfo LoginUser => _user;

        /// <summary>
        ///     当前调用上下文
        /// </summary>
        [JsonProperty] private LoginUserInfo _user;


        /// <summary>
        ///     检查上下文，如果信息为空，加入系统匿名用户上下文
        /// </summary>
        public static void TryCheckByAnymouse()
        {
            if (Current._requestContext == null)
            {
                Current._requestContext = new InternalCallContext
                {
                    RequestId = Guid.NewGuid()
                };
            }
            if (Current._user == null)
            {
                Current._user = new LoginUserInfo
                {
                    UserId = -2,
                    Account = "Anymouse",
                    NickName = "匿名用户",
                    DeviceId = "*SYSTEM",
                    Os = "SYSTEM",
                    Browser = "SYSTEM",
                    LoginSystem = "None",
                    LoginType = 0
                };
            }
            Current.Cache();
        }

        #endregion
    }
}