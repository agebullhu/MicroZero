using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     路由运行时数据相关的数据
    /// </summary>
    public class RouteRuntime
    {
        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        public static readonly string DenyAccess = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.DenyAccess, "*拒绝访问*"));
        
        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        public static readonly string RemoteEmptyError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.UnknowError, "*服务器无返回值*"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string NetworkError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, "*服务器访问异常*"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string Inner2Error =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "**系统内部错误**"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string InnerError =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "*系统内部错误*"));
    }
    
}