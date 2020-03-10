using System;
using System.Collections.Concurrent;
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
    /// <typeparam name="TData">泛型对象</typeparam>
    /// <remarks>
    /// 1 内部使用信号量
    /// 2 用于多生产者单消费者的场景
    /// 3 使用双队列，以防止错误时无法还原
    /// </remarks>
    public class SyncBatchQueue<TData>
    {
        /// <summary>
        /// 内部队列
        /// </summary>
        public ConcurrentBag<TData> Line1 = new ConcurrentBag<TData>();

        /// <summary>
        /// 内部队列
        /// </summary>
        public ConcurrentBag<TData> Line2 = new ConcurrentBag<TData>();


        private int index;

        ConcurrentBag<TData> now;
        /// <summary>
        /// 构造
        /// </summary>
        public SyncBatchQueue()
        {
            now = Line1;
        }
        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="t"></param>
        public void Push(TData t)
        {
            if (inSwitch)
            {
                lock (this)//防止交换时间插入不正常数据
                {
                    Thread.Sleep(0);
                }
            }
            now.Add(t);
        }

        private bool inSwitch;
        /// <summary>
        /// 开始处理队列内容
        /// </summary>
        public List<TData> Switch()
        {
            ConcurrentBag<TData> old = now;
            inSwitch = true;
            lock (this)
            {
                Thread.Sleep(0);//让其它线程进入等待状态
                if (index == 1)
                {
                    index = 2;
                    now = Line2;
                }
                else
                {
                    index = 1;
                    now = Line1;
                }
            }
            inSwitch = false;
            if (old.Count == 0)
                return null;
            List<TData> array = old.ToList();
            if (index == 1)
            {
                Line2 = new ConcurrentBag<TData>();
            }
            else
            {
                Line1 = new ConcurrentBag<TData>();
            }
            return array;
        }

        #region 序列化
        /// <summary>
        /// 保存以备下次启动时使用
        /// </summary>
        public void Save(string file)
        {
            List<TData> data = new List<TData>();
            data.AddRange(Line1);
            data.AddRange(Line2);
            var json = JsonConvert.SerializeObject(data);
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
            {
                index = 1;
                now = Line1;
                return false;
            }

            index = 2;
            now = Line2;
            try
            {
                var json = File.ReadAllText(file);
                var inner = JsonConvert.DeserializeObject<List<TData>>(json);
                if (inner != null)
                    foreach (var item in inner)
                        Line1.Add(item);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            finally
            {
                File.Delete(file);
            }
        }

        #endregion
    }
}