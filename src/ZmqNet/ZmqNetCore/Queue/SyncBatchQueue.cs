using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class SyncBatchQueue<T>
    {
        /// <summary>
        /// 内部队列
        /// </summary>
        public Queue<List<T>> Queue { get; } = new Queue<List<T>>();

        /// <summary>
        /// 正在处理
        /// </summary>
        public Queue<List<T>> Doing { get; } = new Queue<List<T>>();

        /// <summary>
        /// 用于同步的信号量
        /// </summary>
        private readonly Semaphore _semaphore = new Semaphore(0, Int32.MaxValue);

        /// <summary>
        /// 是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => Queue.Count == 0 && Doing.Count <= 1;


        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="t"></param>
        public void Push(T t)
        {
            Interlocked.Increment(ref DataCount);
            lock (this)
            {
                if (Queue.Count == 0)
                {
                    Queue.Enqueue(new List<T> { t });
                    _semaphore.Release();
                }
                else
                    Queue.Peek().Add(t);
            }
        }

        /// <summary>
        /// 开始处理队列内容
        /// </summary>
        /// <param name="t">返回内容（如果返回True)</param>
        /// <param name="waitMs">等待时长</param>
        public bool StartProcess(out List<T> t, int waitMs)
        {
            lock (this)
            {
                if (Doing.Count > 0)//之前存在失败
                {
                    t = Doing.Peek();
                    Interlocked.Increment(ref ProcessCount);
                    return true;
                }
                if (!_semaphore.WaitOne(waitMs))
                {
                    t = null;
                    return false;
                }
                t = Queue.Dequeue();
                Doing.Enqueue(t);
            }
            Interlocked.Increment(ref ProcessCount);
            return true;
        }

        /// <summary>
        /// 完成处理队列内容
        /// </summary>
        public void EndProcess()
        {
            Interlocked.Increment(ref SuccessCount);
            lock (this)
            {
                Doing.Dequeue();
            }
        }

        #region MyRegion


        /// <summary>
        /// 广播总数
        /// </summary>
        public long SuccessCount;

        /// <summary>
        /// 广播总数
        /// </summary>
        public long DataCount;

        /// <summary>
        /// 广播总数
        /// </summary>
        public long ProcessCount;

        #endregion

        #region 序列化
        /// <summary>
        /// 内部序列化使用
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class InnerQueue
        {
            /// <summary>
            /// 内部队列
            /// </summary>
            [JsonProperty]
            public List<List<T>> QueueList;

            /// <summary>
            /// 内部队列
            /// </summary>
            [JsonProperty]
            public List<List<T>> DoingList;
        }
        /// <summary>
        /// 保存以备下次启动时使用
        /// </summary>
        public void Save(string file)
        {
            var inner = new InnerQueue();
            lock (this)
            {
                inner.QueueList = Queue.ToList();
                inner.DoingList = Doing.ToList();
            }
            var json = JsonConvert.SerializeObject(inner);
            IOHelper.CheckPath(Path.GetDirectoryName(file));
            File.WriteAllText(file, json);
        }

        /// <summary>
        /// 载入保存的内容
        /// </summary>
        /// <returns>队列对象</returns>
        public bool Load(string file)
        {
            if (!File.Exists(file))
                return false;

            try
            {
                var json = File.ReadAllText(file);
                var inner = JsonConvert.DeserializeObject<InnerQueue>(json);
                lock (this)
                {
                    if (inner.QueueList != null && inner.QueueList.Count > 0)
                    {
                        foreach (var vl in inner.QueueList)
                            Queue.Enqueue(vl);
                    }
                    if (inner.DoingList != null && inner.DoingList.Count > 0)
                    {
                        foreach (var vl in inner.DoingList)
                            Doing.Enqueue(vl);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        #endregion
    }
}