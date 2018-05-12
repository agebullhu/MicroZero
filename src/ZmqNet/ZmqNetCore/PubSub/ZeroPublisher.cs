using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    public class ZeroPublisher
    {
        /// <summary>
        ///     保持长连接的连接池
        /// </summary>
        private static readonly Dictionary<string, RequestSocket> Publishers =
            new Dictionary<string, RequestSocket>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 请求队列
        /// </summary>
        private static readonly SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _state = -1;
        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private static string CacheFileName => Path.Combine(StationProgram.Config.DataFolder,
            "zero_publish_queue.json");
        /// <summary>
        ///     启动
        /// </summary>
        public static void Initialize()
        {
            _state = 0;
            var old = MulitToOneQueue<PublishItem>.Load(CacheFileName);
            if (old == null)
                return;
            foreach (var val in old.Queue)
                Items.Push(val);
        }
        /// <summary>
        ///     启动
        /// </summary>
        public static void Start()
        {
            Task.Factory.StartNew(SendTask);
        }

        /// <summary>
        ///     结束
        /// </summary>
        public static void Stop()
        {
            _state = 3;
            while (_state == 4)
                Thread.Sleep(3);
            _state = 5;
            Thread.Sleep(3);
            Items.Save(CacheFileName);
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void Publish(string station, string title, string sub, string value)
        {
            if (_state != 5)
                Items.Push(new PublishItem
                {
                    Station = station,
                    Title = title,
                    SubTitle = sub,
                    Content = value ?? "{}"
                });
        }
        /// <summary>
        /// 广播总数
        /// </summary>
        public static ulong PubCount { get;private set; }
        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private static void SendTask()
        {
            
            _state = 2;
            DateTime start = DateTime.Now;
            RequestSocket socket = null;
            while (_state == 2)
            {
                if (!Items.StartProcess(out var item, 100))
                    continue;
                if (socket == null )
                {
                    if (!GetSocket(item.Station, out socket, out var status))
                    {
                        if (status == ZeroCommandStatus.NoFind)
                        {
                            Thread.Sleep(50);
                            continue;
                        }
                        LogRecorder.Trace(LogType.Error, "Publish",
                            $@"因为无法找到站点而导致向【{item.Station}】广播的主题为【{item.Title}】的消息被遗弃，内容为：
{item.Content}");
                        Items.EndProcess();
                        continue;
                    }
                }
                if (!Send(socket, item))
                {
                    LogRecorder.Trace(LogType.Warning, "Publish",
                        $@"向【{item.Station}】广播的主题为【{item.Title}】的消息发送失败，内容为：
{item.Content}");
                    continue;
                }

                Items.EndProcess();
                PubCount++;
            }
            _state = 4;
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="item"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool Send(RequestSocket socket, PublishItem item)
        {
            byte[] description = new byte[5];
            description[0] = 3;
            description[1] = ZeroFrameType.Publisher;
            description[2] = ZeroFrameType.SubTitle;
            description[3] = ZeroFrameType.Argument;
            description[4] = ZeroFrameType.End;
            try
            {
                lock (socket)
                {
                    socket.SendMoreFrame(item.Title);
                    socket.SendMoreFrame(description);
                    socket.SendMoreFrame(StationProgram.Config.StationName);
                    socket.SendMoreFrame(item.SubTitle);
                    socket.SendFrame(item.Content);
                    var word = socket.ReceiveFrameString();
                    return word == ZeroNetStatus.ZeroCommandOk;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteError($"【{item.Station}-{item.Title}】request error =>{e.Message}");
            }

            lock (Publishers)
            {
                Publishers.Remove(item.Station);
            }

            return false;
        }

        //static HashSet<string> registing = new HashSet<string>();

        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="socket"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static bool GetSocket(string type, out RequestSocket socket, out ZeroCommandStatus status)
        {
            //lock (registing)
            //{
            //    if (registing.Contains(type))
            //    {
            //        socket = null;
            //        status = ZeroCommandStatus.NoFind;
            //        return false;
            //    }
            //}
            try
            {
                lock (Publishers)
                {
                    if (Publishers.TryGetValue(type, out socket))
                    {
                        status = ZeroCommandStatus.Success;
                        return true;
                    }
                    var config = StationProgram.GetConfig(type,out status);
                    if (status == ZeroCommandStatus.NoFind)
                    {
                        StationProgram.WriteError($"【{type}】 => 不存在");
                        //lock (registing)
                        //{
                        //    registing.Add(type);
                        //}
                        //config = StationProgram.InstallStation(type, "pub");
                        //lock (registing)
                        //{
                        //    registing.Remove(type);
                        //}
                    }
                    else if (config == null)
                    {
                        StationProgram.WriteError($"【{type}】connect error =>无法拉取配置");
                        return false;
                    }

                    socket = new RequestSocket();
                    socket.Options.Identity =
                        StationProgram.Config.StationName.ToAsciiBytes(); //RandomOperate.Generate(8).ToAsciiBytes();
                    socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                    socket.Connect(config.OutAddress);
                    Publishers.Add(type, socket);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteError($"【{type}】connect error =>连接时发生异常：{e}");
                socket = null;
                status = ZeroCommandStatus.Exception;
                return false;
            }

            return true;
        }
    }
}