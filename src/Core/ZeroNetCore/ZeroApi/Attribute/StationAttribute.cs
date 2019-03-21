using System;

namespace Agebull.MicroZero.ZeroApi
{
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
    
}
