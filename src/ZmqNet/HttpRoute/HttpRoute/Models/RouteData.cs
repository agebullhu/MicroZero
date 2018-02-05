using System;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由数据
    /// </summary>
    internal class RouteData
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public Uri Uri;
        /// <summary>
        ///     当前适用的缓存设置对象
        /// </summary>
        public CacheSetting CacheSetting;
        /// <summary>
        ///     缓存键
        /// </summary>
        public string CacheKey;
        /// <summary>
        /// 执行状态
        /// </summary>
        public RouteStatus Status;
        /// <summary>
        /// 执行HTTP重写向吗
        /// </summary>
        public bool Redirect;
        /// <summary>
        ///     返回值
        /// </summary>
        public string ResultMessage;
        
        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        public string Bearer;

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        public string HostName;

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        public string ApiName;

        /// <summary>
        ///     请求的内容名称
        /// </summary>
        public string Context;

        /// <summary>
        ///     HTTP method
        /// </summary>
        public string HttpMethod;
        /// <summary>
        ///     路由主机信息
        /// </summary>
        public HostConfig RouteHost;

        /// <summary>
        /// 是否正常
        /// </summary>
        public bool IsSucceed { get; set; }
    }
}