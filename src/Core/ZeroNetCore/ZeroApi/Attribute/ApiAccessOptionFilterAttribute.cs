
using System;

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
    /// API对应页面的特性
    /// </summary>
    public class ApiPageAttribute : Attribute
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="pageUrl">页面</param>
        public ApiPageAttribute(string pageUrl)
        {
            PageUrl = pageUrl;
        }
        /// <summary>
        /// 页面
        /// </summary>
        public string PageUrl { get; }
    }
}
