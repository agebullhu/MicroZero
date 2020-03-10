using Agebull.Common.Rpc;
using System;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// API配置过滤器
    /// </summary>
    public class ApiAccessOptionFilterAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="option"></param>
        public ApiAccessOptionFilterAttribute(ApiAccessOption option)
        {
            Option = option;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public ApiAccessOption Option { get; }
    }


    /// <summary>
    /// 站点名称
    /// </summary>
    public class StationAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public StationAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// 路由名称
    /// </summary>
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public RouteAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get;}
    }
    /// <summary>
    /// 路由名称
    /// </summary>
    public class RoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="name"></param>
        public RoutePrefixAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Name { get; }
    }
    
}
