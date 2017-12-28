using System;
using System.Configuration;
using System.Linq;
using Agebull.Common.Logging;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Yizuan.Service.Api;
using Agebull.Common.DataModel.Redis;
using GoodLin.Common.Redis;

namespace Yizuan.Service.Host
{
    /// <summary>
    /// 安全检查员
    /// </summary>
    public class SecurityChecker
    {
        private static bool CheckBearCache = bool.Parse(ConfigurationManager.AppSettings["CheckBearCache"] ?? "true");
        /// <summary>
        /// Http调用
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Auth头
        /// </summary>
        public string Bear { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int Status { get; set; }


        /// <summary>
        ///     执行检查
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空或不合格
        ///     2：令牌是伪造的
        /// </returns>
        public bool Check()
        {
            if (string.IsNullOrWhiteSpace(Bear))
            {
                return Request.GetUri().LocalPath.Contains("/oauth/getdid");
            }
            //var header = Request.Headers.Values.LinkToString(" ");
            //if (string.IsNullOrWhiteSpace(header) || header.Contains("iToolsVM"))
            //    return false;

            switch (Bear[0])
            {
                default:
                    return false;
                case '*':
                    return CheckDeviceId();
                case '{':
                    return false;
                case '$':
                    return false;
                case '#':
                    return CheckAccessToken();
            }
        }

        /// <summary>
        ///     检查设备标识
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空
        ///     2：令牌是伪造的
        /// </returns>
        private bool CheckDeviceId()
        {
            for (var index = 1; index < Bear.Length; index++)
            {
                var ch = Bear[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_')
                    continue;
                Status = ErrorCode.Auth_Device_Unknow;
                return false;
            }
            if (!CheckBearCache)
                return true;
            using (var proxy = new RedisProxy(RedisProxy.DbAuthority))
            {
               return proxy.Client.KeyExists(RedisKeyBuilder.ToAuthKey("token", "did", Bear));
            }
        }

        /// <summary>
        ///     检查AccessToken
        /// </summary>
        /// <returns>
        ///     0:表示通过验证，可以继续
        ///     1：令牌为空
        ///     2：令牌是伪造的
        /// </returns>
        private bool CheckAccessToken()
        {
            if (Bear.Length != 33)
                return false;
            for (var index = 1; index < Bear.Length; index++)
            {
                var ch = Bear[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                    continue;
                Status = ErrorCode.Auth_AccessToken_Unknow;
                return false;
            }
            if (!CheckBearCache)
                return true;
            using (var proxy = new RedisProxy(RedisProxy.DbAuthority))
            {
                return proxy.Client.KeyExists(RedisKeyBuilder.ToAuthKey("at", Bear));
            }
        }
    }
}