using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
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
        private static readonly Dictionary<string, KeyValuePair<StationConfig, RequestSocket>> Publishers =
            new Dictionary<string, KeyValuePair<StationConfig, RequestSocket>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 请求队列
        /// </summary>
        private static readonly SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();

        /// <summary>
        ///     运行状态
        /// </summary>
        public static StationState RunState { get; private set; }
        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private static string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder,
            "zero_publish_queue.json");
        /// <summary>
        ///     启动
        /// </summary>
        public static void Initialize()
        {
            RunState = StationState.Start;
            try
            {
                var old = MulitToOneQueue<PublishItem>.Load(CacheFileName);
                if (old == null)
                    return;
                foreach (var val in old.Queue)
                    Items.Push(val);
            }
            catch
            {
            }
        }
        /// <summary>
        ///     启动
        /// </summary>
        internal static void Start()
        {
            Task.Factory.StartNew(SendTask);
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public static bool Stop()
        {
            lock (Publishers)
            {
                foreach (var vl in Publishers.Values)
                    vl.Value.CloseSocket(vl.Key.RequestAddress);
                Publishers.Clear();
            }
            //未运行状态
            if (RunState < StationState.Start || RunState > StationState.Failed)
                return true;
            StationConsole.WriteInfo("Publisher closing....");
            RunState = StationState.Closing;
            do
            {
                Thread.Sleep(20);
            } while (RunState != StationState.Closed);

            StationConsole.WriteInfo("Publisher closed");
            return true;
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
            Items.Push(new PublishItem
            {
                Station = station,
                Title = title,
                SubTitle = sub,
                RequestId = ApiContext.RequestContext.RequestId,
                Content = value ?? "{}"
            });
            if (RunState == StationState.Closed)
                Items.Save(CacheFileName);
        }
        /// <summary>
        /// 广播总数
        /// </summary>
        public static ulong PubCount { get; private set; }
        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private static void SendTask()
        {
            RunState = StationState.Run;
            while (ZeroApplication.State < StationState.Closing && RunState == StationState.Run)
            {
                if (ZeroApplication.State != StationState.Run)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Items.StartProcess(out var item, 100))
                    continue;

                if (!ZeroApplication.Configs.TryGetValue(item.Station, out var config))
                {
                    LogRecorder.Trace(LogType.Error, "Publish",
                        $@"因为无法找到站点而导致向[{item.Station}]广播的主题为[{item.Title}]的消息被遗弃，内容为：
{item.Content}");
                    Items.EndProcess();
                    continue;
                }
                var socket = config.GetSocket();
                if (socket == null)
                {
                    Thread.Sleep(100);
                    continue;
                }
                try
                {
                    if (!socket.Publish(item))
                    {
                        LogRecorder.Trace(LogType.Warning, "Publish",
                            $@"向[{item.Station}]广播的主题为[{item.Title}]的消息发送失败，内容为：
{item.Content}");
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    StationConsole.WriteError($"[{item.Station}-{item.Title}]request error =>{e.Message}");
                    config.Close(socket);
                    socket = null;
                }
                finally
                {
                    config.Free(socket);
                }
                Items.EndProcess();
                PubCount++;
            }
            RunState = StationState.Closed;
        }


    }
}