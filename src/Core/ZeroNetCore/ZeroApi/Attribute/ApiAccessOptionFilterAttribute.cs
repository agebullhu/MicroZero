
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
    
}
