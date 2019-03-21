using System;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     缓存数据
    /// </summary>
    public class CacheData
    {
        /// <summary>
        ///     下次更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        ///     缓存内容
        /// </summary>
        public string Content { get; set; }
    }
}