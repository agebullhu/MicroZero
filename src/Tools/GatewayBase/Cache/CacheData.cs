using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     缓存数据
    /// </summary>
    public class CacheData
    {
        /// <summary>
        /// 是否正在载入，如果是，等待即可
        /// </summary>
        public int IsLoading;

        /// <summary>
        ///     缓存内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     是否正确
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     下次更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }


        /// <summary>
        ///     同步等待任务
        /// </summary>
        public readonly List<TaskCompletionSource<string>> Waits = new List<TaskCompletionSource<string>>();

    }
}