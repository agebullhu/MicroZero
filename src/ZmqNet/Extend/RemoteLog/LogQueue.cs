using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    /// 多生产者单消费者的同步列表（线程安全）
    /// </summary>
    /// <remarks>
    /// 1 内部使用信号量
    /// 2 用于多生产者单消费者的场景
    /// 3 使用双队列，以防止错误时无法还原
    /// </remarks>
    public class LogQueue
    {
        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Queue<LogType> Queue { get; } = new Queue<LogType>();

        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Dictionary<LogType, List<RecordInfo>> Items { get; } = new Dictionary<LogType, List<RecordInfo>>();

        /// <summary>
        /// 用于同步的信号量
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, Int32.MaxValue);

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Queue.Count == 0;
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="item"></param>
        public void Push(RecordInfo item)
        {
            {
                if (Items.TryGetValue(item.Type, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    Queue.Enqueue(item.Type);
                    Items.Add(item.Type, new List<RecordInfo> { item });
                    _semaphore.Release();
                }
            }
        }

        /// <summary>
        /// 开始处理队列内容
        /// </summary>
        /// <param name="type"></param>
        /// <param name="t">返回内容（如果返回True)</param>
        /// <param name="waitMs">等待时长</param>
        public bool Wait(out LogType type, out List<RecordInfo> t, int waitMs)
        {
            {
                if (!_semaphore.Wait(waitMs))
                {
                    t = null;
                    type = LogType.None;
                    return false;
                }
                type = Queue.Dequeue();
                t = Items[type];
                Items.Remove(type);
                if (t.Count == 0)
                    return true;
            }
            return true;
        }
    }
}