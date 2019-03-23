using System;

namespace Agebull.MicroZero.ZeroApis
{
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
