using System;

namespace Agebull.ZeroNet.ZeroApi
{

    /// <summary>
    /// API访问配置
    /// </summary>
    [Flags]
    public enum ApiAccessOption
    {
        /// <summary>
        /// 不可访问
        /// </summary>
        None,
        /// <summary>
        /// 公开访问
        /// </summary>
        Public = 0x1,
        /// <summary>
        /// 内部访问
        /// </summary>
        Internal = 0x2,
        /// <summary>
        /// 游客
        /// </summary>
        Anymouse = 0x4,
        /// <summary>
        /// 客户
        /// </summary>
        Customer = 0x8,
        /// <summary>
        /// 商家用户
        /// </summary>
        Business = 0x10,
        /// <summary>
        /// 内部员工
        /// </summary>
        Employe = 0x20,

        /// <summary>
        /// 参数可以为null
        /// </summary>
        ArgumentCanNil = 0x1000
    }

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
}
