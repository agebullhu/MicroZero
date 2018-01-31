using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using Agebull.ZeroNet.ZeroApi;
using Agebull.Common.DataModel.Redis;
using Agebull.Common.Redis;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 安全检查员
    /// </summary>
    public class SecurityChecker
    {
        /// <summary>
        /// Http调用
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// Auth头
        /// </summary>
        public string Bearer { get; set; }

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
            //return true;
            if (string.IsNullOrWhiteSpace(Bearer))
            {
                return true;//Request.GetUri().LocalPath.Contains("/oauth/getdid");
            }
            //var header = Request.Headers.Values.LinkToString(" ");
            //if (string.IsNullOrWhiteSpace(header) || header.Contains("iToolsVM"))
            //    return false;

            switch (Bearer[0])
            {
                default:
                    return true;
                case '*':
                    return CheckDeviceId();
                //case '{':
                //case '$':
                //    return true;
                case '#':
                    return CheckAccessToken();
            }
        }

        private static readonly Dictionary<string, bool> Keys = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

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
            for (var index = 1; index < Bearer.Length; index++)
            {
                var ch = Bearer[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_')
                    continue;
                Status = ErrorCode.Auth_Device_Unknow;
                return false;
            }
            if (!AppConfig.Config.SystemConfig.CheckBearerCache)
                return true;
            bool hase;
            lock (Keys)
            {
                if (Keys.TryGetValue(Bearer, out hase))
                    return hase;
            }
            using (var proxy = new RedisProxy(RedisProxy.DbAuthority))
            {
                hase = proxy.Client.KeyExists(RedisKeyBuilder.ToAuthKey("token", "did", Bearer));
            }
            lock (Keys)
            {
                if (!Keys.ContainsKey(Bearer))
                    Keys.Add(Bearer, hase);
            }
            return hase;
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
            if (Bearer.Length != 33)
                return false;
            for (var index = 1; index < Bearer.Length; index++)
            {
                var ch = Bearer[index];
                if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'))
                    continue;
                Status = ErrorCode.Auth_AccessToken_Unknow;
                return false;
            }
            if (!AppConfig.Config.SystemConfig.CheckBearerCache)
                return true;

            bool hase;
            lock (Keys)
            {
                if (Keys.TryGetValue(Bearer, out hase))
                    return hase;
            }
            using (var proxy = new RedisProxy(RedisProxy.DbAuthority))
            {
                hase = proxy.Client.KeyExists(RedisKeyBuilder.ToAuthKey("at", Bearer));
            }
            lock (Keys)
            {
                if (!Keys.ContainsKey(Bearer))
                    Keys.Add(Bearer, hase);
            }
            return hase;
        }
    }
}