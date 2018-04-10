using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private static MulitToOneQueue<PublishItem> _items = new MulitToOneQueue<PublishItem>();

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
                _items.Push(val);
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
            _items.Save(CacheFileName);
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
                _items.Push(new PublishItem
                {
                    Station = station,
                    Title = title,
                    SubTitle = sub,
                    Content = value ?? "{}"
                });
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private static void SendTask()
        {
            _state = 2;
            while (_state == 2)
            {
                if (!_items.StartProcess(out var item, 100))
                    continue;
                if (!GetSocket(item.Station, out var socket))
                {
                    LogRecorder.Trace(LogType.Error, "Publish",
                        $@"因为无法找到站点而导致向【{item.Station}】广播的主题为【{item.Title}】的消息被遗弃，内容为：
{item.Content}");
                    _items.EndProcess();
                    continue;
                }
                if (!Send(socket, item))
                {
                    LogRecorder.Trace(LogType.Warning, "Publish",
                        $@"向【{item.Station}】广播的主题为【{item.Title}】的消息发送失败，内容为：
{item.Content}");
                    continue;
                }
                _items.EndProcess();
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
            try
            {
                lock (socket)
                {
                    byte[] description = new byte[5];
                    description[0] = (byte)3;
                    description[1] = ZeroHelper.zero_pub_publisher;
                    description[2] = ZeroHelper.zero_pub_sub;
                    description[3] = ZeroHelper.zero_arg;
                    description[4] = ZeroHelper.zero_end;
                    socket.SendMoreFrame(item.Title);
                    socket.SendMoreFrame(description);
                    socket.SendMoreFrame(StationProgram.Config.StationName);
                    socket.SendMoreFrame(item.SubTitle);
                    socket.SendFrame(item.Content);
                    var word = socket.ReceiveFrameString();
                    //StationProgram.WriteLine($"【{item.Station}-{item.Title}】{word}");
                    return word == "ok";
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

        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool GetSocket(string type, out RequestSocket socket)
        {
            try
            {
                lock (Publishers)
                {
                    if (Publishers.TryGetValue(type, out socket))
                        return true;
                    var config = StationProgram.GetConfig(type);
                    if (config == null)
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
                return false;
            }

            return true;
        }
    }
}