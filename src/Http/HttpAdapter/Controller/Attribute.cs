using System;
using Microsoft.AspNetCore.Mvc;

namespace Agebull.MicroZero.ZeroApis
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
    public class ApiPageAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="page"></param>
        public ApiPageAttribute(string page)
        {
            Page = page;
        }
        /// <summary>
        /// 配置
        /// </summary>
        public string Page { get; }
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
    public class RoutePrefixAttribute : RouteAttribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="template"></param>
        public RoutePrefixAttribute(string template)
            : base(template)
        {
        }
    }
}
