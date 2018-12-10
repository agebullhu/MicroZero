using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Agebull.Common.Configuration;
using Agebull.ZeroNet.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     路由配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class RouteOption
    {
        #region 配置
        public static SystemConfig Option { get; set; }

        #endregion

        #region 初始化

        /// <summary>
        ///     是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <returns></returns>
        public static bool CheckOption()
        {
            var sec = ConfigurationManager.Root.GetSection("SenparcWeixinSetting");
            if (!sec.Exists())
                throw new Exception("无法找到配置节点WeChart,在appsettings.json中设置");
            Option = sec.Get<SystemConfig>();
            IsInitialized = true;
            return true;
        }

        #endregion
    }
}