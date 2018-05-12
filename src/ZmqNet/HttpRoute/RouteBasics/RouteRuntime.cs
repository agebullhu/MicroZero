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
        ///     页面不存在的Json字符串
        /// </summary>
        public static readonly string NoFindJson = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NoFind, "*页面不存在*"));

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        public static readonly string DenyAccessJson = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.DenyAccess, "*拒绝访问*"));

        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        public static readonly string RemoteEmptyErrorJson =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.UnknowError, "*服务器无返回值*"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string NetworkErrorJson =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, "*服务器访问异常*"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string Inner2ErrorJson =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "**系统内部错误**"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string InnerErrorJson =
            JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.InnerError, "*系统内部错误*"));
    }
    
}