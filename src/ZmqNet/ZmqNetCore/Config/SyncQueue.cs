using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Agebull.Common
{
    /// <summary>
    /// 多生产者单消费者的同步列表（线程安全）
    /// </summary>
    /// <typeparam name="T">泛型对象</typeparam>
    /// <remarks>
    /// 1 内部使用信号量
    /// 2 用于多生产者单消费者的场景
    /// 3 使用双队列，以防止错误时无法还原
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public class SyncQueue<T>
    {
        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Queue<T> Queue { get; } = new Queue<T>();

        /// <summary>
        /// 正在处理
        /// </summary>
        [JsonProperty]
        public Queue<T> Doing { get; } = new Queue<T>();

        /// <summary>
        /// 用于同步的信号量
        /// </summary>
        private Semaphore _semaphore = new Semaphore(0, Int32.MaxValue);

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => Queue.Count == 0 && Doing.Count <= 1;

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
        public static SyncQueue<T> Load(string file)
        {
            SyncQueue<T> queue;
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    queue = JsonConvert.DeserializeObject<SyncQueue<T>>(json);
                    if (queue._semaphore == null)
                        queue._semaphore = new Semaphore(0, Int32.MinValue);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    queue = new SyncQueue<T>();
                }
            }
            else
            {
                queue = new SyncQueue<T>();
            }
            return queue;
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="t"></param>
        public void Push(T t)
        {
            lock (Queue)
            {
                Queue.Enqueue(t);
            }
            log.AppendLine($"Push:{Doing.Count}:{Queue.Count}");
            _semaphore.Release();
        }

        private StringBuilder log = new StringBuilder();
        /// <summary>
        /// 开始处理队列内容
        /// </summary>
        /// <param name="t">返回内容（如果返回True)</param>
        /// <param name="waitMs">等待时长</param>
        public bool StartProcess(out T t, int waitMs)
        {
            lock (Doing)
            {
                if (Doing.Count > 0)//之前存在失败
                {
                    t = Doing.Peek();
                    log.AppendLine($"Peek:{Doing.Count}:{Queue.Count}");
                    return true;
                }
            }
            if (!_semaphore.WaitOne(waitMs))
            {
                t = default(T);
                return false;
            }
            lock (Queue)
            {
                t = Queue.Dequeue();
            }

            lock (Doing)
            {
                Doing.Enqueue(t);
            }
            log.AppendLine($"Enqueue:{Doing.Count}:{Queue.Count}");
            return true;
        }

        /// <summary>
        /// 完成处理队列内容
        /// </summary>
        public void EndProcess()
        {
            lock (Doing)
            {
                log.AppendLine($"Dequeue:{Doing.Count}:{Queue.Count}");
                Doing.Dequeue();
                PubCount += 1;
                DataCount += 1;
                if (DataCount == long.MaxValue)
                    DataCount = 0;
                if (PubCount == long.MaxValue)
                    PubCount = 0;
            }
        }


        /// <summary>
        /// 广播总数
        /// </summary>
        public long PubCount { get; private set; }
        /// <summary>
        /// 广播总数
        /// </summary>
        public long DataCount { get; private set; }

    }
}