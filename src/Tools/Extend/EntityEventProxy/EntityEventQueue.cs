using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Agebull.Common;
using Newtonsoft.Json;

namespace Gboxt.Common.DataModel.ZeroNet
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
    public class EntityEventQueue
    {
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long PubCount { get; private set; }
        /// <summary>
        /// 广播总数
        /// </summary>
        public static long DataCount { get; private set; }
        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Queue<string> Queue { get; } = new Queue<string>();

        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Dictionary<string, List<EntityEventItem>> Items { get; } = new Dictionary<string, List<EntityEventItem>>();

        /// <summary>
        /// 内部队列
        /// </summary>
        [JsonProperty]
        public Dictionary<string, List<EntityEventItem>> DoItems { get; } = new Dictionary<string, List<EntityEventItem>>();

        /// <summary>
        /// 正在处理
        /// </summary>
        [JsonProperty]
        public Queue<string> Doing { get; } = new Queue<string>();

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
        public static EntityEventQueue Load(string file)
        {
            EntityEventQueue queue;
            if (File.Exists(file))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    queue = JsonConvert.DeserializeObject<EntityEventQueue>(json);
                    if (queue._semaphore == null)
                        queue._semaphore = new Semaphore(0, Int32.MinValue);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    queue = new EntityEventQueue();
                }
            }
            else
            {
                queue = new EntityEventQueue();
            }
            return queue;
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="item"></param>
        public void Push(EntityEventItem item)
        {
            lock (Doing)
            {
                if (Items.TryGetValue(item.DbName, out var list))
                {
                    list.Add(item);
                }
                else
                {
                    Queue.Enqueue(item.DbName);
                    Items.Add(item.DbName, new List<EntityEventItem> { item });
                    _semaphore.Release();
                }
            }
        }

        /// <summary>
        /// 开始处理队列内容
        /// </summary>
        /// <param name="database">数据库</param>
        /// <param name="t">返回内容（如果返回True)</param>
        /// <param name="waitMs">等待时长</param>
        public bool StartProcess(out string database, out List<EntityEventItem> t, int waitMs)
        {
            lock (Doing)
            {
                if (Doing.Count > 0)//之前存在失败
                {
                    database = Doing.Peek();
                    if (!DoItems.TryGetValue(database, out t))
                        return false;
                    if (t != null && t.Count != 0)
                        return true;
                    DoItems.Remove(database);
                    t = null;
                    database = null;
                    return false;
                }
                if (!_semaphore.WaitOne(waitMs))
                {
                    t = null;
                    database = null;
                    return false;
                }
                database = Queue.Dequeue();
                t = Items[database];
                Items.Remove(database);
                if (t.Count == 0)
                    return true;
                Doing.Enqueue(database);
                if (DoItems.TryGetValue(database, out var list))
                {
                    list.AddRange(t);
                }
                else
                {
                    DoItems.Add(database, t);
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
                PubCount += 1;
                DataCount += DoItems[key].Count;
                if (DataCount == long.MaxValue)
                    DataCount = 0;
                if (PubCount == long.MaxValue)
                    PubCount = 0;
                DoItems.Remove(key);
            }

        }
    }
}