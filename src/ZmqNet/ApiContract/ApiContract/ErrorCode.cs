using System.Collections.Generic;
// ReSharper disable All


namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 系统错误代码
    /// </summary>
    public class ErrorCode
    {
        #region 消息字典

        static readonly Dictionary<int, string> Map = new Dictionary<int, string>
        {
            {Success,"操作成功" },
            {UnknowError,"未知错误" },
            {ArgumentError,"参数错误" },
            {NetworkError,"网络异常" },
            {InnerError,"服务器内部错误" },
            {DenyAccess,"拒绝访问" },
            {ReTry,"客户端应重新请求" },
            {Ignore,"客户端应中止请求" },
            {NoFind,"页面不存在" },
            {TimeOut,"访问超时" },
            {NoReady,"系统未就绪" },
            {Auth_ServiceKey_Unknow,"未知的ServiceKey" },
            {Auth_RefreshToken_Unknow,"未知的RefreshToken" },
            {Auth_AccessToken_Unknow,"未知的AccessToken" },
            {Auth_User_Unknow,"未知的用户" },
            {Auth_Device_Unknow,"未知的设备识别码" },
            {Auth_AccessToken_TimeOut,"令牌过期" },
        };
        /// <summary>
        /// 取得错误码对应的消息文本
        /// </summary>
        /// <param name="eid">错误码</param>
        /// <returns>消息文本</returns>
        public static string GetMessage(int eid)
        {
            return Map.TryGetValue(eid, out string result) ? result : "发生未知错误";
        }

        #endregion

        /// <summary>
        /// 正确
        /// </summary>
        public const int Success = 0;

        /// <summary>
        /// 未知错误（发生异常)
        /// </summary>
        public const int UnknowError = -1;

        /// <summary>
        /// 发生异常
        /// </summary>
        public const int Exception = UnknowError;

        /// <summary>
        /// 参数错误
        /// </summary>
        public const int ArgumentError = -2;

        /// <summary>
        /// 网络错误(调用API出错)
        /// </summary>
        public const int NetworkError = -3;

        /// <summary>
        /// 调用API出错
        /// </summary>
        public const int CallApiError = NetworkError;

        /// <summary>
        /// 内部错误（逻辑出错）
        /// </summary>
        public const int InnerError = -4;

        /// <summary>
        /// 逻辑出错
        /// </summary>
        public const int BusinessError = InnerError;

        /// <summary>
        /// 拒绝访问
        /// </summary>
        public const int DenyAccess = -5;

        /// <summary>
        /// 客户端应重新请求
        /// </summary>
        public const int ReTry = -6;

        /// <summary>
        /// 客户端应中止请求
        /// </summary>
        public const int Ignore = -7;

        /// <summary>
        /// 访问超时
        /// </summary>
        public const int TimeOut = -8;


        /// <summary>
        /// 系统未就绪
        /// </summary>
        public const int NoReady = -9;

        /// <summary>
        /// 方法不存在
        /// </summary>
        public const int NoFind = 404;

        /// <summary>
        /// 服务不可用
        /// </summary>
        public const int Unavailable = 503;

        /// <summary>
        /// 未知的RefreshToken
        /// </summary>
        public const int Auth_RefreshToken_Unknow = 40083;

        /// <summary>
        /// 未知的ServiceKey
        /// </summary>
        public const int Auth_ServiceKey_Unknow = 40082;

        /// <summary>
        /// 未知的AccessToken
        /// </summary>
        public const int Auth_AccessToken_Unknow = 40081;
        
        /// <summary>
        /// 未知的用户 
        /// </summary>
        public const int Auth_User_Unknow = 40421;

        /// <summary>
        /// 未知的设备识别码
        /// </summary>
        public const int Auth_Device_Unknow = 40022;

        /// <summary>
        /// 令牌过期
        /// </summary>
        public const int Auth_AccessToken_TimeOut = 40036;
    }
}
