using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Agebull.Common;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.LogRecorder
{
    /// <summary>
    /// 多生产者单消费者的同步列表（线程安全）
    /// </summary>
    /// <remarks>
    /// 1 内部使用信号量
    /// 2 用于多生产者单消费者的场景
    /// 3 使用双队列，以防止错误时无法还原
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
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
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Dictionary<LogType, List<RecordInfo>> DoItems { get; } = new Dictionary<LogType, List<RecordInfo>>();

        /// <summary>
        /// 正在处理
        /// </summary>
        [JsonProperty]
        public Queue<LogType> Doing { get; } = new Queue<LogType>();

        /// <summary>
        /// 用于同步的信号量
        /// </summary>
        private Semaphore _semaphore = new Semaphore(0, Int32.MaxValue);

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Queue.Count == 0;
        }


        /// <summary>
        /// 保存以备下次启动时使用
        /// </summary>
        public void Save(string file)
        {
            string json;
            lock (this)
                json = JsonConvert.SerializeObject(this);
            IOHelper.CheckPath(Path.GetDirectoryName(file));
            File.WriteAllText(file, json);
        }

        /// <summary>
        /// 载入保存的内容
        /// </summary>
        /// <returns>队列对象</returns>
        public static LogQueue Load(string file)
        {
            LogQueue queue;
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    queue = JsonConvert.DeserializeObject<LogQueue>(json);
                    if (queue._semaphore == null)
                        queue._semaphore = new Semaphore(0, Int32.MinValue);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    queue = new LogQueue();
                }
            }
            else
            {
                queue = new LogQueue();
            }
            return queue;
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="item"></param>
        public void Push(RecordInfo item)
        {
            lock (Doing)
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
        public bool StartProcess(out LogType type, out List<RecordInfo> t, int waitMs)
        {
            lock (Doing)
            {
                if (Doing.Count > 0)//之前存在失败
                {
                    type = Doing.Peek();
                    if (!DoItems.TryGetValue(type, out t))
                        return false;
                    if (t != null && t.Count != 0)
                        return true;
                    DoItems.Remove(type);
                    t = null;
                    type = LogType.None;
                    return false;
                }
                if (!_semaphore.WaitOne(waitMs))
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
                Doing.Enqueue(type);
                if (DoItems.TryGetValue(type, out var list))
                {
                    list.AddRange(t);
                }
                else
                {
                    DoItems.Add(type, t);
                }
            }
            return true;
        }

        /// <summary>
        /// 完成处理队列内容
        /// </summary>
        public void EndProcess()
        {
            lock (Doing)
            {
                var key = Doing.Dequeue();
                DoItems.Remove(key);
            }
        }
    }
}